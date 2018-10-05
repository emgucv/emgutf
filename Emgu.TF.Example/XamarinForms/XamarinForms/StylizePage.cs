//----------------------------------------------------------------------------
//  Copyright (C) 2004-2018 by EMGU Corporation. All rights reserved.       
//----------------------------------------------------------------------------

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
using Emgu.TF;
using Emgu.Models;
using Emgu.TF.Models;
using System.Net;
using System.ComponentModel;
using System.Diagnostics;

namespace Emgu.TF.XamarinForms
{
    public class StylizePage : ModelButtonTextImagePage
    {        
        private StylizeGraph _stylizeGraph;
        
        public override String GetButtonName(ButtonMode mode)
        {
            switch(mode)
            {
                case ButtonMode.WaitingModelDownload:
                    return "Download Model";
                default:
                    return "Stylize";
            }
        }

        public StylizePage()
            : base()
        {
            if (_stylizeGraph == null)
            {
                _stylizeGraph = new StylizeGraph();
                _stylizeGraph.OnDownloadProgressChanged += onDownloadProgressChanged;
                _stylizeGraph.OnDownloadCompleted += onDownloadCompleted;
                _stylizeGraph.OnDownloadCompleted += (sender, e) =>
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
                    byte[] jpeg = _stylizeGraph.StylizeToJpeg(image[0], 1);
                    watch.Stop();
                    SetImage(jpeg);
#if __MACOS__
                    NSImage img = new NSImage(image[0]);
                    var displayImage = this.GetImage();
                    displayImage.WidthRequest = img.Size.Width;
                    displayImage.HeightRequest = img.Size.Height;
#endif
                    SetMessage(String.Format("Stylized in {0} milliseconds.", watch.ElapsedMilliseconds));
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
                _stylizeGraph.Init();
            }
            else
            {
                LoadImages(new string[] { "surfers.jpg" });
            }
        }
    }
}
