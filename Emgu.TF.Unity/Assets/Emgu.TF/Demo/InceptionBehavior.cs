//----------------------------------------------------------------------------
//  Copyright (C) 2004-2023 by EMGU Corporation. All rights reserved.       
//----------------------------------------------------------------------------

using UnityEngine;
using System;

using System.Collections;
using System.Net;
using System.Net.Security;
using System.Text;
using System.Runtime.InteropServices;
using Emgu.TF;
using Emgu.TF.Models;
using UnityEngine.UI;
using Emgu.Models;

public class InceptionBehavior : MonoBehaviour
{
    private WebCamTexture _webcamTexture;
    
    private Color32[] _data;
    
    private WebCamDevice[] _devices;
    public int _cameraCount = 0;
    private bool _textureResized = false;
    private Quaternion baseRotation;
    private bool _liveCameraView = false;
    private bool _staticViewRendered = false;

    public Text DisplayText;

    private Inception _inceptionGraph = null;

    private void RecognizeAndUpdateText(Texture2D texture)
    {
        Color32[] colors = texture.GetPixels32(); //32bit RGBA
        RecognizeAndUpdateText(colors, texture.width, texture.height);
        /*
        if (_inceptionGraph == null)
            return;
        if (!_inceptionGraph.Imported)
            return;
        Tensor imageTensor = ImageIO.ReadTensorFromTexture2D(texture, 224, 224, 128.0f, 1.0f, true);
        Inception.RecognitionResult[][] results = _inceptionGraph.Recognize(imageTensor);
        _displayMessage = String.Format("Object is {0} with {1}% probability.", results[0][0].Label, results[0][0].Probability*100);*/
    }

    private void RecognizeAndUpdateText(Color32[] pixels, int width, int height)
    {
        if (_inceptionGraph == null)
            return;
        if (!_inceptionGraph.Imported)
            return;
        Inception.RecognitionResult[][] results;
        using (Tensor imageTensor = ImageIO.ReadTensorFromColor32(pixels, width, height, 224, 224, 128.0f, 1.0f, true))
        {
            results = _inceptionGraph.Recognize(imageTensor);
        }
        _displayMessage = String.Format(
            "Object is {0} with {1}% probability.", 
            results[0][0].Label,
            results[0][0].Probability * 100);
    }


    // Use this for initialization
    void Start()
    {
        bool loaded = TfInvoke.Init();
        _inceptionGraph = new Inception();

        //Change the following flag to set default detection based on image / live camera view
        _liveCameraView = false;

        if (_liveCameraView)
        {
            _devices = WebCamTexture.devices;
            _cameraCount = _devices.Length;

            if (_cameraCount == 0)
            {
                _liveCameraView = false;
            }
            else
            {
                _liveCameraView = true;
                _webcamTexture = new WebCamTexture(_devices[0].name);

                baseRotation = transform.rotation;
                _webcamTexture.Play();
                //data = new Color32[webcamTexture.width * webcamTexture.height];
            }
        }

        StartCoroutine(_inceptionGraph.Init());
    }

    private String _displayMessage = String.Empty;

    // Update is called once per frame
    void Update()
    {

        if (!_inceptionGraph.Imported)
        {
            _displayMessage = String.Format("Downloading Inception model files, {0} % of file {1}...", _inceptionGraph.DownloadProgress*100, _inceptionGraph.DownloadFileName);
        }
        else if (_liveCameraView)
        {
            if (_webcamTexture != null && _webcamTexture.didUpdateThisFrame)
            {
                #region convert the webcam texture to RGBA bytes

                if (_data == null || (_data.Length != _webcamTexture.width * _webcamTexture.height))
                {
                    _data = new Color32[_webcamTexture.width * _webcamTexture.height];
                }
                _webcamTexture.GetPixels32(_data);
                #endregion
                
                if (!_textureResized)
                {
                    ResizeTexture(_webcamTexture);
                    _textureResized = true;
                }
                
                transform.rotation = baseRotation * Quaternion.AngleAxis(_webcamTexture.videoRotationAngle, Vector3.up);

                
                RecognizeAndUpdateText(_data, _webcamTexture.width, _webcamTexture.height);

                RenderTexture(_webcamTexture);
                
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
        }

        DisplayText.text = _displayMessage;
    }

    private void RenderTexture(Texture texture)
    {
        RawImage image = this.GetComponent<RawImage>();
        image.texture = texture;
        //image.sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
    }

    private void ResizeTexture(Texture texture)
    {
        RawImage image = this.GetComponent<RawImage>();
        var transform = image.rectTransform;
        transform.sizeDelta = new Vector2(texture.width, texture.height);
        transform.position = new Vector3(-texture.width / 2, -texture.height / 2);
        transform.anchoredPosition = new Vector2(0, 0);
    }
}
