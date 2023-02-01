//----------------------------------------------------------------------------
//  Copyright (C) 2004-2023 by EMGU Corporation. All rights reserved.       
//----------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Runtime.InteropServices;
using Emgu.TF.Util;

namespace Emgu.TF.Util
{
    public static partial class Toolbox
    {
        /// <summary>
        /// A native implementation to convert (32-bit) pixels values to float tensor values 
        /// </summary>
        /// <param name="pixels">The raw pixel data of the image. 32-bit per pixel</param>
        /// <param name="width">The width of the image.</param>
        /// <param name="height">The height of the image.</param>
        /// <param name="inputMean">The input mean to be subtracted</param>
        /// <param name="scale">The scale to be multiplied</param>
        /// <param name="flipUpsideDown">If true, the pixels will be flipped upside down</param>
        /// <param name="swapBR">If true, the first and third output channels will be swapped.</param>
        /// <param name="result">The resulting pointer to the float array. Need to be initialized and big enough to hold all the float data.</param>
        /// <remarks>For internal use only. Improper call to this function can result in application crashing.</remarks>
		/// <returns>The number of bytes written</returns>
        [DllImport(Emgu.TF.Lite.TfLiteInvoke.ExternLibrary, CallingConvention = Emgu.TF.Lite.TfLiteInvoke.TfLiteCallingConvention, EntryPoint = "tfePixel32ToPixelFloat")]
        public static extern int Pixel32ToPixelFloat(
            IntPtr pixels,
            int width,
            int height,
            float inputMean,
            float scale,
            [MarshalAs(Emgu.TF.Lite.TfLiteInvoke.BoolMarshalType)]
            bool flipUpsideDown,
            [MarshalAs(Emgu.TF.Lite.TfLiteInvoke.BoolMarshalType)]
            bool swapBR,
            IntPtr result);

        /// <summary>
        /// A native implementation to convert (32-bit) pixels values to float tensor values 
        /// </summary>
        /// <param name="pixels">The raw pixel data of the image. 32-bit per pixel</param>
        /// <param name="width">The width of the image.</param>
        /// <param name="height">The height of the image.</param>
        /// <param name="inputMean">The input mean to be subtracted</param>
        /// <param name="scale">The scale to be multiplied</param>
        /// <param name="flipUpsideDown">If true, the pixels will be flipped upside down</param>
        /// <param name="swapBR">If true, the first and third output channels will be swapped.</param>
        /// <param name="result">The resulting pointer to the byte array. Need to be initialized and big enough to hold all the byte data.</param>
        /// <remarks>For internal use only. Improper call to this function can result in application crashing.</remarks>
		/// <returns>The number of bytes written</returns>
        [DllImport(Emgu.TF.Lite.TfLiteInvoke.ExternLibrary, CallingConvention = Emgu.TF.Lite.TfLiteInvoke.TfLiteCallingConvention, EntryPoint = "tfePixel32ToPixelByte")]
        public static extern int Pixel32ToPixelByte(
            IntPtr pixels,
            int width,
            int height,
            float inputMean,
            float scale,
            [MarshalAs(Emgu.TF.Lite.TfLiteInvoke.BoolMarshalType)]
            bool flipUpsideDown,
            [MarshalAs(Emgu.TF.Lite.TfLiteInvoke.BoolMarshalType)]
            bool swapBR,
            IntPtr result);

        /// <summary>
        /// A native implementation to convert (24-bit) pixels values to float tensor values 
        /// </summary>
        /// <param name="pixels">The raw pixel data of the image. 24-bit per pixel</param>
        /// <param name="width">The width of the image.</param>
        /// <param name="height">The height of the image.</param>
        /// <param name="inputMean">The input mean to be subtracted</param>
        /// <param name="scale">The scale to be multiplied</param>
        /// <param name="flipUpsideDown">If true, the pixels will be flipped upside down</param>
        /// <param name="swapBR">If true, the first and third output channels will be swapped.</param>
        /// <param name="result">The resulting pointer to the float array. Need to be initialized and big enough to hold all the float data.</param>
        /// <remarks>For internal use only. Improper call to this function can result in application crashing.</remarks>
		/// <returns>The number of bytes written</returns>
        [DllImport(Emgu.TF.Lite.TfLiteInvoke.ExternLibrary, CallingConvention = Emgu.TF.Lite.TfLiteInvoke.TfLiteCallingConvention, EntryPoint = "tfePixel24ToPixelFloat")]
        public static extern int Pixel24ToPixelFloat(
            IntPtr pixels,
            int width,
            int height,
            float inputMean,
            float scale,
            [MarshalAs(Emgu.TF.Lite.TfLiteInvoke.BoolMarshalType)]
            bool flipUpsideDown,
            [MarshalAs(Emgu.TF.Lite.TfLiteInvoke.BoolMarshalType)]
            bool swapBR,
            IntPtr result);

        /// <summary>
        /// A native implementation to convert (24-bit) pixels values to float tensor values 
        /// </summary>
        /// <param name="pixels">The raw pixel data of the image. 24-bit per pixel</param>
        /// <param name="width">The width of the image.</param>
        /// <param name="height">The height of the image.</param>
        /// <param name="inputMean">The input mean to be subtracted</param>
        /// <param name="scale">The scale to be multiplied</param>
        /// <param name="flipUpsideDown">If true, the pixels will be flipped upside down</param>
        /// <param name="swapBR">If true, the first and third output channels will be swapped.</param>
        /// <param name="result">The resulting pointer to the byte array. Need to be initialized and big enough to hold all the byte data.</param>
        /// <remarks>For internal use only. Improper call to this function can result in application crashing.</remarks>
		/// <returns>The number of bytes written</returns>
        [DllImport(Emgu.TF.Lite.TfLiteInvoke.ExternLibrary, CallingConvention = Emgu.TF.Lite.TfLiteInvoke.TfLiteCallingConvention, EntryPoint = "tfePixel24ToPixelByte")]
        public static extern int Pixel24ToPixelByte(
            IntPtr pixels,
            int width,
            int height,
            float inputMean,
            float scale,
            [MarshalAs(Emgu.TF.Lite.TfLiteInvoke.BoolMarshalType)]
            bool flipUpsideDown,
            [MarshalAs(Emgu.TF.Lite.TfLiteInvoke.BoolMarshalType)]
            bool swapBR,
            IntPtr result);
    }
}
