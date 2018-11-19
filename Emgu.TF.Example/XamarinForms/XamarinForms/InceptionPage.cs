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
using Emgu.Models;
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
    public class InceptionPage : ModelButtonTextImagePage
    {
        private Inception _inceptionGraph;

        public override String GetButtonName(ButtonMode mode)
        {
            switch (mode)
            {
                case ButtonMode.WaitingModelDownload:
                    return "Download Model";
                default:
                    return "Recognize object";
            }
        }

        public InceptionPage()
            : base()
        {
            if (_inceptionGraph == null)
            {
                _inceptionGraph = new Inception();
                _inceptionGraph.OnDownloadProgressChanged += onDownloadProgressChanged;
                _inceptionGraph.OnDownloadCompleted += onDownloadCompleted;
                _inceptionGraph.OnDownloadCompleted += (sender, e) => 
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
                    

                    Tensor imageTensor = Emgu.TF.Models.ImageIO.ReadTensorFromImageFile<float>(image[0], 224, 224, 128.0f, 1.0f / 128.0f);
                    Stopwatch watch = Stopwatch.StartNew();
                    Inception.RecognitionResult result = _inceptionGraph.MostLikely(imageTensor);
                    String msg = String.Format("Object is {0} with {1}% probability. Recognized in {2} milliseconds.", result.Label, result.Probability * 100, watch.ElapsedMilliseconds);
                    SetMessage(msg);
                    
                    SetImage(image[0]);
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
                _inceptionGraph.Init();
            }
            else
            {
                LoadImages(new string[] { "space_shuttle.jpg" });
            }
        }

   
    }
}
