//----------------------------------------------------------------------------
//  Copyright (C) 2004-2019 by EMGU Corporation. All rights reserved.       
//----------------------------------------------------------------------------

#if __IOS__
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
using UIKit;
using CoreGraphics;
using Xamarin.Forms;

using System.Threading;
using Foundation;
using AVFoundation;
using CoreVideo;
using CoreMedia;
using CoreImage;
using CoreFoundation;
using Xamarin.Forms.Platform.iOS;

namespace Emgu.TF.XamarinForms
{
    public class CameraViewPage : Xamarin.Forms.ContentPage
    {
        private CocoSsdMobilenet _mobilenet;

        public UIImageView ImageView;
        private Label _label;
        AVCaptureSession session;
        OutputRecorder outputRecorder;
        DispatchQueue queue;

        //private CocoSsdMobilenet _mobilenet;
        //private string[] _imageFiles = null;

        public CameraViewPage()
           : base()
        {
            Button button = new Button();
            button.Text = "Button";
            _label = new Label();
            _label.Text = "Label";

            ImageView = new UIImageView();
            ImageView.ContentMode = UIViewContentMode.ScaleAspectFit;

            Xamarin.Forms.StackLayout stackLayout = new StackLayout();
            stackLayout.Children.Add(button);
            stackLayout.Children.Add(_label);
            stackLayout.Children.Add(ImageView.ToView());

            Content = stackLayout;

            _mobilenet = new CocoSsdMobilenet();


            SetMessage("Please wait...");

            InitModel();


            //CheckVideoPermissionAndStart();
        }

        public void SetMessage(String message)
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

            //Stopwatch watch = Stopwatch.StartNew();
            //var result = _mobilenet.Recognize(_imageFiles[0]);
            //watch.Stop();
            //String resStr = String.Format("Object is {0} with {1}% probability. Recognition completed in {2} milliseconds.", result[0].Label, result[0].Probability * 100, watch.ElapsedMilliseconds);

            //SetImage(_imageFiles[0]);
            //SetMessage(resStr);
            CheckVideoPermissionAndStart();
        }

        private void CheckVideoPermissionAndStart()
        {
            AVFoundation.AVAuthorizationStatus authorizationStatus = AVCaptureDevice.GetAuthorizationStatus(AVMediaType.Video);
            switch (authorizationStatus)
            {
                case AVAuthorizationStatus.NotDetermined:
                    AVCaptureDevice.RequestAccessForMediaType(AVMediaType.Video, delegate (bool granted)
                    {
                        if (granted)
                        {
                            SetupCaptureSession();
                        }
                        else
                        {
                            _label.Text = "Please grant Video Capture permission";
                            //RenderImageMessage("Please grant Video Capture permission");
                        }
                    });
                    break;
                case AVAuthorizationStatus.Authorized:
                    SetupCaptureSession();
                    break;
                case AVAuthorizationStatus.Denied:
                case AVAuthorizationStatus.Restricted:
                    _label.Text = "Please grant Video Capture permission";
                    //RenderImageMessage("Please grant Video Capture permission");
                    break;
                default:

                    break;
                    //do nothing
            }

        }
        private void SetupCaptureSession()
        {
            // configure the capture session for low resolution, change this if your code
            // can cope with more data or volume
            session = new AVCaptureSession()
            {
                SessionPreset = AVCaptureSession.PresetMedium
            };

            // create a device input and attach it to the session
            var captureDevice = AVCaptureDevice.GetDefaultDevice(AVMediaType.Video);
            if (captureDevice == null)
            {
                //RenderImageMessage("Capture device not found.");
                _label.Text = "Capture device not found.";
                return;
            }
            var input = AVCaptureDeviceInput.FromDevice(captureDevice);
            if (input == null)
            {
                //RenderImageMessage("No input device");
                _label.Text = "No input device";
                return;
            }
            session.AddInput(input);

            // create a VideoDataOutput and add it to the sesion
            AVVideoSettingsUncompressed settingUncomp = new AVVideoSettingsUncompressed();
            settingUncomp.PixelFormatType = CVPixelFormatType.CV32BGRA;
            var output = new AVCaptureVideoDataOutput()
            {
                UncompressedVideoSetting = settingUncomp,

                // If you want to cap the frame rate at a given speed, in this sample: 15 frames per second
                //MinFrameDuration = new CMTime (1, 15)
            };


            // configure the output
            queue = new DispatchQueue("myQueue");
            outputRecorder = new OutputRecorder(ImageView, _label, _mobilenet);
            output.SetSampleBufferDelegateQueue(outputRecorder, queue);
            session.AddOutput(output);

            session.StartRunning();

        }
    }

    public class OutputRecorder : AVCaptureVideoDataOutputSampleBufferDelegate
    {
        private UIImageView _imageView;
        private Label _label;
        private CocoSsdMobilenet _mobilenet;

        public OutputRecorder(UIImageView imageView, Label label, CocoSsdMobilenet mobilenet)
        {
            _imageView = imageView;
            _label = label;
            _mobilenet = mobilenet;
        }

        private int _counter = 0;
        public override void DidOutputSampleBuffer(AVCaptureOutput captureOutput, CMSampleBuffer sampleBuffer, AVCaptureConnection connection)
        {
            try
            {
                _counter++;
                UIImage image = ImageFromSampleBuffer(sampleBuffer);
                CocoSsdMobilenet.RecognitionResult[] result = _mobilenet.Recognize(image, 0.5f);
                NativeImageIO.Annotation[] annotations = new NativeImageIO.Annotation[result.Length];
                for (int i = 0; i < result.Length; i++)
                {
                    NativeImageIO.Annotation annotation = new NativeImageIO.Annotation();
                    annotation.Rectangle = result[i].Rectangle;
                    annotation.Label = String.Format("{0}:({1:0.00}%)", result[i].Label, result[i].Score * 100);
                    annotations[i] = annotation;
                }

                UIImage annotatedImage = NativeImageIO.DrawAnnotations(image, annotations);
                image.Dispose();

                // Do something with the image, we just stuff it in our main view.
                BeginInvokeOnMainThread(delegate
                {
                    if (_imageView.Frame.Size != image.Size)
                        _imageView.Frame = new CGRect(CGPoint.Empty, image.Size);
                    
                    UIImage oldImage = _imageView.Image;

                    _imageView.Image = annotatedImage;
                    
                    _label.Text = String.Format("{0} image", _counter);

                    if (oldImage != null)
                        oldImage.Dispose();
                });
                
                //
                // Although this looks innocent "Oh, he is just optimizing this case away"
                // this is incredibly important to call on this callback, because the AVFoundation
                // has a fixed number of buffers and if it runs out of free buffers, it will stop
                // delivering frames. 
                // 
                sampleBuffer.Dispose();
                //Console.WriteLine(String.Format("Frame at: {0}", DateTime.Now));

                
            }
            catch (Exception e)
            {
                Console.WriteLine(e);

                BeginInvokeOnMainThread(delegate
                {
                    _label.Text = String.Format("{0} image", e.Message);
                });
            }
        }

        //private static MCvFont font = new MCvFont(Emgu.CV.CvEnum.FONT.CV_FONT_HERSHEY_PLAIN, 1.0, 1.0);

        UIImage ImageFromSampleBuffer(CMSampleBuffer sampleBuffer)
        {
            UIImage image; 
            // Get the CoreVideo image
            using (CVPixelBuffer pixelBuffer = sampleBuffer.GetImageBuffer() as CVPixelBuffer)
            {
                // Lock the base address
                pixelBuffer.Lock(CVPixelBufferLock.ReadOnly);
                using (CIImage cIImage = new CIImage(pixelBuffer))
                {
                    
                    image = new UIImage(cIImage);
                    //return UIImage.FromImage(cIImage);
                }
                pixelBuffer.Unlock(CVPixelBufferLock.ReadOnly);
            }
            return image;
        }
    }
}

#endif