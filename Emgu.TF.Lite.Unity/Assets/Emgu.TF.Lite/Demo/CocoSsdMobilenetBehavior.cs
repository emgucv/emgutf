//----------------------------------------------------------------------------
//  Copyright (C) 2004-2019 by EMGU Corporation. All rights reserved.       
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
using Emgu.TF.Lite.Models;
using System.IO;
using System.Diagnostics;
using System.Drawing;
using Color = UnityEngine.Color;

public class CocoSsdMobilenetBehavior : MonoBehaviour
{
    private WebCamTexture webcamTexture;
    private Texture2D resultTexture;
    private Texture2D drawableTexture;
    private Color32[] data;
    private byte[] bytes;
    private WebCamDevice[] devices;
    public int cameraCount = 0;
    private bool _textureResized = false;
    private Quaternion baseRotation;
    private bool _liveCameraView = false;
    private bool _staticViewRendered = false;


    private CocoSsdMobilenet _mobilenet = null;

    public Text DisplayText;

    private void RecognizeAndUpdateText(Texture2D texture)
    {
        if (_mobilenet == null)
        {
            _displayMessage = "Waiting for mobile net model to be loaded...";
            return;
        }

        Stopwatch watch = Stopwatch.StartNew();
        CocoSsdMobilenet.RecognitionResult[] results = _mobilenet.Recognize(texture, true, false, 0.5f);
        watch.Stop();

        if (drawableTexture == null || drawableTexture.width != texture.width ||
            drawableTexture.height != texture.height)
            drawableTexture = new Texture2D(texture.width, texture.height, TextureFormat.ARGB32, false);
        drawableTexture.SetPixels(texture.GetPixels());
        NativeImageIO.Annotation[] annotations = new NativeImageIO.Annotation[results.Length];
        for (int i = 0; i < results.Length; i++)
        {
            NativeImageIO.Annotation annotation = new NativeImageIO.Annotation();
            annotation.Rectangle = results[i].Rectangle;
            annotation.Label = String.Format("{0}:({1:0.00}%)", results[i].Label, results[i].Score * 100);
            annotations[i] = annotation;
        }

        String objectNames= String.Empty;
        foreach (NativeImageIO.Annotation annotation in annotations)
        {
            float left = annotation.Rectangle[0] * drawableTexture.width;
            float top = annotation.Rectangle[1] * drawableTexture.height;
            float right = annotation.Rectangle[2] * drawableTexture.width;
            float bottom = annotation.Rectangle[3] * drawableTexture.height;

            Rect scaledLocation = new Rect(left, top, right - left, bottom - top);

            scaledLocation.y = texture.height - scaledLocation.y;
            scaledLocation.height = -scaledLocation.height;
            
            NativeImageIO.DrawRect(drawableTexture, scaledLocation, Color.red);

            objectNames = objectNames + annotation.Label + ";";
        }
        drawableTexture.Apply();
        //MultiboxGraph.DrawResults(drawableTexture, results, 0.2f, true);
        if (!String.IsNullOrEmpty(objectNames))
            objectNames = String.Format("({0})", objectNames);

        String resStr = String.Empty;
        if (results != null)
        {
            resStr = String.Format("{0} objects detected{1}. Recognition completed in {2} milliseconds.", annotations.Length,  objectNames, watch.ElapsedMilliseconds);
            //resStr = String.Format("Object is {0} with {1}% probability. Recognition completed in {2} milliseconds.", results[0].Label, results[0].Probability * 100, watch.ElapsedMilliseconds);
        }

        _displayMessage = resStr;

    }

    // Use this for initialization
    void Start()
    {
        bool loaded = Emgu.TF.Lite.TfLiteInvoke.CheckLibraryLoaded();

        _mobilenet = new Emgu.TF.Lite.Models.CocoSsdMobilenet();

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
        DisplayText.text = "Downloading model, please wait...";
        StartCoroutine(_mobilenet.Init());
    }

    private String _displayMessage = String.Empty;


    private void onDownloadProgressChanged(object sender, System.Net.DownloadProgressChangedEventArgs e)
    {
        _displayMessage = String.Format("{0} of {1} downloaded ({2}%)", e.BytesReceived, e.TotalBytesToReceive, e.ProgressPercentage);
    }

    // Update is called once per frame
    void Update()
    {
        if (_mobilenet == null)
        {
            _displayMessage = String.Format("Pending mobile net initialization");
        }
        else if (!_mobilenet.Imported)
        {
            _displayMessage = String.Format("Downloading Inception model files, {0} % of file {1}...", _mobilenet.DownloadProgress * 100, _mobilenet.DownloadFileName);
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

                RenderTexture(resultTexture);
                //count++;

            }
            //DisplayText.text = _displayMessage;
        }
        else if (!_staticViewRendered)
        {
            UnityEngine.Debug.Log("Reading texture for recognition");

            Texture2D texture = Resources.Load<Texture2D>("dog416");
            UnityEngine.Debug.Log("Starting recognition");

            RecognizeAndUpdateText(texture);

            UnityEngine.Debug.Log("Rendering result");

            RenderTexture(drawableTexture);
            ResizeTexture(drawableTexture);

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
