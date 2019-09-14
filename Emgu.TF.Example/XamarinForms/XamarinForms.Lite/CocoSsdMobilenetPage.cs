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
    public class CocoSsdMobilenetPage : ButtonTextImagePage
    {

        private CocoSsdMobilenet _mobilenet;
        private string[] _imageFiles = null;

        public CocoSsdMobilenetPage()
           : base()
        {

            var button = this.TopButton;
            button.Text = "Perform Object Detection";
            button.Clicked += OnButtonClicked;

            _mobilenet = new CocoSsdMobilenet();

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
                        SetMessage("Please wait while the Coco SSD Mobilenet Model is being downloaded...");
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
                SetMessage(String.Format("{0} bytes downloaded.", e.BytesReceived, e.ProgressPercentage));
            else
                SetMessage(String.Format("{0} of {1} bytes downloaded ({2}%)", e.BytesReceived, e.TotalBytesToReceive, e.ProgressPercentage));
        }

        private void onDownloadCompleted(object sender, System.ComponentModel.AsyncCompletedEventArgs e)
        {
            if (e != null && e.Error != null)
            {
                SetMessage(e.Error.Message);
                return;
            }

            Stopwatch watch = Stopwatch.StartNew();
            var result = _mobilenet.Recognize(_imageFiles[0], 0.5f);
            watch.Stop();

            Annotation[] annotations = new Annotation[result.Length];
            for (int i = 0; i < result.Length; i++)
            {
                Annotation annotation = new Annotation();
                annotation.Rectangle = result[i].Rectangle;
                annotation.Label = String.Format("{0}:({1:0.00}%)", result[i].Label, result[i].Score * 100);
                annotations[i] = annotation;
            }


            JpegData jpeg = NativeImageIO.ImageFileToJpeg(_imageFiles[0], annotations);
            //NativeImageIO.JpegData jpeg = NativeImageIO.ImageFileToJpeg(_imageFiles[0]);
            //String names = String.Join(";", Array.ConvertAll(result, r => r.Label));
            SetImage(jpeg.Raw, jpeg.Width, jpeg.Height);


            String resStr = String.Format("Detected {1} objects in {0} milliseconds.", watch.ElapsedMilliseconds, result.Length);
            SetMessage(resStr);

        }

        private void OnButtonClicked(Object sender, EventArgs args)
        {
            LoadImages(new string[] { "dog416.png" });
        }

    }
}
