//----------------------------------------------------------------------------
//  Copyright (C) 2004-2023 by EMGU Corporation. All rights reserved.       
//----------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Drawing;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Runtime.InteropServices;
using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Platform.Maui.UI;
using Emgu.TF.Lite;
using Emgu.Models;
using Emgu.TF.Lite.Models;
using Size = System.Drawing.Size;

/*
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
*/

namespace Maui.Demo.Lite
{
    public class InceptionPage : ButtonTextImagePage
    {
        private Inception _inception;

        public InceptionPage()
           : base()
        {

            var button = this.TopButton;
            button.Text = "Perform Image Classification";
            button.Clicked += OnButtonClicked;

            _inception = new Inception();
            _inception.OnDownloadProgressChanged += onDownloadProgressChanged;

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
            if (e != null && e.Error != null)
            {
                SetMessage(e.Error.Message);
                return;
            }
        }

        private async void OnButtonClicked(Object sender, EventArgs args)
        {
            SetMessage("Please wait while the Inception Model is being downloaded...");
            await _inception.Init();
            if (!_inception.Imported)
            {
                SetMessage("Failed to initialize Inception Model.");
                return;
            }
            SetImage(null);
            Mat[] imageFiles = await LoadImages(new string[] { "tulips.jpg" });

            //handle user cancel
            if (imageFiles == null || (imageFiles.Length > 0 && imageFiles[0] == null))
            {
                SetMessage("");
                return;
            }

            Stopwatch watch = Stopwatch.StartNew();

            Tensor t = _inception.InputTensor;
            System.Drawing.Size s = new System.Drawing.Size(299, 299);
            using (Mat resizedMat = new Mat(s, DepthType.Cv8U, 3))
            using (Mat tensorMat = new Mat(
                       s, 
                       DepthType.Cv32F, 
                       3, 
                       t.DataPointer,
                       3 * s.Width * Marshal.SizeOf<float>()))
            {
                CvInvoke.Resize(imageFiles[0], resizedMat, s);
                CvInvoke.CvtColor(resizedMat, resizedMat, ColorConversion.Bgr2Rgb);
                resizedMat.ConvertTo(tensorMat, DepthType.Cv32F, 1.0/255.0, -0.0);
                
            }

            var result = _inception.Invoke();
            watch.Stop();
            String resStr = String.Format("Object is {0} with {1}% probability. Recognition completed in {2} milliseconds.", result[0].Label, result[0].Probability * 100, watch.ElapsedMilliseconds);

            SetImage(imageFiles[0]);
            SetMessage(resStr);
        }

    }
}
