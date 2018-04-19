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
using Emgu.TF.Lite.Models;
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
        private Mobilenet _model;

        private string[] _image = null;

        public MobilenetPage()
           : base()
        {
            
            var button = this.GetButton();
            button.Text = "Perform Image Recognition";
            button.Clicked += OnButtonClicked;

            OnImagesLoaded += async (sender, image) =>
            {
                SetMessage("Please wait...");
                SetImage();
                _image = image;

                try
                {
                    SetMessage("Please wait while we download the Mobilenet Model from internet.");
                    _model = new Mobilenet();
                    _model.OnDownloadProgressChanged += onDownloadProgressChanged;
                    _model.OnDownloadCompleted += onDownloadCompleted;
                    _model.Download();           
                }
                catch (Exception e)
                {
                    String msg = e.Message.Replace(System.Environment.NewLine, " ");
                    SetMessage(msg);
                           
                }
            };
        }

        private void onDownloadProgressChanged(object sender, System.Net.DownloadProgressChangedEventArgs e)
        {
            SetMessage(String.Format("{0} of {1} downloaded ({2}%)", e.BytesReceived, e.TotalBytesToReceive, e.ProgressPercentage));
        }

        private void onDownloadCompleted(object sender, System.ComponentModel.AsyncCompletedEventArgs e)
        {
            String localFileName = DownloadableModels.GetLocalFileName(_model._modelFiles[0]);
            System.Diagnostics.Debug.Assert(File.Exists(localFileName), "File doesn't exist");
            FileInfo file = new FileInfo(localFileName);

            using (FlatBufferModel model = new FlatBufferModel(localFileName))
            using (BuildinOpResolver resolver = new BuildinOpResolver())
            using (Interpreter interpreter = new Interpreter(model, resolver))
            {
                
                bool check = model.CheckModelIdentifier();

                int[] input = interpreter.GetInput();
                int[] output = interpreter.GetOutput();
                String inputName = interpreter.GetInputName(input[0]);
                String outputName = interpreter.GetOutputName(output[0]);

                Status allocateTensorStatus = interpreter.AllocateTensors();

                Tensor inputTensor = interpreter.GetTensor(input[0]);
                Tensor outputTensor = interpreter.GetTensor(output[0]);

                Emgu.TF.Lite.Models.ImageIO.ReadImageFileToTensor(_image[0], inputTensor.DataPointer, 224, 224, 128.0f, 1.0f / 128.0f);

                interpreter.Invoke();
                float[] probability = outputTensor.GetData() as float[];
                string[] labels = _model.Labels;

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
                    resStr = String.Format("Object is {0} with {1}% probability.", labels[maxIdx], maxVal * 100);
                }
                //return new Tuple<string, string, long>(_image[0], resStr, 0);

                SetImage(_image[0]);
                SetMessage(resStr);
                
            }
        }

        private void OnButtonClicked(Object sender, EventArgs args)
        {
            LoadImages(new string[] { "grace_hopper.jpg" });
        }

    }
}
