//----------------------------------------------------------------------------
//  Copyright (C) 2004-2024 by EMGU Corporation. All rights reserved.       
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

namespace Emgu.TF
{
    public class MultiboxPeopleDetectorBehavior : MonoBehaviour
    {
        private WebCamTexture _webcamTexture;
        private Texture2D _drawableTexture;
        private Color32[] _data;
        private byte[] _bytes;
        private WebCamDevice[] _devices;
        public int _cameraCount = 0;
        private bool _textureResized = false;
        private Quaternion _baseRotation;
        private bool _liveCameraView = false;
        private bool _staticViewRendered = false;
        private MultiboxGraph _multiboxGraph;

        public Text DisplayText;


        // Use this for initialization
        void Start()
        {
            bool loaded = TfInvoke.Init();
            //DisplayText.text = String.Format("Tensorflow library loaded: {0}", loaded);

            //Use the following flag to change detection based on image / camera
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
                    _baseRotation = this.transform.rotation;
                    _webcamTexture.Play();
                }
            }
        }

        private String _displayMessage = String.Empty;

        // Update is called once per frame
        void Update()
        {
            if (_multiboxGraph == null)
            {
                _multiboxGraph = new MultiboxGraph();
                StartCoroutine(_multiboxGraph.Init());
            }
            else if (!_multiboxGraph.Imported)
            {
                _displayMessage = String.Format("Downloading multibox model files, {0} % of file {1}...",
                    _multiboxGraph.DownloadProgress * 100, _multiboxGraph.DownloadFileName);
            }
            else if (_liveCameraView)
            {
                _displayMessage = String.Empty;

                if (_webcamTexture != null && _webcamTexture.didUpdateThisFrame)
                {
                    int webcamWidth = _webcamTexture.width;
                    int webcamHeight = _webcamTexture.height;

                    #region convert the webcam texture to RGBA bytes

                    if (_data == null || (_data.Length != webcamWidth * webcamHeight))
                    {
                        _data = new Color32[webcamWidth * webcamHeight];
                    }

                    _webcamTexture.GetPixels32(_data);

                    if (_bytes == null || _bytes.Length != _data.Length * 4)
                    {
                        _bytes = new byte[_data.Length * 4];
                    }

                    GCHandle handle = GCHandle.Alloc(_data, GCHandleType.Pinned);
                    Marshal.Copy(handle.AddrOfPinnedObject(), _bytes, 0, _bytes.Length);
                    handle.Free();

                    #endregion

                    #region convert the RGBA bytes to texture2D

                    if (_drawableTexture == null || _drawableTexture.width != webcamWidth ||
                        _drawableTexture.height != webcamHeight)
                    {
                        _drawableTexture = new Texture2D(webcamWidth, webcamHeight, TextureFormat.RGBA32,
                            false);
                    }

                    _drawableTexture.LoadRawTextureData(_bytes);
                    _drawableTexture.Apply();

                    #endregion

                    //Tensor imageTensor = ImageIO.ReadTensorFromTexture2D(_drawableTexture, 224, 224, 128.0f, 1.0f/128.0f, true);
                    Tensor imageTensor = ImageIO.ReadTensorFromColor32(
                        _data,
                        webcamWidth,
                        webcamHeight,
                        224,
                        224,
                        128.0f,
                        1.0f / 128.0f,
                        true);
                    MultiboxGraph.Result[] results = _multiboxGraph.Detect(imageTensor);

                    MultiboxGraph.DrawResults(_drawableTexture, results, 0.2f, true);

                    if (!_textureResized)
                    {
                        ResizeTexture(_drawableTexture);
                        _textureResized = true;
                    }

                    this.transform.rotation =
                        _baseRotation * Quaternion.AngleAxis(_webcamTexture.videoRotationAngle, Vector3.up);
                    RenderTexture(_drawableTexture);
                    //count++;

                }
            }
            else if (!_staticViewRendered)
            {
                Texture2D texture = Resources.Load<Texture2D>("surfers");
                Tensor imageTensor = ImageIO.ReadTensorFromTexture2D(texture, 224, 224, 128.0f, 1.0f / 128.0f, true);

                //byte[] raw = ImageIO.EncodeJpeg(imageTensor, 128.0f, 128.0f);
                //System.IO.File.WriteAllBytes("surfers_out.jpg", raw);

                MultiboxGraph.Result[] results = _multiboxGraph.Detect(imageTensor);

                _drawableTexture = new Texture2D(texture.width, texture.height, TextureFormat.ARGB32, false);
                _drawableTexture.SetPixels32(texture.GetPixels32());
                MultiboxGraph.DrawResults(_drawableTexture, results, 0.1f, true);

                RenderTexture(_drawableTexture);
                ResizeTexture(_drawableTexture);

                _displayMessage = String.Empty;
                _staticViewRendered = true;
            }

            DisplayText.text = _displayMessage;
        }

        private void RenderTexture(Texture texture)
        {
            RawImage image = this.GetComponent<RawImage>();
            if (image.texture != texture)
                image.texture = texture;
            //image.sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
        }

        private void ResizeTexture(Texture texture)
        {
            RawImage image = this.GetComponent<RawImage>();
            var iTransform = image.rectTransform;
            iTransform.sizeDelta = new Vector2(texture.width, texture.height);
            iTransform.position = new Vector3(-texture.width / 2, -texture.height / 2);
            iTransform.anchoredPosition = new Vector2(0, 0);
        }
    }
}