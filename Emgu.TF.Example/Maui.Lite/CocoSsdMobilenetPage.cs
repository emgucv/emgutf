//----------------------------------------------------------------------------
//  Copyright (C) 2004-2024 by EMGU Corporation. All rights reserved.       
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
using Emgu.CV.Structure;
using Emgu.TF.Lite;
using Emgu.Models;
using Emgu.TF.Lite.Models;
using Emgu.Util;


namespace Maui.Demo.Lite
{
    public class CocoSsdMobilenetPage : ButtonTextImagePage
    {
        private CocoSsdMobilenetV3 _mobilenet;
        public CocoSsdMobilenetPage()
           : base()
        {
            var button = this.TopButton;
            button.Text = "Perform Object Detection";
            button.Clicked += OnButtonClicked;

            _mobilenet = new CocoSsdMobilenetV3();
            _mobilenet.OnDownloadProgressChanged += onDownloadProgressChanged;

        }


        private void onDownloadProgressChanged(long? totalBytesToReceive, long bytesReceived, double? progressPercentage)
        {
            if (totalBytesToReceive.HasValue && totalBytesToReceive > 0)
                SetMessage(String.Format("{0} of {1} bytes downloaded ({2}%)", bytesReceived, totalBytesToReceive, progressPercentage));
            else
                SetMessage(String.Format("{0} bytes downloaded.", bytesReceived));
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
            SetMessage("Please wait while the Coco SSD Mobilenet model is being downloaded...");
#if !DEBUG
            try
#endif
            {
                await _mobilenet.Init();
                if (!_mobilenet.Imported)
                {
                    SetMessage("Failed to initialize Mobilenet.");
                    return;
                }
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
                this.TopButton.Text = "Perform Object Detection";
            }
            else
            {
                Mat[] images = await LoadImages(new string[] { "dog416.png" });
                //handle user cancel
                if (images == null || (images.Length > 0 && images[0] == null))
                {
                    SetMessage("");
                    return;
                }

                Tensor t = _mobilenet.InputTensor;
                System.Drawing.Size s = new System.Drawing.Size(t.Dims[2], t.Dims[1]);
                using (Mat tensorMat = new Mat(
                           s,
                           DepthType.Cv8U,
                           3,
                           t.DataPointer,
                           3 * s.Width * Marshal.SizeOf<byte>()))
                {
                    CvInvoke.Resize(images[0], tensorMat, s);
                }

                Stopwatch watch = Stopwatch.StartNew();
                _mobilenet.Interpreter.Invoke();
                var result = _mobilenet.GetResults(0.5f);
                watch.Stop();

                Mat renderMat = images[0];
                Annotation[] annotations = GetAnnotations(result);
                for (int i = 0; i < annotations.Length; i++)
                {
                    if (annotations[i].Rectangle != null)
                    {
                        float[] rects = NativeImageIO.ScaleLocation(annotations[i].Rectangle, renderMat.Width, renderMat.Height);
                        System.Drawing.PointF origin = new System.Drawing.PointF(rects[0], rects[1]);
                        System.Drawing.RectangleF rect = new System.Drawing.RectangleF(origin,
                            new System.Drawing.SizeF(rects[2] - rects[0], rects[3] - rects[1]));
                        CvInvoke.Rectangle(renderMat, System.Drawing.Rectangle.Round(rect), new MCvScalar(0,0,255));
                        
                        String label = annotations[i].Label;
                        CvInvoke.PutText(renderMat, label, System.Drawing.Point.Round( rect.Location ), FontFace.HersheyDuplex, 1.0, new MCvScalar(0,0,255));
                    }
                }

                SetImage(renderMat);
                String resStr = String.Format("Detected {1} objects in {0} milliseconds.", watch.ElapsedMilliseconds, result.Length);
                SetMessage(resStr);

            }
        }

    }
}
