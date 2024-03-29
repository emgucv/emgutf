﻿//----------------------------------------------------------------------------
//  Copyright (C) 2004-2024 by EMGU Corporation. All rights reserved.       
//----------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Drawing;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Net;
using Emgu.TF;
using Emgu.Models;
using Emgu.TF.Models;
using Tensorflow;

namespace Emgu.TF.XamarinForms
{
    public class ResnetPage
#if __ANDROID__
        : AndroidCameraPage
#else
        : ButtonTextImagePage
#endif
    {

        private Resnet _resnet;

        private async Task Init(FileDownloadManager.DownloadProgressChangedEventHandler onProgressChanged)
        {
            if (_resnet == null)
            {

                SessionOptions so = new SessionOptions();
                Tensorflow.ConfigProto config = new Tensorflow.ConfigProto();
#if DEBUG
                config.LogDevicePlacement = true;
#endif
                if (TfInvoke.IsGoogleCudaEnabled)
                {
                    config.GpuOptions = new Tensorflow.GPUOptions();
                    config.GpuOptions.AllowGrowth = true;
                }
                so.SetConfig(config.ToProtobuf());
                _resnet = new Resnet(null, so);
                _resnet.OnDownloadProgressChanged += onProgressChanged;

                await _resnet.Init();
            }
        }

        private bool _coldSession = true;

        private String _defaultButtonText = "Recognize object";

#if __ANDROID__
        private String _StopCameraButtonText = "Stop Camera";
        private bool _isBusy = false;
#endif

        public ResnetPage()
            : base()
        {
#if __ANDROID__
            //HasCameraOption = true;
            HasCameraOption = false;
#endif

            Title = "Object recognition (Resnet)";

            this.TopButton.Text = _defaultButtonText;

            this.TopButton.Clicked += async (sender, e) =>
            {
#if !DEBUG
                try
#endif
                {
                    this.TopButton.IsEnabled = false;
                    SetMessage("Please wait...");
                    SetImage();

                    SetMessage("Please wait while we download / initialize the model ...");
                    await Init(this.onDownloadProgressChanged);

                    if (_resnet == null || (!_resnet.Imported))
                    {
                        _resnet = null;
                        SetMessage("Failed to import resnet.");
                        return;
                    }

                    SetMessage("Model Loaded.");

                    String[] images;
                    images = await LoadImages(new string[] { "space_shuttle.jpg" });

                    if (images == null)
                    {
                        //User canceled
                        this.TopButton.IsEnabled = true;
                        return;
                    }

                    if (images[0] == "Camera")
                    {
                        //handle camera stream
#if __ANDROID__
                        this.TopButton.Text = _StopCameraButtonText;
                        StartCapture(async delegate (Object s, Android.Graphics.Bitmap m)
                        {
                            //Skip the frame if busy, 
                            //Otherwise too many frames arriving and will eventually saturated the memory.
                            if (!_isBusy)
                            {
                                _isBusy = true;
                                try
                                {
                                    Stopwatch watch = Stopwatch.StartNew();
                                    Resnet.RecognitionResult result;
                                    //await Task.Run(() =>
                                    //{
                                        Tensor imageTensor = new Tensor(DataType.Float, new int[] { 1, 224, 224, 3 });
                                        Emgu.Models.NativeImageIO.ReadBitmapToTensor<float>(m, imageTensor.DataPointer, 224, 224, 0.0f,
                                            1.0f / 255.0f, false, false);
                                        result = _resnet.Recognize(imageTensor)[0][0];
                                    //});
                                    watch.Stop();
                                    SetImage(m);
                                    String msg = String.Format("Object is {0} with {1}% probability. Recognized in {2} milliseconds.",
                                        result.Label, result.Probability * 100, watch.ElapsedMilliseconds);
                                    SetMessage(msg);
                                }
                                finally
                                {
                                    _isBusy = false;
                                }
                            }
                        });
#else
                        throw new NotImplementedException("Camera handling is not implemented");
#endif
                    }
                    else
                    {
                        Tensor imageTensor =
                            Emgu.TF.Models.ImageIO.ReadTensorFromImageFile<float>(images[0], 224, 224, 0.0f,
                                1.0f / 255.0f, false, false);

                        Resnet.RecognitionResult[] result;
                        if (_coldSession)
                        {
                            //First run of the recognition graph, here we will compile the graph and initialize the session
                            //This is expected to take much longer time than consecutive runs.
                            result = _resnet.Recognize(imageTensor)[0];
                            _coldSession = false;
                        }

                        //Here we are trying to time the execution of the graph after it is loaded
                        //If we are not interest in the performance, we can skip the following 3 lines
                        Stopwatch sw = Stopwatch.StartNew();
                        result = _resnet.Recognize(imageTensor)[0];
                        sw.Stop();

                        String msg = String.Format(
                            "Object is {0} with {1}% probability. Recognized in {2} milliseconds.",
                            result[0].Label, result[0].Probability * 100, sw.ElapsedMilliseconds);
                        SetMessage(msg);

#if __ANDROID__
                            var bmp = Emgu.Models.NativeImageIO.ImageFileToBitmap(images[0]);
                            SetImage(bmp);
#else
                        var jpeg = Emgu.Models.NativeImageIO.ImageFileToJpeg(images[0]);
                        SetImage(jpeg.Raw, jpeg.Width, jpeg.Height);
#endif
                        this.TopButton.IsEnabled = true;
                    }

                }
#if !DEBUG
                    catch (Exception excpt)
                    {
                        String msg = excpt.Message.Replace(System.Environment.NewLine, " ");
                        SetMessage(msg);
                    }
#endif
                this.Disappearing += ResnetPage_Disappearing;
            };
        }

        private void ResnetPage_Disappearing(object sender, EventArgs e)
        {
            if (_resnet != null)
            {
                _resnet.Dispose();
                _resnet = null;
            }
        }
    }
}
