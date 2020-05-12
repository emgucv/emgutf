//----------------------------------------------------------------------------
//  Copyright (C) 2004-2020 by EMGU Corporation. All rights reserved.       
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
    private WebCamTexture _webcamTexture;
    private Texture2D _drawableTexture;
    
    private Quaternion baseRotation;
    private bool _staticViewRendered = false;

    private CocoSsdMobilenetV3 _mobilenet = null;

    public Text DisplayText;
    private String _displayMessage = String.Empty;

    private Annotation[] Recognize(Texture texture)
    {
        CocoSsdMobilenet.RecognitionResult[] results = _mobilenet.Recognize(texture, true, false, 0.5f);

        if (results == null)
            return null;

        Annotation[] annotations = new Annotation[results.Length];
        for (int i = 0; i < results.Length; i++)
        {
            Annotation annotation = new Annotation();
            annotation.Rectangle = results[i].Rectangle;
            annotation.Label = String.Format("{0}:({1:0.00}%)", results[i].Label, results[i].Score * 100);
            annotations[i] = annotation;
        }

        return annotations;
    }

    private void DrawToTexture(Texture texture, Annotation[] annotations, Texture2D results)
    {
        if (texture is Texture2D)
        {
            Texture2D t2d = texture as Texture2D;
            _drawableTexture.SetPixels32(t2d.GetPixels32());
        }
        else
        {
            Texture2D tmp = new Texture2D(texture.width, texture.height);
            Graphics.CopyTexture(texture, tmp);
            _drawableTexture.SetPixels32(tmp.GetPixels32());
        }

        foreach (Annotation annotation in annotations)
        {
            float left = annotation.Rectangle[0] * _drawableTexture.width;
            float top = annotation.Rectangle[1] * _drawableTexture.height;
            float right = annotation.Rectangle[2] * _drawableTexture.width;
            float bottom = annotation.Rectangle[3] * _drawableTexture.height;

            Rect scaledLocation = new Rect(left, top, right - left, bottom - top);

            scaledLocation.y = texture.height - scaledLocation.y;
            scaledLocation.height = -scaledLocation.height;

            NativeImageIO.DrawRect(_drawableTexture, scaledLocation, Color.red);
        }
        results.Apply();
    }

    private void RecognizeAndUpdateText(Texture texture)
    {
        if (_mobilenet == null)
        {
            _displayMessage = "Waiting for mobile net model to be loaded...";
            return;
        }
        Stopwatch watch = Stopwatch.StartNew();
        Annotation[] annotations = Recognize(texture);
        watch.Stop();

        if (_drawableTexture == null || _drawableTexture.width != texture.width ||
            _drawableTexture.height != texture.height)
        {
            _drawableTexture = new Texture2D(texture.width, texture.height, TextureFormat.ARGB32, false);
        }
        DrawToTexture(texture, annotations, _drawableTexture);

        String objectNames = String.Empty;
        foreach (Annotation annotation in annotations)
        {
            objectNames = objectNames + annotation.Label + ";";
        }
        if (!String.IsNullOrEmpty(objectNames))
            objectNames = String.Format("({0})", objectNames);
        String resStr = String.Empty;
        if (annotations != null)
        {
            resStr = String.Format("{0} objects detected{1}. Recognition completed in {2} milliseconds.", annotations.Length, objectNames, watch.ElapsedMilliseconds);
        }

        _displayMessage = resStr;
    }

    // Use this for initialization
    void Start()
    {
        bool loaded = Emgu.TF.Lite.TfLiteInvoke.CheckLibraryLoaded();

        _mobilenet = new Emgu.TF.Lite.Models.CocoSsdMobilenetV3();

        WebCamDevice[] devices = WebCamTexture.devices;
        if (false)
        //if (devices.Length != 0)
        {
            _webcamTexture = new WebCamTexture(devices[0].name);
            baseRotation = transform.rotation;
            _webcamTexture.Play();
        }
        DisplayText.text = "Downloading model, please wait...";
        StartCoroutine(_mobilenet.Init());
    }

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
            _displayMessage = String.Format("Downloading model files, {0} % of file {1}...", _mobilenet.DownloadProgress * 100, _mobilenet.DownloadFileName);
        }
        else if (_webcamTexture != null)
        {
            if (_webcamTexture.didUpdateThisFrame)
            {
                RecognizeAndUpdateText(_webcamTexture);
                RenderTexture(_drawableTexture);
                ResizeTexture(_drawableTexture);
                //count++;
            }
            else
            {
                _displayMessage = "_webcamTexture has not been updated.";
            }
        }
        else if (!_staticViewRendered)
        {
            UnityEngine.Debug.Log("Reading texture for recognition");

            Texture2D texture = Resources.Load<Texture2D>("dog416");
            UnityEngine.Debug.Log("Starting recognition");

            RecognizeAndUpdateText(texture);
            UnityEngine.Debug.Log("Rendering...");
            RenderTexture(_drawableTexture);
            ResizeTexture(_drawableTexture);

            _staticViewRendered = true;
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
        var rectTransform = image.rectTransform;
        rectTransform.sizeDelta = new Vector2(texture.width, texture.height);
        rectTransform.position = new Vector3(-texture.width / 2, -texture.height / 2);
        rectTransform.anchoredPosition = new Vector2(0, 0);
    }
}
