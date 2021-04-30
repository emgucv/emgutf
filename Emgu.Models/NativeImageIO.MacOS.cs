//----------------------------------------------------------------------------
//  Copyright (C) 2004-2021 by EMGU Corporation. All rights reserved.       
//----------------------------------------------------------------------------
#if __MACOS__
using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Runtime.InteropServices;
using AppKit;
using CoreGraphics;

namespace Emgu.Models
{
    /// <summary>
    /// Platform specific implementation of Image IO
    /// </summary>
    public partial class NativeImageIO
    {
        /// <summary>
        /// Read a NSImage, covert the data and save it to the native pointer
        /// </summary>
        /// <typeparam name="T">The type of the data to covert the image pixel values to. e.g. "float" or "byte"</typeparam>
        /// <param name="image">The input image</param>
        /// <param name="dest">The native pointer where the image pixels values will be saved to.</param>
        /// <param name="inputHeight">The height of the image, must match the height requirement for the tensor</param>
        /// <param name="inputWidth">The width of the image, must match the width requirement for the tensor</param>
        /// <param name="inputMean">The mean value, it will be subtracted from the input image pixel values</param>
        /// <param name="scale">The scale, after mean is subtracted, the scale will be used to multiply the pixel values</param>
        /// <param name="flipUpSideDown">If true, the image needs to be flipped up side down</param>
        /// <param name="swapBR">If true, will flip the Blue channel with the Red. e.g. If false, the tensor's color channel order will be RGB. If true, the tensor's color channle order will be BGR </param>
        public static void ReadImageToTensor<T>(
            NSImage image,
            IntPtr dest,
            int inputHeight = -1,
            int inputWidth = -1,
            float inputMean = 0.0f,
            float scale = 1.0f,
            bool flipUpSideDown = false,
            bool swapBR = false)
            where T : struct
        {
            if (inputHeight <= 0)
                inputHeight = (int)image.Size.Height;
            if (inputWidth <= 0)
                inputWidth = (int)image.Size.Width;

            int[] intValues = new int[inputWidth * inputHeight];

            System.Runtime.InteropServices.GCHandle handle = System.Runtime.InteropServices.GCHandle.Alloc(intValues, System.Runtime.InteropServices.GCHandleType.Pinned);
            using (CGImage cgimage = image.CGImage)
            using (CGColorSpace cspace = CGColorSpace.CreateDeviceRGB())
            using (CGBitmapContext context = new CGBitmapContext(
                handle.AddrOfPinnedObject(),
                inputWidth,
                inputHeight,
                8,
                inputWidth * 4,
                cspace,
                CGImageAlphaInfo.PremultipliedLast
                ))
            {
                context.DrawImage(new CGRect(new CGPoint(), new CGSize(inputWidth, inputHeight)), cgimage);
            }

            if (typeof(T) == typeof(float))
            {
                Emgu.TF.Util.Toolbox.Pixel32ToPixelFloat(
                    handle.AddrOfPinnedObject(),
                    inputWidth,
                    inputHeight,
                    inputMean,
                    scale,
                    flipUpSideDown,
                    swapBR,
                    dest);
            }
            else if (typeof(T) == typeof(byte))
            {
                Emgu.TF.Util.Toolbox.Pixel32ToPixelByte(
                    handle.AddrOfPinnedObject(),
                    inputWidth,
                    inputHeight,
                    inputMean,
                    scale,
                    flipUpSideDown,
                    swapBR,
                    dest);
            }
            else
            {
                throw new NotImplementedException(String.Format("Destination data type {0} is not supported.", typeof(T).ToString()));
            }
            handle.Free();

        }

        public static void DrawAnnotations(NSImage img, Annotation[] annotations)
        {
            img.LockFocus();

            NSColor redColor = NSColor.Red;
            redColor.Set();

            var context = NSGraphicsContext.CurrentContext;
            if (context == null)
                return;
            var cgcontext = context.CGContext;

            for (int i = 0; i < annotations.Length; i++)
            {
                float[] rects = ScaleLocation(annotations[i].Rectangle, (int)img.Size.Width, (int)img.Size.Height);
                CGRect cgRect = new CGRect(
                    rects[0],
                    rects[1],
                    rects[2] - rects[0],
                    rects[3] - rects[1]);
                NSFont font = NSFont.FromFontName("Arial", 20);
                var fontDictionary = Foundation.NSDictionary.FromObjectsAndKeys(
                    new Foundation.NSObject[] { font, NSColor.Red },
                    new Foundation.NSObject[] { NSStringAttributeKey.Font, NSStringAttributeKey.ForegroundColor });
                //CGSize size = text.StringSize(fontDictionary);
                CGPoint p = new CGPoint(cgRect.Location.X, img.Size.Height - cgRect.Location.Y);
                annotations[i].Label.DrawAtPoint(p, fontDictionary);
            }

            cgcontext.ScaleCTM(1, -1);
            cgcontext.TranslateCTM(0, -img.Size.Height);
            //context.IsFlipped = !context.IsFlipped;
            for (int i = 0; i < annotations.Length; i++)
            {
                float[] rects = ScaleLocation(annotations[i].Rectangle, (int)img.Size.Width, (int)img.Size.Height);
                CGRect cgRect = new CGRect(
                    rects[0],
                    rects[1],
                    rects[2] - rects[0],
                    rects[3] - rects[1]);
                NSBezierPath.StrokeRect(cgRect);

            }

            img.UnlockFocus();
        }

        /// <summary>
        /// Read the file and draw rectangles on it.
        /// </summary>
        /// <param name="fileName">The name of the file.</param>
        /// <param name="annotations">Annotations to be add to the image. Can consist of rectangles and labels</param>
        /// <returns>The image in Jpeg stream format</returns>
        public static JpegData ImageFileToJpeg(String fileName, Annotation[] annotations = null)
        {
            NSImage img;

            if (File.Exists(fileName))
                img = new NSImage(fileName); //full path
            else
                img = NSImage.ImageNamed(fileName); //image included in the package
           
            if (annotations != null && annotations.Length > 0)
                DrawAnnotations(img, annotations);

            var imageData = img.AsTiff();
            var imageRep = NSBitmapImageRep.ImageRepsWithData(imageData)[0] as NSBitmapImageRep;
            var jpegData = imageRep.RepresentationUsingTypeProperties(NSBitmapImageFileType.Jpeg, null);
            byte[] jpeg = new byte[jpegData.Length];
            System.Runtime.InteropServices.Marshal.Copy(jpegData.Bytes, jpeg, 0, (int)jpegData.Length);

            JpegData result = new JpegData();
            result.Raw = jpeg;
            result.Width = (int)img.Size.Width;
            result.Height = (int)img.Size.Height;

            return result;
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

            NSImage image = new NSImage(fileName);

            ReadImageToTensor<T>(image, dest, inputHeight, inputWidth, inputMean, scale, flipUpSideDown, swapBR);
                    }

        /// <summary>
        /// Converting raw pixel data to jpeg stream
        /// </summary>
        /// <param name="rawPixel">The raw pixel data</param>
        /// <param name="width">The width of the image</param>
        /// <param name="height">The height of the image</param>
        /// <param name="channels">The number of channels</param>
        /// <returns>The jpeg stream</returns>
        public static JpegData PixelToJpeg(byte[] rawPixel, int width, int height, int channels)
        {
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
                JpegData result = new JpegData();
                var rawJpegData = newImg.RepresentationUsingTypeProperties(NSBitmapImageFileType.Jpeg);

                byte[] raw = new byte[rawJpegData.Length];
                System.Runtime.InteropServices.Marshal.Copy(rawJpegData.Bytes, raw, 0,
                    (int)rawJpegData.Length);
                result.Raw = raw;
                result.Width = sz.Width;
                result.Height = sz.Height;
                return result;
            }
        }
    }
}

#endif