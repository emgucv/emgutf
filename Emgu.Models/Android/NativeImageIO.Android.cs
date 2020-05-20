//----------------------------------------------------------------------------
//  Copyright (C) 2004-2020 by EMGU Corporation. All rights reserved.       
//----------------------------------------------------------------------------

#if __ANDROID__
using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Runtime.InteropServices;

using Android.Graphics;
using Android.Provider;

namespace Emgu.Models
{
    /// <summary>
    /// Platform specific implementation of Image IO
    /// </summary>
    public static partial class NativeImageIO
    {
        /// <summary>
        /// Read the file and draw rectangles on it.
        /// </summary>
        /// <param name="fileName">The name of the file.</param>
        /// <param name="annotations">Annotations to be add to the image. Can consist of rectangles and labels</param>
        /// <returns>The image in Jpeg stream format</returns>
        public static Emgu.Models.JpegData ImageFileToJpeg(String fileName, Annotation[] annotations = null)
        {
            using (Android.Graphics.Bitmap bmp = ImageFileToBitmap(fileName, annotations))
            {
                return bmp.ToJpeg();
            }
        }

        /// <summary>
        /// Convert bitmap to jpeg
        /// </summary>
        /// <param name="bmp">The android bitmap</param>
        /// <returns>The jpeg representation</returns>
        public static Emgu.Models.JpegData ToJpeg(this Android.Graphics.Bitmap bmp)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                bmp.Compress(Bitmap.CompressFormat.Jpeg, 90, ms);
                JpegData result = new JpegData();
                result.Raw = ms.ToArray();
                result.Width = bmp.Width;
                result.Height = bmp.Height;
                return result;
            }
        }

        /// <summary>
        /// Read the file and draw rectangles on it.
        /// </summary>
        /// <param name="fileName">The name of the file.</param>
        /// <param name="annotations">Annotations to be add to the image. Can consist of rectangles and labels</param>
        /// <returns>The image in Bitmap format</returns>
        public static Bitmap ImageFileToBitmap(String fileName, Annotation[] annotations = null)
        {
            using (BitmapFactory.Options options = new BitmapFactory.Options())
            {
                options.InMutable = true;
                Android.Graphics.Bitmap bmp = BitmapFactory.DecodeFile(fileName, options);
                DrawAnnotations(bmp, annotations);
                return bmp;
            }
        }

        /// <summary>
        /// Draw the annotations on the Bitmap
        /// </summary>
        /// <param name="bmp">The image where annotations will be drawn to</param>
        /// <param name="annotations">The annotations to be drawn</param>
        public static void DrawAnnotations(Android.Graphics.Bitmap bmp, Annotation[] annotations = null)
        {
            if (annotations != null)
            {
                using (Android.Graphics.Paint p = new Android.Graphics.Paint())
                using (Canvas c = new Canvas(bmp))
                {
                    p.AntiAlias = true;
                    p.Color = Android.Graphics.Color.Red;

                    p.TextSize = 20;
                    for (int i = 0; i < annotations.Length; i++)
                    {
                        p.SetStyle(Paint.Style.Stroke);
                        float[] rects = ScaleLocation(annotations[i].Rectangle, bmp.Width, bmp.Height);
                        Android.Graphics.Rect r = new Rect((int)rects[0], (int)rects[1], (int)rects[2],
                            (int)rects[3]);
                        c.DrawRect(r, p);

                        p.SetStyle(Paint.Style.Fill);
                        c.DrawText(annotations[i].Label, (int)rects[0], (int)rects[1], p);
                    }
                }
            }
        }

        /// <summary>
        /// Read an image file, covert the data and save it to the native pointer
        /// </summary>
        /// <typeparam name="T">The type of the data to covert the image pixel values to. e.g. "float" or "byte"</typeparam>
        /// <param name="bmp">The bitmap</param>
        /// <param name="dest">The native pointer where the image pixels values will be saved to. The pixel will have 3 color channles (BGR or RGB depends on the swapBR flag).</param>
        /// <param name="inputHeight">The height of the image, must match the height requirement for the tensor</param>
        /// <param name="inputWidth">The width of the image, must match the width requirement for the tensor</param>
        /// <param name="inputMean">The mean value, it will be subtracted from the input image pixel values</param>
        /// <param name="scale">The scale, after mean is subtracted, the scale will be used to multiply the pixel values</param>
        /// <param name="flipUpSideDown">If true, the image needs to be flipped up side down</param>
        /// <param name="swapBR">If true, will flip the Blue channel with the Red. e.g. If false, the tensor's color channel order will be RGB. If true, the tensor's color channel order will be BGR </param>
        public static void ReadBitmapToTensor<T>(
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

            Bitmap resized;
            if (inputHeight > 0 || inputWidth > 0)
            {
                resized = Bitmap.CreateScaledBitmap(bmp, inputWidth, inputHeight, false);
            }
            else
            {
                resized = bmp;
            }

            try
            {
                Bitmap flipped;
                if (flipUpSideDown)
                {
                    Matrix matrix = new Matrix();
                    matrix.PostScale(1, -1, resized.Width / 2, resized.Height / 2);
                    flipped = Bitmap.CreateBitmap(resized, 0, 0, resized.Width, resized.Height, matrix, true);
                }
                else
                {
                    flipped = resized;
                }

                try
                {
                    if (swapBR)
                    {
                        float[] swapBRColorTransform = new float[]
                        {
                            0.0f, 0.0f, 1.0f, 0.0f, 0.0f,
                            0.0f, 1.0f, 0.0f, 0.0f, 0.0f,
                            1.0f, 0.0f, 0.0f, 0.0f, 0.0f,
                            0.0f, 0.0f, 0.0f, 1.0f, 0.0f
                        };
                        ColorMatrix swapBrColorMatrix = new ColorMatrix();
                        swapBrColorMatrix.Set(swapBRColorTransform);
                        ColorMatrixColorFilter colorFilter = new ColorMatrixColorFilter(swapBrColorMatrix);
                        Paint paint = new Paint();
                        paint.SetColorFilter(colorFilter);

                        //Bitmap swapBrBitmap = Bitmap.CreateBitmap(bmp, 0, 0, bmp.Width, bmp.Height);
                        //Canvas canvas = new Canvas(swapBrBitmap);
                        //canvas.DrawBitmap(swapBrBitmap, 0, 0, paint);

                        Canvas canvas = new Canvas(flipped);
                        canvas.DrawBitmap(flipped, 0, 0, paint);
                    }

                    int[] intValues = new int[flipped.Width * flipped.Height];
                    float[] floatValues = new float[flipped.Width * flipped.Height * 3];
                    flipped.GetPixels(intValues, 0, flipped.Width, 0, 0, flipped.Width, flipped.Height);

                    for (int i = 0; i < intValues.Length; ++i)
                    {
                        int val = intValues[i];
                        floatValues[i * 3 + 0] = (((val >> 16) & 0xFF) - inputMean) * scale;
                        floatValues[i * 3 + 1] = (((val >> 8) & 0xFF) - inputMean) * scale;
                        floatValues[i * 3 + 2] = ((val & 0xFF) - inputMean) * scale;
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
                            byteValues[i] = (byte) floatValues[i];
                        Marshal.Copy(byteValues, 0, dest, byteValues.Length);
                    }
                    else
                    {
                        throw new NotImplementedException(String.Format("Destination data type {0} is not supported.",
                            typeof(T).ToString()));
                    }
                }
                finally
                {
                    if (flipped != resized)
                        flipped.Dispose();
                }
            }
            finally
            {
                if (resized != bmp)
                    resized.Dispose();
            }

        }

        /// <summary>
        /// Read an image file, covert the data and save it to the native pointer
        /// </summary>
        /// <typeparam name="T">The type of the data to covert the image pixel values to. e.g. "float" or "byte"</typeparam>
        /// <param name="fileName">The name of the image file</param>
        /// <param name="dest">The native pointer where the image pixels values will be saved to. The pixel will have 3 color channles (BGR or RGB depends on the swapBR flag).</param>
        /// <param name="inputHeight">The height of the image, must match the height requirement for the tensor</param>
        /// <param name="inputWidth">The width of the image, must match the width requirement for the tensor</param>
        /// <param name="inputMean">The mean value, it will be subtracted from the input image pixel values</param>
        /// <param name="scale">The scale, after mean is subtracted, the scale will be used to multiply the pixel values</param>
        /// <param name="flipUpSideDown">If true, the image needs to be flipped up side down</param>
        /// <param name="swapBR">If true, will flip the Blue channel with the Red. e.g. If false, the tensor's color channel order will be RGB. If true, the tensor's color channel order will be BGR </param>
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
            if (!File.Exists(fileName))
                throw new FileNotFoundException(String.Format("File {0} do not exist.", fileName));

            using (Android.Graphics.Bitmap bmp = BitmapFactory.DecodeFile(fileName))
                ReadBitmapToTensor<T>(bmp, dest, inputHeight, inputWidth, inputMean, scale, flipUpSideDown, swapBR);
            
        }

        /// <summary>
        /// Converting raw pixel data to jpeg stream
        /// </summary>
        /// <param name="rawPixel">The raw pixel data</param>
        /// <param name="width">The width of the image</param>
        /// <param name="height">The height of the image</param>
        /// <param name="channels">The number of channels</param>
        /// <returns>The jpeg stream</returns>
        public static Emgu.Models.JpegData PixelToJpeg(byte[] rawPixel, int width, int height, int channels)
        {
            using (Bitmap bmp = PixelToBitmap(rawPixel, width, height, channels))
                return bmp.ToJpeg();
        }

        public static Bitmap PixelToBitmap(byte[] rawPixel, int width, int height, int channels)
        {
            if (channels != 4)
                throw new NotImplementedException("Only 4 channel pixel input is supported.");

            Bitmap bitmap = Bitmap.CreateBitmap(width, height, Bitmap.Config.Argb8888);
            IntPtr ptr = bitmap.LockPixels();
            Marshal.Copy(rawPixel, 0, ptr, rawPixel.Length);
            bitmap.UnlockPixels();
            return bitmap;
        }
    }
}

#endif