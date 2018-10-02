//----------------------------------------------------------------------------
//  Copyright (C) 2004-2018 by EMGU Corporation. All rights reserved.       
//----------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Text;
using System.IO;
using System.Runtime.InteropServices;
using Emgu.TF.Util.TypeEnum;

#if UNITY_EDITOR || UNITY_IOS || UNITY_ANDROID || UNITY_STANDALONE
using UnityEngine;
#elif __ANDROID__
using Android.Graphics;
#elif __IOS__
using CoreGraphics;
using UIKit;
#elif __UNIFIED__
using AppKit;
using CoreGraphics;
#endif

namespace Emgu.Models
{
    /// <summary>
    /// Platform specific implementation of Image IO
    /// </summary>
    public class NativeImageIO
    {
        public static void ReadImageFileToTensor(String fileName, IntPtr dest, int inputHeight = -1, int inputWidth = -1, float inputMean = 0.0f, float scale = 1.0f)
        {
#if __ANDROID__
            Android.Graphics.Bitmap bmp = BitmapFactory.DecodeFile(fileName);

            if (inputHeight > 0 || inputWidth >  0)
            {
                Bitmap resized = Bitmap.CreateScaledBitmap(bmp, inputWidth, inputHeight, false);
                bmp.Dispose();
                bmp = resized;
            }
            int[] intValues = new int[bmp.Width * bmp.Height];
            float[] floatValues = new float[bmp.Width * bmp.Height * 3];
            bmp.GetPixels(intValues, 0, bmp.Width, 0, 0, bmp.Width, bmp.Height);
            for (int i = 0; i < intValues.Length; ++i)
            {
                int val = intValues[i];
                floatValues[i * 3 + 0] = (((val >> 16) & 0xFF) - inputMean) * scale;
                floatValues[i * 3 + 1] = (((val >> 8) & 0xFF) - inputMean) * scale;
                floatValues[i * 3 + 2] = ((val & 0xFF) - inputMean) * scale;
            }

            System.Runtime.InteropServices.Marshal.Copy(floatValues, 0, dest, floatValues.Length);

#elif __IOS__
            UIImage image = new UIImage(fileName);
			if (inputHeight > 0 || inputWidth > 0)
			{
                UIImage resized = image.Scale(new CGSize(inputWidth, inputHeight));
                image.Dispose();
				image = resized;
			}
            int[] intValues = new int[(int) (image.Size.Width * image.Size.Height)];
            float[] floatValues = new float[(int) (image.Size.Width * image.Size.Height * 3)];
            System.Runtime.InteropServices.GCHandle handle = System.Runtime.InteropServices.GCHandle.Alloc(intValues, System.Runtime.InteropServices.GCHandleType.Pinned);
            using (CGImage cgimage = image.CGImage)
            using (CGColorSpace cspace = CGColorSpace.CreateDeviceRGB())
            using (CGBitmapContext context = new CGBitmapContext(
                handle.AddrOfPinnedObject(),
                (nint)image.Size.Width,
                (nint)image.Size.Height,
                8,
                (nint)image.Size.Width * 4,
                cspace,
                CGImageAlphaInfo.PremultipliedLast
                ))
            {
                context.DrawImage(new CGRect(new CGPoint(), image.Size), cgimage);

            }
            handle.Free();

			for (int i = 0; i < intValues.Length; ++i)
			{
				int val = intValues[i];
				floatValues[i * 3 + 0] = (((val >> 16) & 0xFF) - inputMean) * scale;
				floatValues[i * 3 + 1] = (((val >> 8) & 0xFF) - inputMean) * scale;
				floatValues[i * 3 + 2] = ((val & 0xFF) - inputMean) * scale;
			}
			System.Runtime.InteropServices.Marshal.Copy(floatValues, 0, dest, floatValues.Length);
#elif __UNIFIED__
            NSImage image = new NSImage(fileName);
            if (inputHeight > 0 || inputWidth > 0)
            {
                NSImage resized = new NSImage(new CGSize(inputWidth, inputHeight));
                resized.LockFocus();
                image.DrawInRect(new CGRect(0, 0, inputWidth, inputHeight), CGRect.Empty, NSCompositingOperation.SourceOver, 1.0f);
                resized.UnlockFocus();       
                image.Dispose();
                image = resized;
            }
            int[] intValues = new int[(int) (image.Size.Width * image.Size.Height)];
            float[] floatValues = new float[(int) (image.Size.Width * image.Size.Height * 3)];
            System.Runtime.InteropServices.GCHandle handle = System.Runtime.InteropServices.GCHandle.Alloc(intValues, System.Runtime.InteropServices.GCHandleType.Pinned);
            using (CGImage cgimage = image.CGImage)
            using (CGColorSpace cspace = CGColorSpace.CreateDeviceRGB())
            using (CGBitmapContext context = new CGBitmapContext(
                handle.AddrOfPinnedObject(),
                (nint)image.Size.Width,
                (nint)image.Size.Height,
                8,
                (nint)image.Size.Width * 4,
                cspace,
                CGImageAlphaInfo.PremultipliedLast
                ))
            {
                context.DrawImage(new CGRect(new CGPoint(), image.Size), cgimage);

            }
            handle.Free();

            for (int i = 0; i < intValues.Length; ++i)
            {
                int val = intValues[i];
                floatValues[i * 3 + 0] = (((val >> 16) & 0xFF) - inputMean) * scale;
                floatValues[i * 3 + 1] = (((val >> 8) & 0xFF) - inputMean) * scale;
                floatValues[i * 3 + 2] = ((val & 0xFF) - inputMean) * scale;
            }
            System.Runtime.InteropServices.Marshal.Copy(floatValues, 0, dest, floatValues.Length);

#else
            if (Emgu.TF.Util.Platform.OperationSystem ==  OS.Windows)
            {
                //Do something for Windows
                System.Drawing.Bitmap bmp = new Bitmap(fileName);

                if (inputHeight > 0 || inputWidth > 0)
                {
                    //resize bmp
                    System.Drawing.Bitmap newBmp = new Bitmap(bmp, inputWidth, inputHeight);
                    bmp.Dispose();
                    bmp = newBmp;
                    //bmp.Save("tmp.png");
                }

                byte[] byteValues = new byte[bmp.Width * bmp.Height * 3];
                BitmapData bd = new BitmapData();

                bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height), ImageLockMode.ReadOnly,
                    PixelFormat.Format24bppRgb, bd);
                System.Runtime.InteropServices.Marshal.Copy(bd.Scan0, byteValues, 0, byteValues.Length);
                bmp.UnlockBits(bd);

                float[] floatValues = new float[bmp.Width * bmp.Height * 3];
                for (int i = 0; i < byteValues.Length; ++i)
                {
                    floatValues[i] = ((float)byteValues[i] - inputMean) * scale;
                }

                System.Runtime.InteropServices.Marshal.Copy(floatValues, 0, dest, floatValues.Length);
            }
            else
            {
                throw new Exception("Not implemented");
            }
#endif
        }

        public static byte[] PixelToJpeg(byte[] rawPixel, int width, int height, int channels)
        {
#if __ANDROID__
            if (channels != 4)
                throw new NotImplementedException("Only 4 channel pixel input is supported.");
            using (Bitmap bitmap = Bitmap.CreateBitmap(width, height, Bitmap.Config.Argb8888))
            using (MemoryStream ms = new MemoryStream())
            {
                IntPtr ptr = bitmap.LockPixels();
                //GCHandle handle = GCHandle.Alloc(colors, GCHandleType.Pinned);
                Marshal.Copy(rawPixel, 0, ptr, rawPixel.Length);

                bitmap.UnlockPixels();

                bitmap.Compress(Bitmap.CompressFormat.Jpeg, 90, ms);
                return ms.ToArray();
            }
#elif __IOS__
            if (channels != 3)
                throw new NotImplementedException("Only 3 channel pixel input is supported.");
            System.Drawing.Size sz = new System.Drawing.Size(width, height);
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
                return raw;
            }
#elif __UNIFIED__ //OSX
                    if (channels != 4)
                throw new NotImplementedException("Only 4 channel pixel input is supported.");
                                    System.Drawing.Size sz = new System.Drawing.Size(width, height);

            using (CGColorSpace cspace = CGColorSpace.CreateDeviceRGB())
            using (CGBitmapContext context = new CGBitmapContext(
                rawPixel,
                sz.Width, sz.Height,
                8,
                sz.Width * 4,
                cspace,
                CGBitmapFlags.PremultipliedLast | CGBitmapFlags.ByteOrder32Big))
            using (CGImage cgImage = context.ToImage())

            using (NSBitmapImageRep newImg = new NSBitmapImageRep(cgImage))
            {
                var jpegData = newImg.RepresentationUsingTypeProperties(NSBitmapImageFileType.Jpeg);

                byte[] raw = new byte[jpegData.Length];
                System.Runtime.InteropServices.Marshal.Copy(jpegData.Bytes, raw, 0,
                    (int)jpegData.Length);
                return raw;
            }
#else
            throw new NotImplementedException("Not Implemented");
#endif
        }
    }
}
