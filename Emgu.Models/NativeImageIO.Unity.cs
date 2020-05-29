//----------------------------------------------------------------------------
//  Copyright (C) 2004-2020 by EMGU Corporation. All rights reserved.       
//----------------------------------------------------------------------------

#if UNITY_EDITOR || UNITY_IOS || UNITY_ANDROID || UNITY_STANDALONE
using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Runtime.InteropServices;

using UnityEngine;

namespace Emgu.Models
{
    /// <summary>
    /// Platform specific implementation of Image IO
    /// </summary>
    public partial class NativeImageIO
    {
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
            if (!File.Exists(fileName))
                throw new FileNotFoundException(String.Format("File {0} do not exist.", fileName));

            Texture2D texture = ReadTexture2DFromFile(fileName);
            ReadTensorFromTexture<T>(texture, dest, inputHeight, inputWidth, inputMean, scale, flipUpSideDown, swapBR);
        }

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

        public static void Resize(Texture source, Texture2D dst) 
        {
            int newWidth = dst.width;
            int newHeight = dst.height;
            source.filterMode = FilterMode.Bilinear;
            UnityEngine.RenderTexture rt = UnityEngine.RenderTexture.GetTemporary(newWidth, newHeight);
            rt.filterMode = FilterMode.Bilinear;

            UnityEngine.RenderTexture.active = rt;
            Graphics.Blit(source, rt);
            //var nTex = new Texture2D(newWidth, newHeight);
            dst.ReadPixels(new Rect(0, 0, newWidth, newHeight), 0, 0);
            dst.Apply();
            UnityEngine.RenderTexture.active = null;
            UnityEngine.RenderTexture.ReleaseTemporary(rt);
            //return nTex;
        }

        public static void ReadTensorFromTexture<T>(
            Texture texture, IntPtr dest, int inputHeight = -1, int inputWidth = -1,
            float inputMean = 0.0f, float scale = 1.0f, bool flipUpsideDown = false, bool swapBR = false) where T : struct
        {
            int width, height;
            if (inputWidth < 0)
                width = texture.width;
            else
            {
                width = inputWidth;
            }
            if (inputHeight < 0)
                height = texture.height;
            else
            {
                height = inputHeight;
            }

            Texture2D resized = new Texture2D(inputWidth, inputHeight);
            Resize(texture, resized);
            Color32[] colors = resized.GetPixels32();
            Texture2D.Destroy(resized);

            float[] floatValues = new float[colors.Length * 3];
            int idx = 0;
            if (flipUpsideDown)
            {
                //handle flip upside down
                for (int i = 0; i < height; i++)
                    for (int j = 0; j < width; j++)
                    {
                        Color32 val = colors[(height - i - 1) * width + j];
                        if (swapBR)
                        {
                            floatValues[idx++] = (val.b - inputMean) * scale;
                            floatValues[idx++] = (val.g - inputMean) * scale;
                            floatValues[idx++] = (val.r - inputMean) * scale;
                        }
                        else
                        {
                            floatValues[idx++] = (val.r - inputMean) * scale;
                            floatValues[idx++] = (val.g - inputMean) * scale;
                            floatValues[idx++] = (val.b - inputMean) * scale;
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
                        floatValues[idx++] = (val.b - inputMean) * scale;
                        floatValues[idx++] = (val.g - inputMean) * scale;
                        floatValues[idx++] = (val.r - inputMean) * scale;
                    }
                    else
                    {
                        floatValues[idx++] = (val.r - inputMean) * scale;
                        floatValues[idx++] = (val.g - inputMean) * scale;
                        floatValues[idx++] = (val.b - inputMean) * scale;
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

#region TextureDrawLine function from http://wiki.unity3d.com/index.php?title=TextureDrawLine
        public static void DrawLine(Texture2D tex, int x0, int y0, int x1, int y1, Color col)
        {
            int dy = (int)(y1 - y0);
            int dx = (int)(x1 - x0);
            int stepx, stepy;

            if (dy < 0) { dy = -dy; stepy = -1; }
            else { stepy = 1; }
            if (dx < 0) { dx = -dx; stepx = -1; }
            else { stepx = 1; }
            dy <<= 1;
            dx <<= 1;

            float fraction = 0;

            tex.SetPixel(x0, y0, col);
            if (dx > dy)
            {
                fraction = dy - (dx >> 1);
                while (Mathf.Abs(x0 - x1) > 1)
                {
                    if (fraction >= 0)
                    {
                        y0 += stepy;
                        fraction -= dx;
                    }
                    x0 += stepx;
                    fraction += dy;
                    tex.SetPixel(x0, y0, col);
                }
            }
            else
            {
                fraction = dx - (dy >> 1);
                while (Mathf.Abs(y0 - y1) > 1)
                {
                    if (fraction >= 0)
                    {
                        x0 += stepx;
                        fraction -= dy;
                    }
                    y0 += stepy;
                    fraction += dx;
                    tex.SetPixel(x0, y0, col);
                }
            }
        }
#endregion

        public static void DrawRect(Texture2D image, Rect rect, Color color)
        {
            DrawLine(image, (int)rect.position.x, (int)rect.position.y, (int)(rect.position.x + rect.width), (int)rect.position.y, color);
            DrawLine(image, (int)rect.position.x, (int)rect.position.y, (int)rect.position.x, (int)(rect.position.y + rect.height), color);
            DrawLine(image, (int)(rect.position.x + rect.width), (int)(rect.position.y + rect.height), (int)(rect.position.x + rect.width), (int)rect.position.y, color);
            DrawLine(image, (int)(rect.position.x + rect.width), (int)(rect.position.y + rect.height), (int)rect.position.x, (int)(rect.position.y + rect.height), color);
        }
    }
}

#endif