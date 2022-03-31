//----------------------------------------------------------------------------
//  Copyright (C) 2004-2022 by EMGU Corporation. All rights reserved.       
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
#elif __IOS__
using UIKit;
using CoreGraphics;
#elif __MACOS__ 
using AppKit;
using CoreGraphics;
#endif

namespace Emgu.TF.XamarinForms
{
    public class MobilenetPage : ButtonTextImagePage
    {
        private Mobilenet _mobilenet;

        public MobilenetPage()
           : base()
        {

            var button = this.TopButton;
            button.Text = "Perform Mobilenet Image Classification";
            button.Clicked += OnButtonClicked;

            var picker = this.Picker;
            picker.Title = "TF Lite backend";


            picker.Items.Add("CPU");

            #if __ANDROID__
            if (TfLiteInvoke.DefaultNnApiDelegate != null)
            {
                picker.Items.Add("NNAPI");
            }
            #endif

            
            if (TfLiteInvoke.DefaultGpuDelegateV2 != null)
            {
                picker.Items.Add("GPU");
            }

            picker.IsVisible = picker.Items.Count > 1;

            picker.SelectedIndexChanged += (sender, args) =>
            {
                if (_mobilenet != null)
                {
                    _mobilenet.Dispose();
                    _mobilenet = null;
                }
            };
        }

        private void onDownloadProgressChanged(object sender, System.Net.DownloadProgressChangedEventArgs e)
        {
            if (e.TotalBytesToReceive <= 0)
                SetMessage(String.Format("{0} bytes downloaded", e.BytesReceived, e.ProgressPercentage));
            else
                SetMessage(String.Format("{0} of {1} bytes downloaded ({2}%)", e.BytesReceived, e.TotalBytesToReceive, e.ProgressPercentage));
        }

        private async void OnButtonClicked(Object sender, EventArgs args)
        {
            SetImage();
            String[] imageFiles = await LoadImages(new string[] { "space_shuttle.jpg" });
            //handle user cancel
            if (imageFiles == null || (imageFiles.Length > 0 && imageFiles[0] == null))
            {
                SetMessage("");
                return;
            }

            SetMessage("Please wait while the Mobilenet Model is being downloaded...");

            if (_mobilenet == null)
            {
                _mobilenet = new Mobilenet();
                _mobilenet.OnDownloadProgressChanged += onDownloadProgressChanged;
            }

            await _mobilenet.Init();

            var picker = this.Picker;
            if (picker.SelectedIndex > 0)
            {
                String selectedBackend = picker.SelectedItem.ToString();
                if (selectedBackend.Equals("NNAPI"))
                {
                    try
                    {
                        Status addNNApiDelegateStatus = _mobilenet.Interpreter.ModifyGraphWithDelegate(TfLiteInvoke.DefaultNnApiDelegate);
                    }
                    catch (Exception e)
                    {
                        System.Diagnostics.Trace.WriteLine(e);
                        throw;
                    }
                    
                } else if (selectedBackend.Equals("GPU"))
                {
                    try
                    {
                        Status addGpuDelegateStatus = _mobilenet.Interpreter.ModifyGraphWithDelegate(TfLiteInvoke.DefaultGpuDelegateV2);
                    }
                    catch (Exception e)
                    {
                        System.Diagnostics.Trace.WriteLine(e);
                        throw;
                    }
                    
                }
            }

            Stopwatch watch = Stopwatch.StartNew();
            var result = _mobilenet.Recognize(imageFiles[0]);
            watch.Stop();
            String resStr = String.Format("Object is {0} with {1}% probability. Recognition completed in {2} milliseconds.", result[0].Label, result[0].Probability * 100, watch.ElapsedMilliseconds);
            SetImage(imageFiles[0]);
            SetMessage(resStr);
        }

    }
}
