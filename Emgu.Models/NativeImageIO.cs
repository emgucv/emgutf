//----------------------------------------------------------------------------
//  Copyright (C) 2004-2018 by EMGU Corporation. All rights reserved.       
//----------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Runtime.InteropServices;
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
            UIImage image = new UIImage("surfers.jpg");
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
#else
            throw new Exception("Not implemented");
#endif
        }
    }
}
