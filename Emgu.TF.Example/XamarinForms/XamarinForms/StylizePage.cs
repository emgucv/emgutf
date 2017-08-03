//----------------------------------------------------------------------------
//  Copyright (C) 2004-2017 by EMGU Corporation. All rights reserved.       
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
#endif
using Emgu.TF;
using Emgu.TF.Models;


namespace Emgu.TF.XamarinForms
{
    public class StylizePage : ButtonTextImagePage
    {
        public StylizePage()
            : base()
        {

            var button = this.GetButton();
            button.Text = "Stylizing image";
            button.Clicked += OnButtonClicked;

            OnImagesLoaded += async (sender, image) =>
            {
                SetMessage("Please wait...");
                SetImage();

                Task<Tuple<byte[], string, long>> t = new Task<Tuple<byte[], string, long>>(
                    () =>
                    {
                        try
                        {
                            SetMessage("Please wait while we download the Stylize Model from internet.");
                            StylizeGraph stylizeGraph = new StylizeGraph();
                            SetMessage("Please wait...");

                            Tensor imageTensor = Emgu.TF.Models.ImageIO.ReadTensorFromImageFile(image[0], -1, -1, 0f, 1.0f/255f);
                            Tensor stylizedImage = stylizeGraph.Stylize(imageTensor, 0);



#if __ANDROID__
                            byte[] rawPixel = Emgu.TF.Models.ImageIO.GetRawImage(stylizedImage, 255.0f, 4);
                            int[] dim = stylizedImage.Dim;
                            using (Bitmap bitmap = Bitmap.CreateBitmap(dim[2], dim[1], Bitmap.Config.Argb8888))
                            using (MemoryStream ms = new MemoryStream())
                            {
                                IntPtr ptr = bitmap.LockPixels();
                                //GCHandle handle = GCHandle.Alloc(colors, GCHandleType.Pinned);
                                Marshal.Copy(rawPixel, 0, ptr, rawPixel.Length);

                                bitmap.UnlockPixels();
                                
                                bitmap.Compress(Bitmap.CompressFormat.Jpeg, 90, ms);
                                return new Tuple<byte[], string, long>(ms.ToArray(), null, 0);
                            }
#elif __IOS__
                            byte[] rawPixel = Emgu.TF.Models.ImageIO.GetRawImage(stylizedImage, 255.0f);
                            int[] dim = stylizedImage.Dim;
                            System.Drawing.Size sz = new System.Drawing.Size(dim[2], dim[1]);
                            GCHandle handle = GCHandle.Alloc(rawPixel, GCHandleType.Pinned);
                            using (CGColorSpace cspace = CGColorSpace.CreateDeviceRGB())
                            using (CGBitmapContext context = new CGBitmapContext(
                                handle.AddrOfPinnedObject(),
                                sz.Width, sz.Height,
                                8,
                                sz.Width * 3,
                                cspace,
                                CGImageAlphaInfo.PremultipliedLast))
                            using (CGImage cgImage = context.ToImage())
                            using (UIImage newImg = new UIImage(cgImage))
                            {
                                handle.Free();
                                var jpegData = newImg.AsJPEG();
                                byte[] raw = new byte[jpegData.Length];
                                System.Runtime.InteropServices.Marshal.Copy(jpegData.Bytes, raw, 0,
                                    (int)jpegData.Length);
                                return new Tuple<byte[], string, long>(raw, String.Empty, 0);
                            }
#else
                            return new Tuple<byte[], string, long>(null, null, 0);
#endif

                            //SetImage(t.Result.Item1);
                            //GetLabel().Text = String.Format("Detected {0} in {1} milliseconds.", t.Result.Item2, t.Result.Item3);
                        }
                        catch (Exception e)
                        {
                            String msg = e.Message.Replace(System.Environment.NewLine, " ");
                            SetMessage(msg);
                            return new Tuple<byte[], string, long>(null, msg, 0);
                        }
                    });
                t.Start();

#if __ANDROID__
                var result = await t;
                SetImage(t.Result.Item1);
                GetLabel().Text = t.Result.Item2;
#elif __IOS__
                var result = await t;
                SetImage(t.Result.Item1);
                GetLabel().Text = t.Result.Item2;
#elif !(__UNIFIED__)
                var result = await t;
                //SetImage(t.Result.Item1);
                GetLabel().Text = t.Result.Item2;
#endif
            };
        }

        private void OnButtonClicked(Object sender, EventArgs args)
        {
            LoadImages(new string[] { "surfers.jpg" });
        }

    }
}
