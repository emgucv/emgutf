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
using Emgu.TF;
using Emgu.TF.Models;
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
    public class MultiboxDetectionPage : ModelButtonTextImagePage
    {
        private MultiboxGraph _multiboxGraph;

        public override String GetButtonName(ButtonMode mode)
        {
            switch (mode)
            {
                case ButtonMode.WaitingModelDownload:
                    return "Download Model";
                default:
                    return "Detect People";
            }
        }

        public MultiboxDetectionPage()
           : base()
        {
            if (_multiboxGraph == null)
            {
                _multiboxGraph = new MultiboxGraph();
                _multiboxGraph.OnDownloadProgressChanged += onDownloadProgressChanged;
                _multiboxGraph.OnDownloadCompleted += onDownloadCompleted;
                _multiboxGraph.OnDownloadCompleted += (sender, e) =>
                {
                    OnButtonClicked(sender, e);
                };
            }

            OnImagesLoaded += (sender, image) =>
            {
                try
                {
                    SetMessage("Please wait...");
                    SetImage();
                    Stopwatch watch = Stopwatch.StartNew();
                    Tensor imageTensor = Emgu.TF.Models.ImageIO.ReadTensorFromImageFile(image[0], 224, 224, 128.0f, 1.0f / 128.0f);
                    MultiboxGraph.Result[] detectResult = _multiboxGraph.Detect(imageTensor);
                    watch.Stop();

                    byte[] jpeg = _multiboxGraph.DrawResultsToJpeg(image[0], detectResult);

                    watch.Stop();
                    SetImage(jpeg);
#if __MACOS__
                    NSImage img = new NSImage(image[0]);
                    var displayImage = this.GetImage();
                    displayImage.WidthRequest = img.Size.Width;
                    displayImage.HeightRequest = img.Size.Height;
#endif

                    SetMessage(String.Format("Detected in {0} milliseconds.", watch.ElapsedMilliseconds));
                }
                catch (Exception excpt)
                {
                    String msg = excpt.Message.Replace(System.Environment.NewLine, " ");
                    SetMessage(msg);
                }
            };
        }

        public override void OnButtonClicked(Object sender, EventArgs args)
        {
            base.OnButtonClicked(sender, args);

            if (_buttonMode == ButtonMode.WaitingModelDownload)
            {
                _multiboxGraph.Init();
            }
            else
            {
                LoadImages(new string[] { "surfers.jpg" });
            }
        }

    }
}
