//----------------------------------------------------------------------------
//  Copyright (C) 2004-2017 by EMGU Corporation. All rights reserved.       
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
    public class MultiboxDetectionPage : ButtonTextImagePage
    {
        public MultiboxDetectionPage()
           : base()
        {

            var button = this.GetButton();
            button.Text = "Perform People Detection";
            button.Clicked += OnButtonClicked;

            OnImagesLoaded += async (sender, image) =>
            {
                GetLabel().Text = "Please wait...";
                SetImage();

                Task<Tuple<byte[], long>> t = new Task<Tuple<byte[], long>>(
                    () =>
                    {
                        //MultiboxGrapho.Download();
                        MultiboxGraph graph = new MultiboxGraph();
                        Tensor imageTensor = Emgu.TF.Models.ImageIO.ReadTensorFromImageFile(image[0], 224, 224, 128.0f, 1.0f / 128.0f);
                        Stopwatch watch = Stopwatch.StartNew();
                        MultiboxGraph.Result detectResult = graph.Detect(imageTensor);
                        watch.Stop();
#if __ANDROID__
                     BitmapFactory.Options options = new BitmapFactory.Options();
                     options.InMutable = true;
                     Android.Graphics.Bitmap bmp = BitmapFactory.DecodeFile(image[0], options);
                     MultiboxGraph.DrawResults(bmp, detectResult, 0.2f);
                     using (MemoryStream ms = new MemoryStream())
                     {
                         bmp.Compress(Bitmap.CompressFormat.Jpeg, 90, ms);
                         return new Tuple<byte[], long>(ms.ToArray(), watch.ElapsedMilliseconds);
                     }

#elif __UNIFIED__ && !__IOS__

                        NSImage img = NSImage.ImageNamed(image[0]);
                       
                        Xamarin.Forms.Device.BeginInvokeOnMainThread(() =>
                     {
                         MultiboxGraph.DrawResults(img, detectResult, 0.1f);
                         var imageData = img.AsTiff();
                         var imageRep = NSBitmapImageRep.ImageRepsWithData(imageData)[0] as NSBitmapImageRep;
                         var jpegData = imageRep.RepresentationUsingTypeProperties(NSBitmapImageFileType.Jpeg, null);
                         byte[] raw = new byte[jpegData.Length];
                         System.Runtime.InteropServices.Marshal.Copy(jpegData.Bytes, raw, 0, (int)jpegData.Length);
						 SetImage(raw);
						 GetLabel().Text = String.Format("Detected with in {0} milliseconds.", watch.ElapsedMilliseconds);
                     });



                        return new Tuple<byte[], long>(null, 0);
#elif __IOS__
                        UIImage uiimage = new UIImage(image[0]);

						Xamarin.Forms.Device.BeginInvokeOnMainThread(() =>
						{
							UIImage newImg = MultiboxGraph.DrawResults(uiimage, detectResult, 0.1f);
	                        var jpegData = newImg.AsJPEG();
							byte[] raw = new byte[jpegData.Length];
							System.Runtime.InteropServices.Marshal.Copy(jpegData.Bytes, raw, 0, (int)jpegData.Length);
	                        SetImage(raw);
							GetLabel().Text = String.Format("Detected with in {0} milliseconds.", watch.ElapsedMilliseconds);
						});

                    return new Tuple<byte[], long>(null, 0);
#else
                    return new Tuple<byte[], long>(new byte[10], 0);
#endif
                    });
                t.Start();

#if !(__UNIFIED__)
             var result = await t;
            SetImage(t.Result.Item1);
            GetLabel().Text = String.Format("Detection took {0} milliseconds.", t.Result.Item2);
#endif
            };
      }

      private void OnButtonClicked(Object sender, EventArgs args)
      {
         LoadImages(new string[] { "surfers.jpg" });
      }

   }
}
