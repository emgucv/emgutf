//----------------------------------------------------------------------------
//  Copyright (C) 2004-2018 by EMGU Corporation. All rights reserved.       
//----------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Runtime.InteropServices;
using Emgu.Models;
using Emgu.TF;
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

namespace Emgu.TF.Models
{
    public class ImageIO
    {
        /// <summary>
        /// Encode the tensor to JPEG image
        /// </summary>
        /// <param name="image">The image tensor. Should be a single channel or 3 channel 4-D tensor</param>
        /// <param name="scale">The tensor value will be scaled with this values first</param>
        /// <param name="inputMean">The mean value will be added back to the image after scaling is done</param>
        /// <returns></returns>
        public static byte[] EncodeJpeg(Tensor image, float scale = 1.0f, float inputMean = 0.0f)
        {
            var graph = new Graph();
            Operation input = graph.Placeholder(DataType.Float);

            Tensor scaleTensor = new Tensor(scale);
            Operation scaleOp = graph.Const(scaleTensor, scaleTensor.Type, opName: "scale");
            Operation scaled = graph.Mul(input, scaleOp);
            Tensor mean = new Tensor(inputMean);
            Operation meanOp = graph.Const(mean, mean.Type, opName: "mean");
            Operation added = graph.Add(scaled, meanOp);
            Operation uintCaster = graph.Cast(added, DstT: DataType.Uint8); //cast to float
            Operation squeezed = graph.Squeeze(uintCaster, new long[] { 0 });
            Operation jpegRaw = graph.EncodeJpeg(squeezed);
            Session session = new Session(graph);

            Tensor[] raw = session.Run(new Output[] { input }, new Tensor[] { image },
                new Output[] { jpegRaw });

            return raw[0].DecodeString();
        }

        public static byte[] TensorToPixel(Tensor imageTensorF, float scale = 1.0f, int dstChannels = 3, Status status = null)
        {
            int[] dim = imageTensorF.Dim;
            if (dim[3] != 3)
            {
                throw new NotImplementedException("Only 3 channel tensor input is supported.");
            }

            using (StatusChecker checker = new StatusChecker(status))
            {
                var graph = new Graph();
                Operation input = graph.Placeholder(imageTensorF.Type);

                //multiply with scale
                Tensor scaleTensor = new Tensor(scale);
                Operation scaleOp = graph.Const(scaleTensor, scaleTensor.Type, opName: "scale");
                Operation scaled = graph.Mul(input, scaleOp);

                //cast to byte
                Operation byteCaster = graph.Cast(scaled, DstT: DataType.Uint8);

                //run the graph
                Session session = new Session(graph);
                Tensor[] imageResults = session.Run(new Output[] { input }, new Tensor[] { imageTensorF },
                    new Output[] { byteCaster });

                //get the raw data
                byte[] raw = imageResults[0].Flat<byte>();

                if (dstChannels == 3)
                {
                    return raw;
                }
                else if (dstChannels == 4)
                {
                    int pixelCount = raw.Length / 3;
                    byte[] colors = new byte[pixelCount * 4];
                    for (int i = 0; i < pixelCount; i++)
                    {
                        colors[i * 4] = raw[i * 3];
                        colors[i * 4 + 1] = raw[i * 3 + 1];
                        colors[i * 4 + 2] = raw[i * 3 + 2];
                        colors[i * 4 + 3] = (byte)255;
                    }
                    return colors;

                }
                else
                {
                    throw new Exception(String.Format("Output channel count of {0} is not supported", dstChannels));
                }

            }
        }

#if UNITY_EDITOR || UNITY_IOS || UNITY_ANDROID || UNITY_STANDALONE

        public static Tensor ReadTensorFromImageFile(String fileName, int inputHeight = -1, int inputWidth = -1, float inputMean = 0.0f, float scale = 1.0f, bool flipUpsideDown = false)
        {
            Texture2D texture = NativeImageIO.ReadTexture2DFromFile(fileName);
            return ReadTensorFromTexture2D(texture, inputHeight, inputWidth, inputMean, scale, flipUpsideDown);
        }

        public static Tensor ReadTensorFromTexture2D(
            Texture2D texture, int inputHeight = -1, int inputWidth = -1,
            float inputMean = 0.0f, float scale = 1.0f, bool flipUpsideDown = false)
        {
        #region Get the RGBA raw data as imgOrig
            Tensor imgOrig = new Tensor(DataType.Uint8, new int[] { 1, texture.height, texture.width, 4 });
            Color32[] colors = texture.GetPixels32(); //32bit RGBA
            GCHandle colorsHandle = GCHandle.Alloc(colors, GCHandleType.Pinned);
            Emgu.TF.TfInvoke.tfeMemcpy(imgOrig.DataPointer, colorsHandle.AddrOfPinnedObject(), colors.Length * Marshal.SizeOf(typeof(Color32)));
            colorsHandle.Free();
        #endregion

            var graph = new Graph();
            Operation input = graph.Placeholder(DataType.Uint8);

            #region Cast to float
            Operation floatCaster = graph.Cast(input, DstT: DataType.Float); //cast to float
            #endregion

            #region slice out the alpha channel
            Tensor sliceBegin = new Tensor(new int[] { 0, 0, 0, 0 });
            Operation sliceBeginOp = graph.Const(sliceBegin, sliceBegin.Type, opName: "sliceBegin");
            Tensor sliceSize = new Tensor(new int[] { 1, -1, -1, 3 });
            Operation sliceSizeOp = graph.Const(sliceSize, sliceSize.Type, opName: "sliceSize");
            Operation sliced = graph.Slice(floatCaster, sliceBeginOp, sliceSizeOp, "slice");
            #endregion

            #region crop and resize image
            Tensor boxes = new Tensor(DataType.Float, new int[] {1, 4});
            float[] boxCorners;
            if (flipUpsideDown)
            {
                boxCorners = new float[] { 1f, 0f, 0f, 1f }; //y1, x1, y2, x2    
            }
            else
            {
                boxCorners = new float[] { 0f, 0f, 1f, 1f }; //y1, x1, y2, x2    
            }
            Marshal.Copy(boxCorners, 0, boxes.DataPointer, boxCorners.Length);
            Operation boxesOp = graph.Const(boxes, boxes.Type, "boxes");
            

            Tensor boxIdx = new Tensor(new int[] {0});
            Operation boxIdxOp = graph.Const(boxIdx, boxIdx.Type, "boxIdx");
            int width, height;
            if (inputHeight > 0 || inputWidth > 0)
            {
                width = inputWidth;
                height = inputHeight;
            }
            else
            {
                width = texture.width;
                height = texture.height;
            }
            Tensor cropSize = new Tensor(new int[] {height, width });
            Operation cropSizeOp = graph.Const(cropSize, cropSize.Type, "cropSize");
            Operation resized = graph.CropAndResize(sliced, boxesOp, boxIdxOp, cropSizeOp);
            #endregion

            Tensor mean = new Tensor(inputMean);
            Operation meanOp = graph.Const(mean, mean.Type, opName: "mean");
            Operation substracted = graph.Sub(resized, meanOp);

            Tensor scaleTensor = new Tensor(scale);
            Operation scaleOp = graph.Const(scaleTensor, scaleTensor.Type, opName: "scale");
            Operation scaled = graph.Mul(substracted, scaleOp);

            //Operation scaled = graph.
            Session session = new Session(graph);

            Tensor[] imageResults = session.Run(new Output[] { input }, new Tensor[] { imgOrig },
                new Output[] { scaled });
            return imageResults[0];
        }


        public static Texture2D Resize(Texture2D source, int newWidth, int newHeight)
        {
            source.filterMode = FilterMode.Bilinear;
            RenderTexture rt = RenderTexture.GetTemporary(newWidth, newHeight);
            rt.filterMode = FilterMode.Bilinear;
            
            RenderTexture.active = rt;
            Graphics.Blit(source, rt);
            var nTex = new Texture2D(newWidth, newHeight);
            nTex.ReadPixels(new Rect(0, 0, newWidth, newHeight), 0, 0);
            nTex.Apply();
            RenderTexture.active = null;
            RenderTexture.ReleaseTemporary(rt);
            return nTex;
        }

        /*
        public static Tensor ReadTensorFromTexture2D_V0(
            Texture2D texture, int inputHeight = -1, int inputWidth = -1,
            float inputMean = 0.0f, float scale = 1.0f, bool flipUpsideDown = false)
        {
            Color32[] colors;
            Tensor t;
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
            t = new Tensor(DataType.Float, new int[] { 1, height, width, 3 });

            float[] floatValues = new float[colors.Length*3];

            if (flipUpsideDown)
            {
                for (int i = 0; i < height; i++)
                for (int j = 0; j < width; j++)
                {
                    Color32 val = colors[(height - i - 1) * width + j];
                    int idx = i * width + j;
                    floatValues[idx * 3 + 0] = (val.r - inputMean) * scale;
                    floatValues[idx * 3 + 1] = (val.g - inputMean) * scale;
                    floatValues[idx * 3 + 2] = (val.b - inputMean) * scale;
                }
            }
            else
            {
                for (int i = 0; i < colors.Length; ++i)
                {
                    Color32 val = colors[i];
                    floatValues[i * 3 + 0] = (val.r - inputMean) * scale;
                    floatValues[i * 3 + 1] = (val.g - inputMean) * scale;
                    floatValues[i * 3 + 2] = (val.b - inputMean) * scale;
                }
            }
            

            System.Runtime.InteropServices.Marshal.Copy(floatValues, 0, t.DataPointer, floatValues.Length);

            return t;
        }*/
#else

        public static byte[] TensorToJpeg(Tensor stylizedImage, float scale = 1.0f, Status status = null)
        {
#if __ANDROID__
            byte[] rawPixel = TensorToPixel(stylizedImage, scale, 4);
            int[] dim = stylizedImage.Dim;
            return NativeImageIO.PixelToJpeg(rawPixel, dim[2], dim[1], 4);
#elif __IOS__
            byte[] rawPixel = TensorToPixel(stylizedImage, scale, 3);
            int[] dim = stylizedImage.Dim;
            return NativeImageIO.PixelToJpeg(rawPixel, dim[2], dim[1], 3);
#elif __UNIFIED__ //Mac OSX
            byte[] rawPixel = TensorToPixel(stylizedImage, scale, 4);
            int[] dim = stylizedImage.Dim;
            return NativeImageIO.PixelToJpeg(rawPixel, dim[2], dim[1], 4);
#else
            return null;
#endif
        }


        public static Tensor ReadTensorFromImageFile<T>(
            String fileName, 
            int inputHeight = -1, 
            int inputWidth = -1, 
            float inputMean = 0.0f, 
            float scale = 1.0f,
            bool flipUpSideDown = false,
            bool swapBR = false,
            Status status = null) where T: struct
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

            Tensor t =  new Tensor(DataType.Float, new int[] {1, bmp.Height, bmp.Width, 3});
            System.Runtime.InteropServices.Marshal.Copy(floatValues, 0, t.DataPointer, floatValues.Length);
            return t;
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

            Tensor t = new Tensor(DataType.Float, new int[] { 1, (int)image.Size.Height, (int) image.Size.Width, 3 });
			System.Runtime.InteropServices.Marshal.Copy(floatValues, 0, t.DataPointer, floatValues.Length);
			return t;
#else

            if (Emgu.TF.Util.Platform.OperationSystem == OS.Windows)
            {
                Tensor t;
                if (typeof(T) == typeof(float))
                    t = new Tensor(DataType.Float, new int[] { 1, (int)inputHeight, (int)inputWidth, 3 });
                else if (typeof(T) == typeof(byte))
                    t = new Tensor(DataType.Uint8, new int[] { 1, (int)inputHeight, (int)inputWidth, 3 });
                else
                {
                    throw new Exception(String.Format("Convertion to tensor of type {0} is not implemented", typeof(T)));
                }

                NativeImageIO.ReadImageFileToTensor<T>(
                    fileName,
                    t.DataPointer,
                    inputHeight,
                    inputWidth,
                    inputMean,
                    scale,
                    flipUpSideDown,
                    swapBR
                    );
                return t;
            }
            else
            {
                if (flipUpSideDown)
                    throw new NotImplementedException("Flip Up Side Down is Not implemented");
                if (swapBR)
                    throw new NotImplementedException("swapBR is Not implemented");

                //Mac OS or Linux
                using (StatusChecker checker = new StatusChecker(status))
                {
                    var graph = new Graph();
                    Operation input = graph.Placeholder(DataType.String);

                    Operation jpegDecoder = graph.DecodeJpeg(input, 3); //dimension 3

                    Operation floatCaster = graph.Cast(jpegDecoder, DstT: DataType.Float); //cast to float

                    Tensor axis = new Tensor(0);
                    Operation axisOp = graph.Const(axis, axis.Type, opName: "axis");
                    Operation dimsExpander = graph.ExpandDims(floatCaster, axisOp); //turn it to dimension [1,3]

                    Operation resized;
                    bool resizeRequired = (inputHeight > 0) && (inputWidth > 0);
                    if (resizeRequired)
                    {
                        Tensor size = new Tensor(new int[] { inputHeight, inputWidth }); // new size;
                        Operation sizeOp = graph.Const(size, size.Type, opName: "size");
                        resized = graph.ResizeBilinear(dimsExpander, sizeOp); //resize image
                    }
                    else
                    {
                        resized = dimsExpander;
                    }

                    Tensor mean = new Tensor(inputMean);
                    Operation meanOp = graph.Const(mean, mean.Type, opName: "mean");
                    Operation substracted = graph.Sub(resized, meanOp);

                    Tensor scaleTensor = new Tensor(scale);
                    Operation scaleOp = graph.Const(scaleTensor, scaleTensor.Type, opName: "scale");
                    Operation scaled = graph.Mul(substracted, scaleOp);

                    Session session = new Session(graph);
                    Tensor imageTensor = Tensor.FromString(File.ReadAllBytes(fileName), status);
                    Tensor[] imageResults = session.Run(new Output[] { input }, new Tensor[] { imageTensor },
                        new Output[] { scaled });
                    return imageResults[0];

                }
            }
#endif
        }
#endif
    }
}
