//----------------------------------------------------------------------------
//  Copyright (C) 2004-2019 by EMGU Corporation. All rights reserved.       
//----------------------------------------------------------------------------

#if __IOS__ || __MACOS__
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
using Emgu.Util;

using CoreGraphics;
using Xamarin.Forms;

using System.Threading;
using Foundation;
using AVFoundation;
using CoreVideo;
using CoreMedia;
using CoreImage;
using CoreFoundation;

#if __IOS__
using UIKit;
using Xamarin.Forms.Platform.iOS;
#else
using AppKit;
using Xamarin.Forms.Platform.MacOS;
#endif

namespace Emgu.TF.XamarinForms
{
    public class CameraViewPage : AvCaptureSessionPage
    {
        private CocoSsdMobilenet _mobilenet;

#if __IOS__
        public UIImageView ImageView;
#else
        public NSImageView ImageView;
#endif
        private Label _label;

        public CameraViewPage()
           : base()
        {
            Button button = new Button();
            button.Text = "Button";
            _label = new Label();
            _label.Text = "Label";

#if __IOS__
            ImageView = new UIImageView();
            ImageView.ContentMode = UIViewContentMode.ScaleAspectFit;
#else
            ImageView = new NSImageView();
            ImageView.ImageScaling = NSImageScale.None;
#endif
            Xamarin.Forms.StackLayout stackLayout = new StackLayout();
            stackLayout.Children.Add(button);
            stackLayout.Children.Add(_label);
            stackLayout.Children.Add(ImageView.ToView());

            Content = stackLayout;

            _mobilenet = new CocoSsdMobilenet();

            outputRecorder.BufferReceived += OutputRecorder_BufferReceived;

            _label.Text = "Please wait...";
            InitModel();

        }

        private static Annotation[] GetAnnotations(CocoSsdMobilenet.RecognitionResult[] result)
        {
            Annotation[] annotations = new Annotation[result.Length];
            for (int i = 0; i < result.Length; i++)
            {
                Annotation annotation = new Annotation();
                annotation.Rectangle = result[i].Rectangle;
                annotation.Label = String.Format("{0}:({1:0.00}%)", result[i].Label, result[i].Score * 100);
                annotations[i] = annotation;
            }
            return annotations;
        }

        private int _counter = 0;

        private void OutputRecorder_BufferReceived(object sender, OutputRecorder.BufferReceivedEventArgs e)
        {
            try
            {
                _counter++;
#if __IOS__
                UIImage image = sampleBuffer.ToUIImage();
                CocoSsdMobilenet.RecognitionResult[] result = _mobilenet.Recognize(image, 0.5f);
                Annotation[] annotations = GetAnnotations(result);
                UIImage annotatedImage = NativeImageIO.DrawAnnotations(image, annotations);
                image.Dispose();
#else
                NSImage image = e.Buffer.ToNSImage();
                CocoSsdMobilenet.RecognitionResult[] result = _mobilenet.Recognize(image, 0.5f);
                Annotation[] annotations = GetAnnotations(result);
#endif

                //Debug.WriteLine(image == null ? "null image" : String.Format(">image {0} x {1}", image.Size.Width, image.Size.Height));
                // Do something with the image, we just stuff it in our main view.
                Xamarin.Forms.Device.BeginInvokeOnMainThread(delegate
                {
                    //Debug.WriteLine(image == null ? "null image" : String.Format(">>image {0} x {1}", image.Size.Width, image.Size.Height));
#if __IOS__
                    if (_imageView.Frame.Size != annotatedImage.Size)
                        _imageView.Frame = new CGRect(CGPoint.Empty, annotatedImage.Size);
                    _label.Text = String.Format("{0} image", _counter);
                    UIImage oldImage = ImageView.Image;
                    ImageView.Image = annotatedImage;
                    if (oldImage != null)
                        oldImage.Dispose();
#else
                    NativeImageIO.DrawAnnotations(image, annotations);
                    if (ImageView.Frame.Size != image.Size)
                        ImageView.Frame = new CGRect(CGPoint.Empty, image.Size);
                    _label.Text = String.Format("{0} image", _counter);
                    NSImage oldImage = ImageView.Image;
                    ImageView.Image = image;
                    if (oldImage != null)
                        oldImage.Dispose();
#endif
                });

                //
                // Although this looks innocent "Oh, he is just optimizing this case away"
                // this is incredibly important to call on this callback, because the AVFoundation
                // has a fixed number of buffers and if it runs out of free buffers, it will stop
                // delivering frames. 
                // 

                Console.WriteLine(String.Format("Frame at: {0}", DateTime.Now));

            }
            catch (Exception ex)
            {
                Console.WriteLine(e);
                SetMessage(ex.Message);
            }
        }

        public override void SetMessage(String message)
        {
            Xamarin.Forms.Device.BeginInvokeOnMainThread(
                () =>
                {
                    _label.Text = message;
                    _label.LineBreakMode = LineBreakMode.WordWrap;
                    _label.WidthRequest = this.Width;
                }
            );
        }

        private void InitModel()
        {
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

            CheckVideoPermissionAndStart();
        }
    }
}

#endif
