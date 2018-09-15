//----------------------------------------------------------------------------
//  Copyright (C) 2004-2018 by EMGU Corporation. All rights reserved.       
//----------------------------------------------------------------------------

using UnityEngine;
using System;

using System.Collections;
using System.Net;
using System.Net.Security;
using System.Text;
using System.Runtime.InteropServices;
using System.Security.Cryptography.X509Certificates;
using Emgu.TF.Lite;
using UnityEngine.UI;
using Emgu.Models;
using System.IO;
using System.Diagnostics;

public class MobilenetBehavior : MonoBehaviour
{
    private WebCamTexture webcamTexture;
    private Texture2D resultTexture;
    private Color32[] data;
    private byte[] bytes;
    private WebCamDevice[] devices;
    public int cameraCount = 0;
    private bool _textureResized = false;
    private Quaternion baseRotation;
    private bool _liveCameraView = false;
    private bool _staticViewRendered = false;


    private FileDownloadManager _downloadManager;
    private String[] _labels = null;
    private FlatBufferModel _model = null;
    private BuildinOpResolver _resolver = null;
    private Interpreter _interpreter = null;

    public Text DisplayText;

    //private Inception _inceptionGraph = null;

    public static Texture2D Resize(Texture2D source, int newWidth, int newHeight)
    {
        source.filterMode = FilterMode.Bilinear;
        UnityEngine.RenderTexture rt = UnityEngine.RenderTexture.GetTemporary(newWidth, newHeight);
        rt.filterMode = FilterMode.Bilinear;

        UnityEngine.RenderTexture.active = rt;
        Graphics.Blit(source, rt);
        var nTex = new Texture2D(newWidth, newHeight);
        nTex.ReadPixels(new Rect(0, 0, newWidth, newHeight), 0, 0);
        nTex.Apply();
        UnityEngine.RenderTexture.active = null;
        UnityEngine.RenderTexture.ReleaseTemporary(rt);
        return nTex;
    }

    public static Tensor ReadTensorFromTexture2D(
        Tensor t, Texture2D texture, int inputHeight = -1, int inputWidth = -1,
        float inputMean = 0.0f, float scale = 1.0f, bool flipUpsideDown = false)
    {
        Color32[] colors;

        int width, height;
        if (inputHeight > 0 || inputWidth > 0)
        {
            Texture2D small = Resize(texture, inputWidth, inputHeight);
            colors = small.GetPixels32();
            width = inputWidth;
            height = inputHeight;
        }
        else
        {
            width = texture.width;
            height = texture.height;
            colors = texture.GetPixels32();
        }
        //t = new Tensor(DataType.Float, new int[] { 1, height, width, 3 });

        float[] floatValues = new float[colors.Length*3];

        if (flipUpsideDown)
        {
            for (int i = 0; i < height; i++)
            for (int j = 0; j < width; j++)
            {
                Color32 val = colors[(height - i - 1) * width + j];
                int idx = i * width + j;
                floatValues[idx * 3 + 0] = (val.r - inputMean) * scale;
                floatValues[idx * 3 + 1] = (val.g - inputMean) * scale;
                floatValues[idx * 3 + 2] = (val.b - inputMean) * scale;
            }
        }
        else
        {
            for (int i = 0; i < colors.Length; ++i)
            {
                Color32 val = colors[i];
                floatValues[i * 3 + 0] = (val.r - inputMean) * scale;
                floatValues[i * 3 + 1] = (val.g - inputMean) * scale;
                floatValues[i * 3 + 2] = (val.b - inputMean) * scale;
            }
        }


        System.Runtime.InteropServices.Marshal.Copy(floatValues, 0, t.DataPointer, floatValues.Length);

        return t;
    }

    private void RecognizeAndUpdateText(Texture2D texture)
    {
        int[] input = _interpreter.GetInput();
        int[] output = _interpreter.GetOutput();

        Tensor inputTensor = _interpreter.GetTensor(input[0]);
        Tensor outputTensor = _interpreter.GetTensor(output[0]);

        ReadTensorFromTexture2D(inputTensor, texture, 224, 224, 128.0f, 1.0f / 128.0f);
        Stopwatch watch = Stopwatch.StartNew();
        _interpreter.Invoke();
        watch.Stop();

        float[] probability = outputTensor.GetData() as float[];

        String resStr = String.Empty;
        if (probability != null)
        {
            float maxVal = 0;
            int maxIdx = 0;
            for (int i = 0; i < probability.Length; i++)
            {
                if (probability[i] > maxVal)
                {
                    maxVal = probability[i];
                    maxIdx = i;
                }
            }
            resStr = String.Format("Object is {0} with {1}% probability. Recognition completed in {2} milliseconds.", _labels[maxIdx], maxVal * 100, watch.ElapsedMilliseconds);
        }

        //SetImage(_image[0]);
        _displayMessage = resStr;

    }


    // Use this for initialization
    void Start()
    {
        _downloadManager = new FileDownloadManager();
        String downloadUrl = "https://github.com/emgucv/models/raw/master/mobilenet_v1_1.0_224_float_2017_11_08/";
        String[] fileNames = new string[] { "mobilenet_v1_1.0_224.tflite", "labels.txt" };

        for (int i = 0; i < fileNames.Length; i++)
        {
            _downloadManager.AddFile(downloadUrl + fileNames[i]);
        }

        _downloadManager.OnDownloadProgressChanged += onDownloadProgressChanged;
        _downloadManager.OnDownloadCompleted += onDownloadCompleted;
        _downloadManager.Download();

        //bool loaded = TfInvoke.CheckLibraryLoaded();
        //_inceptionGraph = new Inception();
        _liveCameraView = false;
        /*
        WebCamDevice[] devices = WebCamTexture.devices;
        cameraCount = devices.Length;

        if (cameraCount == 0)
        {
            _liveCameraView = false;
        }
        else
        {
            _liveCameraView = true;
            webcamTexture = new WebCamTexture(devices[0].name);

            baseRotation = transform.rotation;
            webcamTexture.Play();
            //data = new Color32[webcamTexture.width * webcamTexture.height];
        }*/

        //StartCoroutine(_inceptionGraph.Init());
    }

    private String _displayMessage = String.Empty;


    private void onDownloadProgressChanged(object sender, System.Net.DownloadProgressChangedEventArgs e)
    {
        _displayMessage = String.Format("{0} of {1} downloaded ({2}%)", e.BytesReceived, e.TotalBytesToReceive, e.ProgressPercentage);
    }

    private void onDownloadCompleted(object sender, System.ComponentModel.AsyncCompletedEventArgs e)
    {
        String localFileName = _downloadManager.Files[0].LocalFile;
        if (_labels == null)
            _labels = File.ReadAllLines(_downloadManager.Files[1].LocalFile);

        System.Diagnostics.Debug.Assert(File.Exists(localFileName), "File doesn't exist");
        FileInfo file = new FileInfo(localFileName);

        if (_model == null)
        {
            _model = new FlatBufferModel(localFileName);
            if (!_model.CheckModelIdentifier())
                throw new Exception("Model indentifier check failed");
        }

        if (_resolver == null)
            _resolver = new BuildinOpResolver();

        if (_interpreter == null)
        {
            _interpreter = new Interpreter(_model, _resolver);
            Status allocateTensorStatus = _interpreter.AllocateTensors();
            if (allocateTensorStatus == Status.Error)
                throw new Exception("Failed to allocate tensor");
        }



    }

    // Update is called once per frame
    void Update()
    {

        if (_interpreter == null)
        {
            //_displayMessage = String.Format("Downloading Inception model files, {0} % of file {1}...", _inceptionGraph.DownloadProgress*100, _inceptionGraph.DownloadFileName);
        }
        else if (_liveCameraView)
        {
            if (webcamTexture != null && webcamTexture.didUpdateThisFrame)
            {
                #region convert the webcam texture to RGBA bytes

                if (data == null || (data.Length != webcamTexture.width * webcamTexture.height))
                {
                    data = new Color32[webcamTexture.width * webcamTexture.height];
                }
                webcamTexture.GetPixels32(data);

                if (bytes == null || bytes.Length != data.Length * 4)
                {
                    bytes = new byte[data.Length * 4];
                }
                GCHandle handle = GCHandle.Alloc(data, GCHandleType.Pinned);
                Marshal.Copy(handle.AddrOfPinnedObject(), bytes, 0, bytes.Length);
                handle.Free();

                #endregion

                #region convert the RGBA bytes to texture2D

                if (resultTexture == null || resultTexture.width != webcamTexture.width ||
                    resultTexture.height != webcamTexture.height)
                {
                    resultTexture = new Texture2D(webcamTexture.width, webcamTexture.height, TextureFormat.RGBA32,
                        false);
                }

                resultTexture.LoadRawTextureData(bytes);
                resultTexture.Apply();

                #endregion

                if (!_textureResized)
                {
                    ResizeTexture(resultTexture);
                    _textureResized = true;
                }

                transform.rotation = baseRotation * Quaternion.AngleAxis(webcamTexture.videoRotationAngle, Vector3.up);

                RecognizeAndUpdateText(resultTexture);

                RenderTexture( resultTexture );
                //count++;

            }
            //DisplayText.text = _displayMessage;
        }
        else if (!_staticViewRendered)
        {
            UnityEngine.Debug.Log("Reading texture for recognition");

            Texture2D texture = Resources.Load<Texture2D>("space_shuttle");
            UnityEngine.Debug.Log("Starting recognition");

            RecognizeAndUpdateText(texture);

            UnityEngine.Debug.Log("Rendering result");

            RenderTexture(texture);
            ResizeTexture(texture);

            _staticViewRendered = true;
            //DisplayText.text = _displayMessage;
        }

        DisplayText.text = _displayMessage;
    }


    private void RenderTexture(Texture2D texture)
    {
        Image image = this.GetComponent<Image>();
        image.sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
    }

    private void ResizeTexture(Texture2D texture)
    {
        Image image = this.GetComponent<Image>();
        var transform = image.rectTransform;
        transform.sizeDelta = new Vector2(texture.width, texture.height);
        transform.position = new Vector3(-texture.width / 2, -texture.height / 2);
        transform.anchoredPosition = new Vector2(0, 0);
    }
}
