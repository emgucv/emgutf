//----------------------------------------------------------------------------
//  Copyright (C) 2004-2019 by EMGU Corporation. All rights reserved.       
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
        private string[] _imageFiles = null;

        public MobilenetPage()
           : base()
        {

            var button = this.TopButton;
            button.Text = "Perform Image Classification";
            button.Clicked += OnButtonClicked;

            _mobilenet = new Mobilenet();

            OnImagesLoaded += (sender, imageFiles) =>
            {
                SetMessage("Please wait...");
                SetImage();
                _imageFiles = imageFiles;

#if !DEBUG
                try
#endif
                {
                    if (_mobilenet.Imported)
                    {
                        onDownloadCompleted(this, new System.ComponentModel.AsyncCompletedEventArgs(null, false, null));
                    }
                    else
                    {
                        SetMessage("Please wait while the Mobilenet Model is being downloaded...");
                        _mobilenet.OnDownloadProgressChanged += onDownloadProgressChanged;
                        _mobilenet.OnDownloadCompleted += onDownloadCompleted;
                        _mobilenet.Init();
                    }
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
                SetMessage(String.Format("{0} bytes downloaded", e.BytesReceived, e.ProgressPercentage));
            else
                SetMessage(String.Format("{0} of {1} bytes downloaded ({2}%)", e.BytesReceived, e.TotalBytesToReceive, e.ProgressPercentage));
        }

        private void onDownloadCompleted(object sender, System.ComponentModel.AsyncCompletedEventArgs e)
        {

            Stopwatch watch = Stopwatch.StartNew();
            var result = _mobilenet.Recognize(_imageFiles[0]);
            watch.Stop();
            String resStr = String.Format("Object is {0} with {1}% probability. Recognition completed in {2} milliseconds.", result[0].Label, result[0].Probability * 100, watch.ElapsedMilliseconds);

            SetImage(_imageFiles[0]);
            SetMessage(resStr);
            
        }

        private void OnButtonClicked(Object sender, EventArgs args)
        {
            LoadImages(new string[] { "space_shuttle.jpg" });
        }

    }
}
