//----------------------------------------------------------------------------
//  Copyright (C) 2004-2018 by EMGU Corporation. All rights reserved.       
//----------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Drawing;
using System.Threading.Tasks;
using System.Diagnostics;
using Emgu.TF.Lite;
using Emgu.Models;
#if __ANDROID__
using Android.App;
using Android.Content;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.OS;
using Android.Graphics;
using Android.Preferences;
#elif __UNIFIED__ && !__IOS__
using AppKit;
using CoreGraphics;
#elif __IOS__
using UIKit;
using CoreGraphics;
#endif

namespace Emgu.TF.XamarinForms
{
    public class MobilenetPage : ButtonTextImagePage
    {
        private FileDownloadManager _downloadManager;
        private String[] _labels = null;
        private FlatBufferModel _model = null;
        private BuildinOpResolver _resolver = null;
        private Interpreter _interpreter = null;
        private string[] _image = null;

        public MobilenetPage()
           : base()
        {

            var button = this.GetButton();
            button.Text = "Perform Image Recognition";
            button.Clicked += OnButtonClicked;

            OnImagesLoaded += (sender, image) =>
            {
                SetMessage("Please wait...");
                SetImage();
                _image = image;

#if !DEBUG
                try
#endif
                {
                    SetMessage("Please wait while the Mobilenet Model is being downloaded...");
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
                }
#if !DEBUG
                catch (Exception e)
                {
                    String msg = e.Message.Replace(System.Environment.NewLine, " ");
                    SetMessage(msg);     
                }
#endif
            };
        }

        private void onDownloadProgressChanged(object sender, System.Net.DownloadProgressChangedEventArgs e)
        {
            if (e.TotalBytesToReceive <= 0)
                SetMessage(String.Format("{0} bytes downloaded ({1}%)", e.BytesReceived, e.ProgressPercentage));
            else
                SetMessage(String.Format("{0} of {1} bytes downloaded ({2}%)", e.BytesReceived, e.TotalBytesToReceive, e.ProgressPercentage));
        }

        private void onDownloadCompleted(object sender, System.ComponentModel.AsyncCompletedEventArgs e)
        {
            String modelFileName = _downloadManager.Files[0].LocalFile;
            String labelFileName = _downloadManager.Files[1].LocalFile;

            System.Diagnostics.Debug.Assert(File.Exists(modelFileName), String.Format("File {0} doesn't exist", modelFileName));
            System.Diagnostics.Debug.Assert(File.Exists(labelFileName), String.Format("File {0} doesn't exist", labelFileName));

            if (!File.Exists(modelFileName) || !File.Exists(labelFileName))
                return;

            //FileInfo file = new FileInfo(modelFileName);

            if (_labels == null)
                _labels = File.ReadAllLines(labelFileName);
            
            if (_model == null)
            {
                _model = new FlatBufferModel(modelFileName);
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

            int[] input = _interpreter.GetInput();
            int[] output = _interpreter.GetOutput();

            Tensor inputTensor = _interpreter.GetTensor(input[0]);
            Tensor outputTensor = _interpreter.GetTensor(output[0]);

            NativeImageIO.ReadImageFileToTensor(_image[0], inputTensor.DataPointer, 224, 224, 128.0f, 1.0f / 128.0f);
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

            SetImage(_image[0]);
            SetMessage(resStr);

        }

        private void OnButtonClicked(Object sender, EventArgs args)
        {
            LoadImages(new string[] { "grace_hopper.jpg" });
        }

    }
}
