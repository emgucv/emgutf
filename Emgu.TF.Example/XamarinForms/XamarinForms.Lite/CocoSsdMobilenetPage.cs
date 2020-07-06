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
#elif __MACOS__
using AppKit;
using CoreGraphics;
using Emgu.Util;
#elif __IOS__
using UIKit;
using CoreGraphics;
using Emgu.Util;
#endif

namespace Emgu.TF.XamarinForms
{
    public class CocoSsdMobilenetPage : ButtonTextImagePage
    {
        private CocoSsdMobilenetV3 _mobilenet;

        public CocoSsdMobilenetPage()
           : base()
        {
#if __MACOS__ || __IOS__
            AllowAvCaptureSession = true;
#endif
            var button = this.TopButton;
            button.Text = "Perform Object Detection";
            button.Clicked += OnButtonClicked;

            _mobilenet = new CocoSsdMobilenetV3();
            _mobilenet.OnDownloadProgressChanged += onDownloadProgressChanged;

#if __MACOS__ || __IOS__
            outputRecorder.BufferReceived += OutputRecorder_BufferReceived;
#endif

        }

#if __MACOS__ || __IOS__

        //private int _counter = 0;
        private void OutputRecorder_BufferReceived(object sender, OutputRecorder.BufferReceivedEventArgs e)
        {
            try
            {
                //_counter++;
#if __IOS__
                UIImage image = e.Buffer.ToUIImage();
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

                    //if (UIImageView.Frame.Size != annotatedImage.Size)
                    //    UIImageView.Frame = new CGRect(CGPoint.Empty, annotatedImage.Size);
                    //SetMessage( String.Format("{0} image", _counter) );
                    //UIImage oldImage = UIImageView.Image;
                    //UIImageView.Image = annotatedImage;
                    //if (oldImage != null)
                    //    oldImage.Dispose();
                    SetImage(annotatedImage);
#else
                    NativeImageIO.DrawAnnotations(image, annotations);
                    
                    //SetMessage(String.Format("{0} image", _counter));
                    SetImage(image);
#endif
                });

                //
                // Although this looks innocent "Oh, he is just optimizing this case away"
                // this is incredibly important to call on this callback, because the AVFoundation
                // has a fixed number of buffers and if it runs out of free buffers, it will stop
                // delivering frames. 
                // 

                //Console.WriteLine(String.Format("Frame at: {0}", DateTime.Now));

            }
            catch (Exception ex)
            {
                Console.WriteLine(e);
                SetMessage(ex.Message);
            }
        }
#endif

        private void onDownloadProgressChanged(object sender, System.Net.DownloadProgressChangedEventArgs e)
        {
            if (e.TotalBytesToReceive <= 0)
                SetMessage(String.Format("{0} bytes downloaded.", e.BytesReceived));
            else
                SetMessage(String.Format("{0} of {1} bytes downloaded ({2}%)", e.BytesReceived, e.TotalBytesToReceive, e.ProgressPercentage));
        }

        private static Annotation[] GetAnnotations(CocoSsdMobilenetV3.RecognitionResult[] result)
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

        private async void OnButtonClicked(Object sender, EventArgs args)
        {
            SetMessage("Please wait while the Coco SSD Mobilenet Model is being downloaded...");
#if !DEBUG
            try
#endif
            {
                await _mobilenet.Init();
            }
#if !DEBUG
                catch (Exception e)
                {
                    String msg = e.Message.Replace(System.Environment.NewLine, " ");
                    SetMessage(msg);     
                }
#endif

            if (this.TopButton.Text.Equals("Stop"))
            {
                // Stop camera
#if __IOS__ || __MACOS__
                this.StopCaptureSession();
#endif
                this.TopButton.Text = "Perform Object Detection";
            } else
            {
                String[] imageFiles = await LoadImages(new string[] { "dog416.png" });
                if (imageFiles == null)
                {
                    //User canceled
                    return;
                }

                String imageFileName = imageFiles[0];

                if (imageFileName.Equals("Camera Stream"))
                {

#if __MACOS__ || __IOS__
                SetMessage(String.Format("Model trained to recognize the following objects: {0}", String.Join("; ", _mobilenet.Labels)));
                this.TopButton.Text = "Stop";
                CheckVideoPermissionAndStart();
#else

#endif
                }
                else
                {
                    Stopwatch watch = Stopwatch.StartNew();
                    var result = _mobilenet.Recognize(imageFileName, 0.5f);
                    watch.Stop();

                    Annotation[] annotations = GetAnnotations(result);
                    
                    JpegData jpeg = NativeImageIO.ImageFileToJpeg(imageFileName, annotations);
                    //NativeImageIO.JpegData jpeg = NativeImageIO.ImageFileToJpeg(_imageFiles[0]);
                    //String names = String.Join(";", Array.ConvertAll(result, r => r.Label));
                    SetImage(jpeg.Raw, jpeg.Width, jpeg.Height);

                    String resStr = String.Format("Detected {1} objects in {0} milliseconds.", watch.ElapsedMilliseconds, result.Length);
                    SetMessage(resStr);
                }
            }
        }

    }
}
