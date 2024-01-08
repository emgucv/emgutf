//----------------------------------------------------------------------------
//  Copyright (C) 2004-2024 by EMGU Corporation. All rights reserved.       
//----------------------------------------------------------------------------
#if !(UNITY_EDITOR || UNITY_IOS || UNITY_ANDROID || UNITY_STANDALONE || __ANDROID__ || __IOS__ || __MACOS__)
using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Runtime.InteropServices;
using System.Drawing;
using Emgu.TF;


namespace Emgu.Models
{
    /// <summary>
    /// Platform specific implementation of Image IO
    /// </summary>
    public partial class NativeImageIO
    {
        /// <summary>
        /// Covert the Bitmap data and save it to the native pointer
        /// </summary>
        /// <typeparam name="T">The type of the data to covert the image pixel values to. e.g. "float" or "byte"</typeparam>
        /// <param name="bmp">The Bitmap to convert to tensor</param>
        /// <param name="dest">The native pointer where the image pixels values will be saved to.</param>
        /// <param name="inputHeight">The height of the image, must match the height requirement for the tensor</param>
        /// <param name="inputWidth">The width of the image, must match the width requirement for the tensor</param>
        /// <param name="inputMean">The mean value, it will be subtracted from the input image pixel values</param>
        /// <param name="scale">The scale, after mean is subtracted, the scale will be used to multiply the pixel values</param>
        /// <param name="flipUpSideDown">If true, the image needs to be flipped up side down</param>
        /// <param name="swapBR">If true, will flip the Blue channel with the Red. e.g. If false, the tensor's color channel order will be RGB. If true, the tensor's color channle order will be BGR </param>
        public static int ReadBitmapToTensor<T>(
            Bitmap bmp,
            IntPtr dest,
            int inputHeight = -1,
            int inputWidth = -1,
            float inputMean = 0.0f,
            float scale = 1.0f,
            bool flipUpSideDown = false,
            bool swapBR = false)
            where T : struct
        {
            if (inputHeight > 0 && inputWidth > 0 &&
                ((inputHeight != bmp.Height) || (inputWidth != bmp.Width)))
            {
                //resize bmp
                System.Drawing.Bitmap newBmp = new Bitmap(bmp, inputWidth, inputHeight);
                bmp.Dispose();
                bmp = newBmp;
            }

            int bmpWidth = bmp.Width;
            int bmpHeight = bmp.Height;
            System.Drawing.Imaging.BitmapData bd = new System.Drawing.Imaging.BitmapData();
            bmp.LockBits(
                new Rectangle(0, 0, bmpWidth, bmpHeight),
                System.Drawing.Imaging.ImageLockMode.ReadOnly,
                System.Drawing.Imaging.PixelFormat.Format24bppRgb, bd);

            try
            {
                if (typeof(T) == typeof(float))
                {
                    return Emgu.TF.Util.Toolbox.Pixel24ToPixelFloat(
                        bd.Scan0,
                        bmpWidth,
                        bmpHeight,
                        inputMean,
                        scale,
                        flipUpSideDown,
                        swapBR,
                        dest
                    );
                }
                else if (typeof(T) == typeof(byte))
                {
                    return Emgu.TF.Util.Toolbox.Pixel24ToPixelByte(
                        bd.Scan0,
                        bmpWidth,
                        bmpHeight,
                        inputMean,
                        scale,
                        flipUpSideDown,
                        swapBR,
                        dest
                    );
                }
                else
                {
                    throw new NotImplementedException(String.Format("Destination data type {0} is not supported.",
                        typeof(T).ToString()));
                }
            }
            finally
            {
                bmp.UnlockBits(bd);
            }
        }

        /// <summary>
        /// Read image files, covert the data and save it to the native pointer
        /// </summary>
        /// <typeparam name="T">The type of the data to covert the image pixel values to. e.g. "float" or "byte"</typeparam>
        /// <param name="fileNames">The name of the image files</param>
        /// <param name="dest">The native pointer where the image pixels values will be saved to.</param>
        /// <param name="inputHeight">The height of the image, must match the height requirement for the tensor</param>
        /// <param name="inputWidth">The width of the image, must match the width requirement for the tensor</param>
        /// <param name="inputMean">The mean value, it will be subtracted from the input image pixel values</param>
        /// <param name="scale">The scale, after mean is subtracted, the scale will be used to multiply the pixel values</param>
        /// <param name="flipUpSideDown">If true, the image needs to be flipped up side down</param>
        /// <param name="swapBR">If true, will flip the Blue channel with the Red. e.g. If false, the tensor's color channel order will be RGB. If true, the tensor's color channle order will be BGR </param>
        public static void ReadImageFilesToTensor<T>(
            String[] fileNames,
            IntPtr dest,
            int inputHeight = -1,
            int inputWidth = -1,
            float inputMean = 0.0f,
            float scale = 1.0f,
            bool flipUpSideDown = false,
            bool swapBR = false)
            where T : struct
        {
            IntPtr dataPtr = dest;
            for (int i = 0; i < fileNames.Length; i++)
            {
                String fileName = fileNames[i];
                if (!File.Exists(fileName))
                    throw new FileNotFoundException(String.Format("File {0} do not exist.", fileName));

                //Read the file using Bitmap class
                System.Drawing.Bitmap bmp = new Bitmap(fileName);

                int step = ReadBitmapToTensor<T>(bmp, dataPtr, inputHeight, inputWidth, inputMean, scale,
                    flipUpSideDown, swapBR);

                dataPtr = new IntPtr(dataPtr.ToInt64() + step);
                
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
            where T : struct
        {
            ReadImageFilesToTensor<T>(
                new string[] {fileName}, 
                dest, 
                inputHeight, 
                inputWidth, 
                inputMean, 
                scale,
                flipUpSideDown, swapBR);
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
            throw new NotImplementedException("PixelToJpeg Not Implemented in this platform");
        }


        /// <summary>
        /// Read the file and draw rectangles on it.
        /// </summary>
        /// <param name="fileName">The name of the file.</param>
        /// <param name="annotations">Annotations to be add to the image. Can consist of rectangles and labels</param>
        /// <returns>The image in Jpeg stream format</returns>
        public static JpegData ImageFileToJpeg(String fileName, Annotation[] annotations = null)
        {
            Bitmap img = new Bitmap(fileName);

            if (annotations != null)
            {
                using (Graphics g = Graphics.FromImage(img))
                {
                    for (int i = 0; i < annotations.Length; i++)
                    {
                        if (annotations[i].Rectangle != null)
                        {
                            float[] rects = ScaleLocation(annotations[i].Rectangle, img.Width, img.Height);
                            PointF origin = new PointF(rects[0], rects[1]);
                            RectangleF rect = new RectangleF(origin,
                                new SizeF(rects[2] - rects[0], rects[3] - rects[1]));
                            Pen redPen = new Pen(Color.Red, 3);
                            g.DrawRectangle(redPen, Rectangle.Round(rect));

                            String label = annotations[i].Label;
                            if (label != null)
                            {
                                g.DrawString(label, new Font(FontFamily.GenericSansSerif, 20f), Brushes.Red, origin);
                            }
                        }
                    }

                    g.Save();
                }
            }

            using (MemoryStream ms = new MemoryStream())
            {
                img.Save(ms, System.Drawing.Imaging.ImageFormat.Jpeg);
                JpegData result = new JpegData();
                result.Raw = ms.ToArray();
                result.Width = img.Size.Width;
                result.Height = img.Size.Height;
                return result;
            }
        }
    }
}

#endif