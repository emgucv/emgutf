//----------------------------------------------------------------------------
//  Copyright (C) 2004-2020 by EMGU Corporation. All rights reserved.       
//----------------------------------------------------------------------------

/*

using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

using System.Threading.Tasks;
#if __ANDROID__
using Android.Graphics;
#elif __IOS__
using UIKit;
using CoreGraphics;
#elif __UNIFIED__
using AppKit;
using CoreGraphics;
#endif
using Emgu.TF.Lite;
using Emgu.TF.Lite.Models;

using Xamarin.Forms;

namespace Emgu.TF.XamarinForms
{
    public class SmartReplyPage : ContentPage
    {
        private Entry _entry;
        private Label _smartReplyLabel;
        private SmartReply _model;

        public SmartReplyPage()
            : base()
        {

            _entry = new Entry();
            _entry.Placeholder = "Enter text here";
            _smartReplyLabel = new Label();

            _entry.TextChanged += Entry_TextChanged;
            Content = new StackLayout
            {
                VerticalOptions = LayoutOptions.Start,
                Children =
                     {
                           _entry,
                           _smartReplyLabel

                     }
            };

            Task t = new Task(
            () =>
            {
                try
                {
                    SetMessage("Please wait while we download the SmartReply Model from internet.");
                    _model = new SmartReply();

                    //Uncomment the following 2 lines to force re-download of the SmartReply model
                    //String localFileName = DownloadableModels.GetLocalFileName(_model._modelFiles[0]);
                    //File.Delete(localFileName);

                    _model.Init(onDownloadProgressChanged, onDownloadCompleted);
                }
                catch (Exception e)
                {
                    String msg = e.Message.Replace(System.Environment.NewLine, " ");
                    SetMessage(msg);
                }

            });
            t.Start();
        }

        private void onDownloadProgressChanged(object sender, System.Net.DownloadProgressChangedEventArgs e)
        {
            SetMessage(String.Format("{0} of {1} downloaded ({2}%)", e.BytesReceived, e.TotalBytesToReceive, e.ProgressPercentage));
        }

        private Interpreter _interpreter = null;
        private Tensor _inputTensor = null;
        private Tensor _outputTensor = null;

        private void onDownloadCompleted(object sender, System.ComponentModel.AsyncCompletedEventArgs e)
        {
            String localFileName = DownloadableModels.GetLocalFileName(_model._modelFiles[0]);

            using (FlatBufferModel model = new FlatBufferModel(localFileName))
            using (BuildinOpResolver resolver = new BuildinOpResolver())
            {
                _interpreter = new Interpreter(model, resolver);
                bool check = model.CheckModelIdentifier();
                Status allocateTensorStatus = _interpreter.AllocateTensors();

                int[] input = _interpreter.GetInput();
                int[] output = _interpreter.GetOutput();

                String inputName = _interpreter.GetInputName(input[0]);
                String outputName = _interpreter.GetOutputName(output[0]);

                _inputTensor = _interpreter.GetTensor(input[0]);
                _outputTensor = _interpreter.GetTensor(output[0]);

            }
        }

        private void SetMessage(String msg)
        {
            Xamarin.Forms.Device.BeginInvokeOnMainThread(
             () =>
             {
                 _smartReplyLabel.Text = msg;
             }
         );
        }

        private void Entry_TextChanged(object sender, TextChangedEventArgs e)
        {
            String entryText = _entry.Text;
                if (_interpreter != null)
                {
                    using (DynamicBuffer buffer = new DynamicBuffer())
                    {
                        buffer.AddString(entryText);
                        buffer.WriteToTensor(_inputTensor);
                        _interpreter.Invoke();
                        var result = _outputTensor.GetData();

                        SetMessage(String.Format("{0}", entryText.Length));
                    }
                }
        }
    }
}
*/