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
using Emgu.TF.Lite.Models;
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


    private Mobilenet _mobilenet = null;

    public Text DisplayText;

    private void RecognizeAndUpdateText(Texture2D texture)
    {
        Stopwatch watch = Stopwatch.StartNew();
        Mobilenet.RecognitionResult[] results = _mobilenet.Recognize(texture);
        
        watch.Stop();

        String resStr = String.Empty;
        if (results != null)
        {
            resStr = String.Format("Object is {0} with {1}% probability. Recognition completed in {2} milliseconds.", results[0].Label, results[0].Probability * 100, watch.ElapsedMilliseconds);
        }

        _displayMessage = resStr;

    }

    // Use this for initialization
    void Start()
    {
        bool loaded = Emgu.TF.Lite.TfLiteInvoke.CheckLibraryLoaded();

        _mobilenet = new Emgu.TF.Lite.Models.Mobilenet();

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

        if (!_mobilenet.Imported)
        {
            _displayMessage = String.Format("Downloading Inception model files, {0} % of file {1}...", _mobilenet.DownloadProgress*100, _mobilenet.DownloadFileName);
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

            Texture2D texture = Resources.Load<Texture2D>("grace_hopper");
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
