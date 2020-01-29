//----------------------------------------------------------------------------
//  Copyright (C) 2004-2020 by EMGU Corporation. All rights reserved.       
//----------------------------------------------------------------------------

#if __IOS__
using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Runtime.InteropServices;
using CoreGraphics;
using UIKit;

namespace Emgu.Models
{
    /// <summary>
    /// Platform specific implementation of Image IO
    /// </summary>
    public partial class NativeImageIO
    {
        /// <summary>
        /// Read an UIImage, covert the data and save it to the native pointer
        /// </summary>
        /// <typeparam name="T">The type of the data to covert the image pixel values to. e.g. "float" or "byte"</typeparam>
        /// <param name="imageOriginal">The input image</param>
        /// <param name="dest">The native pointer where the image pixels values will be saved to.</param>
        /// <param name="inputHeight">The height of the image, must match the height requirement for the tensor</param>
        /// <param name="inputWidth">The width of the image, must match the width requirement for the tensor</param>
        /// <param name="inputMean">The mean value, it will be subtracted from the input image pixel values</param>
        /// <param name="scale">The scale, after mean is subtracted, the scale will be used to multiply the pixel values</param>
        /// <param name="flipUpSideDown">If true, the image needs to be flipped up side down</param>
        /// <param name="swapBR">If true, will flip the Blue channel with the Red. e.g. If false, the tensor's color channel order will be RGB. If true, the tensor's color channle order will be BGR </param>
        public static void ReadImageToTensor<T>(
            UIImage imageOriginal,
            IntPtr dest,
            int inputHeight = -1,
            int inputWidth = -1,
            float inputMean = 0.0f,
            float scale = 1.0f,
            bool flipUpSideDown = false,
            bool swapBR = false)
            where T : struct
        {
            if (flipUpSideDown)
                throw new NotImplementedException("Flip Up Side Down is Not implemented");

            UIImage image;

            if (inputHeight > 0 || inputWidth > 0)
            {
                image = imageOriginal.Scale(new CGSize(inputWidth, inputHeight));
                //image.Dispose();
                //image = resized;
            } else
            {
                image = imageOriginal;
            }

            try
            {
                int[] intValues = new int[(int)(image.Size.Width * image.Size.Height)];
                float[] floatValues = new float[(int)(image.Size.Width * image.Size.Height * 3)];
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

                if (swapBR)
                {
                    for (int i = 0; i < intValues.Length; ++i)
                    {
                        int val = intValues[i];
                        floatValues[i * 3 + 0] = ((val & 0xFF) - inputMean) * scale;
                        floatValues[i * 3 + 1] = (((val >> 8) & 0xFF) - inputMean) * scale;
                        floatValues[i * 3 + 2] = (((val >> 16) & 0xFF) - inputMean) * scale;
                    }
                }
                else
                {
                    for (int i = 0; i < intValues.Length; ++i)
                    {
                        int val = intValues[i];
                        floatValues[i * 3 + 0] = (((val >> 16) & 0xFF) - inputMean) * scale;
                        floatValues[i * 3 + 1] = (((val >> 8) & 0xFF) - inputMean) * scale;
                        floatValues[i * 3 + 2] = ((val & 0xFF) - inputMean) * scale;
                    }
                }

                if (typeof(T) == typeof(float))
                {
                    Marshal.Copy(floatValues, 0, dest, floatValues.Length);
                }
                else if (typeof(T) == typeof(byte))
                {
                    //copy float to bytes
                    byte[] byteValues = new byte[floatValues.Length];
                    for (int i = 0; i < floatValues.Length; i++)
                        byteValues[i] = (byte)floatValues[i];
                    Marshal.Copy(byteValues, 0, dest, byteValues.Length);
                }
                else
                {
                    throw new NotImplementedException(String.Format("Destination data type {0} is not supported.", typeof(T).ToString()));
                }
            }
            finally
            {
                if (image != imageOriginal)
                    image.Dispose();
            }
        }

        public static UIImage DrawAnnotations(UIImage uiimage, Annotation[] annotations)
        {
            UIGraphics.BeginImageContextWithOptions(uiimage.Size, false, 0);
            var context = UIGraphics.GetCurrentContext();

            uiimage.Draw(new CGPoint());
            context.SetStrokeColor(UIColor.Red.CGColor);
            context.SetLineWidth(2);

            for (int i = 0; i < annotations.Length; i++)
            {
                float[] rects = ScaleLocation(
                    annotations[i].Rectangle,
                    (int)uiimage.Size.Width,
                    (int)uiimage.Size.Height);
                CGRect cgRect = new CGRect(
                                           (nfloat)rects[0],
                                           (nfloat)rects[1],
                                           (nfloat)(rects[2] - rects[0]),
                                           (nfloat)(rects[3] - rects[1]));
                context.AddRect(cgRect);
                context.DrawPath(CGPathDrawingMode.Stroke);
            }
            context.ScaleCTM(1, -1);
            context.TranslateCTM(0, -uiimage.Size.Height);
            for (int i = 0; i < annotations.Length; i++)
            {
                float[] rects = ScaleLocation(
                    annotations[i].Rectangle,
                    (int)uiimage.Size.Width,
                    (int)uiimage.Size.Height);
                context.SelectFont("Helvetica", 18, CGTextEncoding.MacRoman);
                context.SetFillColor((nfloat)1.0, (nfloat)0.0, (nfloat)0.0, (nfloat)1.0);
                context.SetTextDrawingMode(CGTextDrawingMode.Fill);
                context.ShowTextAtPoint(rects[0], uiimage.Size.Height - rects[1], annotations[i].Label);
            }
            UIImage imgWithRect = UIGraphics.GetImageFromCurrentImageContext();
            UIGraphics.EndImageContext();
            return imgWithRect;
        }

        /// <summary>
        /// Read the file and draw rectangles on it.
        /// </summary>
        /// <param name="fileName">The name of the file.</param>
        /// <param name="annotations">Annotations to be add to the image. Can consist of rectangles and labels</param>
        /// <returns>The image in Jpeg stream format</returns>
        public static JpegData ImageFileToJpeg(String fileName, Annotation[] annotations = null)
        {
            UIImage uiimage = new UIImage(fileName);

            UIImage imgWithRect = DrawAnnotations(uiimage, annotations);
            var jpegData = imgWithRect.AsJPEG();
			byte[] jpeg = new byte[jpegData.Length];
			System.Runtime.InteropServices.Marshal.Copy(jpegData.Bytes, jpeg, 0, (int)jpegData.Length);
            JpegData result = new JpegData();
            result.Raw = jpeg;
            result.Width = (int)uiimage.Size.Width;
            result.Height = (int)uiimage.Size.Height;
            return result;
        }

        /// <summary>
        /// Converting raw pixel data to jpeg stream
        /// </summary>
        /// <param name="rawPixel">The raw pixel data</param>
        /// <param name="width">The width of the image</param>
        /// <param name="height">The height of the image</param>
        /// <param name="channels">The number of channels</param>
        /// <returns>The jpeg stream</returns>
        public static byte[] PixelToJpeg(byte[] rawPixel, int width, int height, int channels)
        {
            if (channels != 3)
                throw new NotImplementedException("Only 3 channel pixel input is supported.");
            //System.Drawing.Size sz = new System.Drawing.Size(width, height);
            GCHandle handle = GCHandle.Alloc(rawPixel, GCHandleType.Pinned);
            using (CGColorSpace cspace = CGColorSpace.CreateDeviceRGB())
            using (CGBitmapContext context = new CGBitmapContext(
                handle.AddrOfPinnedObject(),
                width, height,
                8,
                width * 3,
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
        }

        /// <summary>
        /// Read an image file, covert the data and save it to the native pointer
        /// </summary>
        /// <typeparam name="T">The type of the data to covert the image pixel values to. e.g. "float" or "byte"</typeparam>
        /// <param name="fileName">The name of the image file</param>
        /// <param name="dest">The native pointer where the image pixels values will be saved to.</param>
        /// <param name="inputHeight">The height of the image, must match the height requirement for the tensor</param>
        /// <param name="inputWidth">The width of the image, must match the width requirement for the tensor</param>
        /// <param name="inputMean">The mean value, it will be subtracted from the input image pixel values</param>
        /// <param name="scale">The scale, after mean is subtracted, the scale will be used to multiply the pixel values</param>
        /// <param name="flipUpSideDown">If true, the image needs to be flipped up side down</param>
        /// <param name="swapBR">If true, will flip the Blue channel with the Red. e.g. If false, the tensor's color channel order will be RGB. If true, the tensor's color channle order will be BGR </param>
        public static void ReadImageFileToTensor<T>(
            String fileName,
            IntPtr dest,
            int inputHeight = -1,
            int inputWidth = -1,
            float inputMean = 0.0f,
            float scale = 1.0f, 
            bool flipUpSideDown = false,
            bool swapBR = false)
            where T: struct
        {
            if (!File.Exists(fileName))
                throw new FileNotFoundException(String.Format("File {0} do not exist.", fileName));

            UIImage image = new UIImage(fileName);

            ReadImageToTensor<T>(image, dest, inputHeight, inputWidth, inputMean, scale, flipUpSideDown, swapBR);
                    }
}
}

#endif