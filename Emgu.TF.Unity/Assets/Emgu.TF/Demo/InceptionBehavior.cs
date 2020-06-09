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
using Emgu.TF;
using Emgu.TF.Models;
using UnityEngine.UI;
using Emgu.Models;

public class InceptionBehavior : MonoBehaviour
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

    public Text DisplayText;

    private Inception _inceptionGraph = null;

    private void RecognizeAndUpdateText(Texture2D texture)
    {
        if (!_inceptionGraph.Imported)
            return;
        Tensor imageTensor = ImageIO.ReadTensorFromTexture2D(texture, 224, 224, 128.0f, 1.0f, true);
        Inception.RecognitionResult[] results = _inceptionGraph.Recognize(imageTensor);
        _displayMessage = String.Format("Object is {0} with {1}% probability.", results[0].Label, results[0].Probability*100);
    }


    // Use this for initialization
    void Start()
    {
        bool loaded = TfInvoke.CheckLibraryLoaded();
        _inceptionGraph = new Inception();
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

            Texture2D texture = Resources.Load<Texture2D>("space_shuttle");
            UnityEngine.Debug.Log("Starting recognition");

            RecognizeAndUpdateText(texture);

            UnityEngine.Debug.Log("Rendering result");

            RenderTexture(texture);
            ResizeTexture(texture);
            /*
            Image image = this.GetComponent<Image>();
            image.sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
            var transform = image.rectTransform;
            transform.sizeDelta = new Vector2(texture.width, texture.height);
            transform.position = new Vector3(-texture.width / 2, -texture.height / 2);
            transform.anchoredPosition = new Vector2(0, 0);            //this.GetComponent<Image>().pixelInset = new Rect(-texture.width / 2, -texture.height / 2, texture.width, texture.height);
            */
            _staticViewRendered = true;
            //DisplayText.text = _displayMessage;
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
