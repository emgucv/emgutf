//----------------------------------------------------------------------------
//  Copyright (C) 2004-2020 by EMGU Corporation. All rights reserved.       
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
        private Mobilenet _mobilenet;

        public MobilenetPage()
           : base()
        {

            var button = this.TopButton;
            button.Text = "Perform Image Classification";
            button.Clicked += OnButtonClicked;

            _mobilenet = new Mobilenet();
            _mobilenet.OnDownloadProgressChanged += onDownloadProgressChanged;

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
            SetMessage("Please wait while the Mobilenet Model is being downloaded...");
            await _mobilenet.Init();

            SetImage();
            String[] imageFiles = await LoadImages(new string[] { "space_shuttle.jpg" });
            if (imageFiles == null || (imageFiles.Length > 0 && imageFiles[0] == null))
            {
                SetMessage("");
                return;
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
