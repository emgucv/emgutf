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

public class MobilenetBehavior : MonoBehaviour
{
    private WebCamTexture _webcamTexture;
    private Quaternion baseRotation;
    private bool _staticViewRendered = false;
    private Mobilenet _mobilenet = null;
    public Text DisplayText;
    private String _displayMessage = String.Empty;

    // Use this for initialization
    IEnumerator Start()
    {
        bool tryUseCamera = true;
        bool loaded = Emgu.TF.Lite.TfLiteInvoke.CheckLibraryLoaded();

        _mobilenet = new Emgu.TF.Lite.Models.Mobilenet();

        WebCamDevice[] devices = WebCamTexture.devices;
        
        if (tryUseCamera && devices.Length != 0)
        {
            yield return Application.RequestUserAuthorization(UserAuthorization.WebCam);
            if (Application.HasUserAuthorization(UserAuthorization.WebCam))
            {
                UnityEngine.Debug.Log("webcam use authorized");
                _webcamTexture = new WebCamTexture(devices[0].name);

                RawImage image = this.GetComponent<RawImage>();
                image.texture = _webcamTexture;

                _webcamTexture.Play();
            }
        }
        DisplayText.text = "Downloading model, please wait...";
        StartCoroutine(_mobilenet.Init());
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
            _displayMessage = String.Format("Downloading Inception model files, {0} % of file {1}...", _mobilenet.DownloadProgress*100, _mobilenet.DownloadFileName);
        }
        else if (_webcamTexture != null)
        {
            if (_webcamTexture.didUpdateThisFrame)
            {
                transform.rotation = baseRotation * Quaternion.AngleAxis(_webcamTexture.videoRotationAngle, Vector3.up);

                RecognizeAndUpdateText(_webcamTexture);
                RawImage image = this.GetComponent<RawImage>();
                var rectTransform = image.rectTransform;
                rectTransform.sizeDelta = new Vector2(_webcamTexture.width, _webcamTexture.height);
                rectTransform.position = new Vector3(-_webcamTexture.width / 2, -_webcamTexture.height / 2);
                rectTransform.anchoredPosition = new Vector2(0, 0);

                float scaleY = _webcamTexture.videoVerticallyMirrored ? -1.0f : 1.0f;
                rectTransform.localScale = new Vector3(1.0f, scaleY, 1.0f);

                int orient = -_webcamTexture.videoRotationAngle;
                rectTransform.localEulerAngles = new Vector3(0, 0, orient);

            }
            else
            {
                _displayMessage = "_webcamTexture has not been updated.";
            }
        }
        else if (!_staticViewRendered)
        {
            UnityEngine.Debug.Log("Reading texture for recognition");
            Texture2D texture = Resources.Load<Texture2D>("grace_hopper");
            UnityEngine.Debug.Log("Starting recognition");
            RecognizeAndUpdateText(texture);
            UnityEngine.Debug.Log("Rendering result");
            RenderTexture(texture);

            _staticViewRendered = true;
        }

        DisplayText.text = _displayMessage;
    }
    private void RecognizeAndUpdateText(Texture texture)
    {
        if (_mobilenet == null)
        {
            _displayMessage = "Waiting for mobile net model to be loaded...";
            return;
        }

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


    private void onDownloadProgressChanged(object sender, System.Net.DownloadProgressChangedEventArgs e)
    {
        _displayMessage = String.Format("{0} of {1} downloaded ({2}%)", e.BytesReceived, e.TotalBytesToReceive, e.ProgressPercentage);
    }

    private void RenderTexture(Texture texture)
    {
        RawImage image = this.GetComponent<RawImage>();
        image.texture = texture;
        var rectTransform = image.rectTransform;
        rectTransform.sizeDelta = new Vector2(texture.width, texture.height);
        rectTransform.position = new Vector3(-texture.width / 2, -texture.height / 2);
        rectTransform.anchoredPosition = new Vector2(0, 0);
    }
}
