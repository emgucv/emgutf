//----------------------------------------------------------------------------
//  Copyright (C) 2004-2019 by EMGU Corporation. All rights reserved.       
//----------------------------------------------------------------------------

using System;
using System.Collections.Generic;

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
#else
using System.Drawing;
#endif

namespace Emgu.Models
{
    /// <summary>
    /// Platform specific implementation of Image IO
    /// </summary>
    public class NativeImageIO
    {
        /// <summary>
        /// Image annotation
        /// </summary>
        public class Annotation
        {
            /// <summary>
            /// The coordinates of the rectangle, the values are in the range of [0, 1], each rectangle contains 4 values, corresponding to the top left corner (x0, y0) and bottom right corner (x1, y1)
            /// </summary>
            public float[] Rectangle;

            /// <summary>
            /// The text to be drawn on the top left corner of the Rectangle
            /// </summary>
            public String Label;
        }

        /// <summary>
        /// Read an image file, covert the data and save it to the native pointer
        /// </summary>
        /// <typeparam name="T">The type of the data to covert the image pixel values to. e.g. "float" or "byte"</typeparam>
        /// <param name="fileName">The name of the image file</param>
        /// <param name="dest">The native pointer where the image pixels values will be saved to.</param>
        /// <param name="inputHeight">The height of the image, must match the height requirement for the tensor</param>
        /// <param name="inputWidth">The width of the image, must match the width requirement for the tensor</param>
        /// <param name="inputMean">The mean value, it will be substracted from the input image pixel values</param>
        /// <param name="scale">The scale, after mean is substracted, the scale will be used to multiply the pixel values</param>
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

#if __ANDROID__
            
            Android.Graphics.Bitmap bmp = BitmapFactory.DecodeFile(fileName);
            if (inputHeight > 0 || inputWidth >  0)
            {
                Bitmap resized = Bitmap.CreateScaledBitmap(bmp, inputWidth, inputHeight, false);
                bmp.Dispose();
                bmp = resized;                
            }

            if (flipUpSideDown)
            {
                Matrix matrix = new Matrix();
                matrix.PostScale(1, -1, bmp.Width / 2, bmp.Height / 2);
                Bitmap flipped = Bitmap.CreateBitmap(bmp, 0, 0, bmp.Width, bmp.Height, matrix, true);
                bmp.Dispose();
                bmp = flipped;
            }
            
            int[] intValues = new int[bmp.Width * bmp.Height];
            float[] floatValues = new float[bmp.Width * bmp.Height * 3];
            bmp.GetPixels(intValues, 0, bmp.Width, 0, 0, bmp.Width, bmp.Height);

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
                    byteValues[i] = (byte) floatValues[i];
                Marshal.Copy(byteValues, 0, dest, byteValues.Length);
            }
            else
            {
                throw new NotImplementedException(String.Format("Destination data type {0} is not supported.", typeof(T).ToString()));
            }

#elif __IOS__
            if (flipUpSideDown)
                throw new NotImplementedException("Flip Up Side Down is Not implemented");

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

            if (swapBR)
            {
                for (int i = 0; i < intValues.Length; ++i)
                {
                    int val = intValues[i];
                    floatValues[i * 3 + 0] = ((val & 0xFF) - inputMean) * scale;
                    floatValues[i * 3 + 1] = (((val >> 8) & 0xFF) - inputMean) * scale;
                    floatValues[i * 3 + 2] = (((val >> 16) & 0xFF) - inputMean) * scale;
                }
            } else
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

            //System.Runtime.InteropServices.Marshal.Copy(floatValues, 0, dest, floatValues.Length);
#elif __UNIFIED__
            if (flipUpSideDown)
                throw new NotImplementedException("Flip Up Side Down is Not implemented");
            //if (swapBR)
            //    throw new NotImplementedException("swapBR is Not implemented");
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
            if (swapBR)
            {
                for (int i = 0; i < intValues.Length; ++i)
                {
                    int val = intValues[i];
                    floatValues[i * 3 + 0] = ((val & 0xFF) - inputMean) * scale;
                    floatValues[i * 3 + 1] = (((val >> 8) & 0xFF) - inputMean) * scale;
                    floatValues[i * 3 + 2] = (((val >> 16) & 0xFF) - inputMean) * scale;
                }
            } else
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
                    byteValues[i] = (byte) floatValues[i];
                Marshal.Copy(byteValues, 0, dest, byteValues.Length);
            }
            else
            {
                throw new NotImplementedException(String.Format("Destination data type {0} is not supported.", typeof(T).ToString()));
            }

            //System.Runtime.InteropServices.Marshal.Copy(floatValues, 0, dest, floatValues.Length);
#elif UNITY_EDITOR || UNITY_IOS || UNITY_ANDROID || UNITY_STANDALONE
            Texture2D texture = ReadTexture2DFromFile(fileName);
            ReadTensorFromTexture2D<T>(texture, dest, inputHeight, inputWidth, inputMean, scale, flipUpSideDown, false);
#else

            if (Emgu.TF.Util.Platform.OperationSystem == OS.Windows)
            {
                //Read the file using Bitmap class
                System.Drawing.Bitmap bmp = new Bitmap(fileName);

                if (inputHeight > 0 || inputWidth > 0)
                {
                    //resize bmp
                    System.Drawing.Bitmap newBmp = new Bitmap(bmp, inputWidth, inputHeight);
                    bmp.Dispose();
                    bmp = newBmp;
                    //bmp.Save("tmp.png");
                }

                if (flipUpSideDown)
                {
                    bmp.RotateFlip(RotateFlipType.RotateNoneFlipY);
                }

                int bmpWidth = bmp.Width;
                int bmpHeight = bmp.Height;
                System.Drawing.Imaging.BitmapData bd = new System.Drawing.Imaging.BitmapData();
                bmp.LockBits(
                    new Rectangle(0, 0, bmpWidth, bmpHeight),
                    System.Drawing.Imaging.ImageLockMode.ReadOnly,
                    System.Drawing.Imaging.PixelFormat.Format24bppRgb, bd);
                int stride = bd.Stride;

                byte[] byteValues = new byte[bmpHeight * stride];
                Marshal.Copy(bd.Scan0, byteValues, 0, byteValues.Length);
                bmp.UnlockBits(bd);

                if (typeof(T) == typeof(float))
                {
                    int imageSize = bmpWidth * bmpHeight;
                    float[] floatValues = new float[imageSize * 3];
                    if (swapBR)
                    {
                        int idx = 0;
                        int rowOffset = 0;
                        for (int i = 0; i < bmpHeight; ++i)
                        {
                            int rowPtr = rowOffset;
                            for (int j = 0; j < bmpWidth; ++j)
                            {
                                float b = ((float)byteValues[rowPtr++] - inputMean) * scale;
                                float g = ((float)byteValues[rowPtr++] - inputMean) * scale;
                                float r = ((float)byteValues[rowPtr++] - inputMean) * scale;
                                floatValues[idx++] = r;
                                floatValues[idx++] = g;
                                floatValues[idx++] = b;
                            }
                            rowOffset += stride;
                        }
                    }
                    else
                    {
                        int idx = 0;
                        int rowOffset = 0;
                        for (int i = 0; i < bmpHeight; ++i)
                        {
                            int rowPtr = rowOffset;
                            for (int j = 0; j < bmpWidth; ++j)
                            {
                                floatValues[idx++] = ((float) byteValues[rowPtr++] - inputMean) * scale;
                                floatValues[idx++] = ((float) byteValues[rowPtr++] - inputMean) * scale;
                                floatValues[idx++] = ((float) byteValues[rowPtr++] - inputMean) * scale;
                            }
                            rowOffset += stride;
                        }
                    }
                    Marshal.Copy(floatValues, 0, dest, floatValues.Length);
                } else if (typeof(T) == typeof(byte))
                {
                    int imageSize = bmp.Width * bmp.Height;
                    if (swapBR)
                    {
                        int idx = 0;
                        for (int i = 0; i < bmpHeight; ++i)
                        {
                            int offset = i * stride;
                            for (int j = 0; j < bmpWidth; ++j)
                            {
                                byte b = (byte)(((float)byteValues[offset++] - inputMean) * scale);
                                byte g = (byte)(((float)byteValues[offset++] - inputMean) * scale);
                                byte r = (byte)(((float)byteValues[offset++] - inputMean) * scale);
                                byteValues[idx++] = r;
                                byteValues[idx++] = g;
                                byteValues[idx++] = b;
                            }
                        }
                    } else
                    {
                        int idx = 0;
                        for (int i = 0; i < bmpHeight; ++i)
                        {
                            int offset = i * stride;
                            for (int j = 0; j < bmpWidth * 3; ++j)
                            {
                                byteValues[idx++] = (byte)(((float)byteValues[offset++] - inputMean) * scale);
                            }
                        }
                    }
                    Marshal.Copy(byteValues, 0, dest, imageSize*3);

                } else
                {
                    throw new NotImplementedException(String.Format("Destination data type {0} is not supported.", typeof(T).ToString()));
                }
            }
            else //Unix
            {
                //if (flipUpSideDown)
                //    throw new NotImplementedException("Flip Up Side Down is Not implemented");

                throw new NotImplementedException("Not implemented");
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
            throw new NotImplementedException("PixelToJpeg Not Implemented in this platform");
#endif
        }


#if UNITY_EDITOR || UNITY_IOS || UNITY_ANDROID || UNITY_STANDALONE
        public static Texture2D ReadTexture2DFromFile(String fileName)
        {
            Texture2D texture = null;
            byte[] fileData;

            if (File.Exists(fileName))
            {
                fileData = File.ReadAllBytes(fileName);
                texture = new Texture2D(2, 2, TextureFormat.BGRA32, false);
                texture.LoadImage(fileData); //..this will auto-resize the texture dimensions.
            }
            return texture;
        }

        public static Texture2D Resize(Texture2D source, int newWidth, int newHeight)
        {
            source.filterMode = FilterMode.Bilinear;
            UnityEngine.RenderTexture rt = UnityEngine.RenderTexture.GetTemporary(newWidth, newHeight);
            rt.filterMode = FilterMode.Bilinear;

            UnityEngine.RenderTexture.active = rt;
            Graphics.Blit(source, rt);
            var nTex = new Texture2D(newWidth, newHeight);
            nTex.ReadPixels(new Rect(0, 0, newWidth, newHeight), 0, 0);
            nTex.Apply();
            UnityEngine.RenderTexture.active = null;
            UnityEngine.RenderTexture.ReleaseTemporary(rt);
            return nTex;
        }

        public static void ReadTensorFromTexture2D<T>(
            Texture2D texture, IntPtr dest, int inputHeight = -1, int inputWidth = -1,
            float inputMean = 0.0f, float scale = 1.0f, bool flipUpsideDown = false, bool swapBR = false) where T : struct
        {
            Color32[] colors;

            int width, height;
            if (inputHeight > 0 || inputWidth > 0)
            {
                Texture2D small = Resize(texture, inputWidth, inputHeight);
                colors = small.GetPixels32();
                width = inputWidth;
                height = inputHeight;
            }
            else
            {
                width = texture.width;
                height = texture.height;
                colors = texture.GetPixels32();
            }

            float[] floatValues = new float[colors.Length * 3];

            if (flipUpsideDown)
            {
                //handle flip upside down
                for (int i = 0; i < height; i++)
                    for (int j = 0; j < width; j++)
                    {
                        Color32 val = colors[(height - i - 1) * width + j];
                        int idx = i * width + j;
                        if (swapBR)
                        {
                            floatValues[idx * 3 + 0] = (val.b - inputMean) * scale;
                            floatValues[idx * 3 + 1] = (val.g - inputMean) * scale;
                            floatValues[idx * 3 + 2] = (val.r - inputMean) * scale;
                        }
                        else
                        {
                            floatValues[idx * 3 + 0] = (val.r - inputMean) * scale;
                            floatValues[idx * 3 + 1] = (val.g - inputMean) * scale;
                            floatValues[idx * 3 + 2] = (val.b - inputMean) * scale;
                        }
                    }
            }
            else
            {
                for (int i = 0; i < colors.Length; ++i)
                {
                    Color32 val = colors[i];
                    if (swapBR)
                    {
                        floatValues[i * 3 + 0] = (val.b - inputMean) * scale;
                        floatValues[i * 3 + 1] = (val.g - inputMean) * scale;
                        floatValues[i * 3 + 2] = (val.r - inputMean) * scale;
                    }
                    else
                    {
                        floatValues[i * 3 + 0] = (val.r - inputMean) * scale;
                        floatValues[i * 3 + 1] = (val.g - inputMean) * scale;
                        floatValues[i * 3 + 2] = (val.b - inputMean) * scale;
                    }
                }
            }

            if (typeof(T) == typeof(float))
            {
                Marshal.Copy(floatValues, 0, dest, floatValues.Length);
            }
            else if (typeof(T) == typeof(byte))
            {
                byte[] bValues = new byte[floatValues.Length];
                for (int i = 0; i < bValues.Length; ++i)
                {
                    bValues[i] = (byte)floatValues[i];
                }
                Marshal.Copy(bValues, 0, dest, bValues.Length);
            }
            else
            {
                throw new Exception(String.Format("Destination data type {0} is not supported.", typeof(T).ToString()));
            }
        }

#else

        private static float[] ScaleLocation(float[] location, int imageWidth, int imageHeight)
        {
            float left = location[0] * imageWidth;
            float top = location[1] * imageHeight;
            float right = location[2] * imageWidth;
            float bottom = location[3] * imageHeight;
            return new float[] { left, top, right, bottom };
        }

#if __MACOS__

        public static void DrawAnnotations(NSImage img, Annotation[] annotations)
        {
            img.LockFocus();

            NSColor redColor = NSColor.Red;
            redColor.Set();
            var context = NSGraphicsContext.CurrentContext;
            var cgcontext = context.CGContext;
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
#endif

        public class JpegData
        {
            public int Width { get; set; }
            public int Height { get; set; }
            public byte[] Raw { get; set; }
        }

        /// <summary>
        /// Read the file and draw rectangles on it.
        /// </summary>
        /// <param name="fileName">The name of the file.</param>
        /// <param name="annotations">Annotations to be add to the image. Can consist of rectangles and lables</param>
        /// <returns>The image in Jpeg stream format</returns>
        public static JpegData ImageFileToJpeg(String fileName, Annotation[] annotations = null)
        {
#if __ANDROID__
            BitmapFactory.Options options = new BitmapFactory.Options();
            options.InMutable = true;
            Android.Graphics.Bitmap bmp = BitmapFactory.DecodeFile(fileName, options);

            Android.Graphics.Paint p = new Android.Graphics.Paint();
            p.SetStyle(Paint.Style.Stroke);
            p.AntiAlias = true;
            p.Color = Android.Graphics.Color.Red;
            Canvas c = new Canvas(bmp);
                        
            for (int i = 0; i < annotations.Length; i++)
            {
                float[] rects = ScaleLocation(annotations[i].Rectangle, bmp.Width, bmp.Height);
                Android.Graphics.Rect r = new Rect((int)rects[0], (int) rects[1], (int) rects[2], (int) rects[3]);
                c.DrawRect(r, p);
            }     

            using (MemoryStream ms = new MemoryStream())
            {
                bmp.Compress(Bitmap.CompressFormat.Jpeg, 90, ms);
                JpegData result = new JpegData();
                result.Raw = ms.ToArray();
                result.Width = bmp.Width;
                result.Height = bmp.Height;
                return result;
            }
#elif __MACOS__
            NSImage img = NSImage.ImageNamed(fileName);

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
#elif __IOS__
            UIImage uiimage = new UIImage(fileName);

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

            var jpegData = imgWithRect.AsJPEG();
			byte[] jpeg = new byte[jpegData.Length];
			System.Runtime.InteropServices.Marshal.Copy(jpegData.Bytes, jpeg, 0, (int)jpegData.Length);
            JpegData result = new JpegData();
            result.Raw = jpeg;
            result.Width = (int)uiimage.Size.Width;
            result.Height = (int)uiimage.Size.Height;
            return result;
#else
            if (Emgu.TF.Util.Platform.OperationSystem == OS.Windows)
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
                                RectangleF rect = new RectangleF(origin, new SizeF(rects[2] - rects[0], rects[3] - rects[1]));
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
            else
            {
                throw new Exception("DrawResultsToJpeg Not implemented for this platform");
            }
#endif

        }

#endif
    }
}
