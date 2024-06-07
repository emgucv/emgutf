//----------------------------------------------------------------------------
//  Copyright (C) 2004-2024 by EMGU Corporation. All rights reserved.       
//----------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Runtime.InteropServices;

namespace Emgu.Models
{
    /// <summary>
    /// Platform specific implementation of Image IO
    /// </summary>
    public partial class NativeImageIO
    {
        /// <summary>
        /// Scales the location of a rectangle in an image.
        /// </summary>
        /// <param name="location">An array of four float values representing the top left (x0, y0) and bottom right (x1, y1) corners of a rectangle. The values should be in the range of [0, 1].</param>
        /// <param name="imageWidth">The width of the image.</param>
        /// <param name="imageHeight">The height of the image.</param>
        /// <returns>An array of four float values representing the scaled top left (x0, y0) and bottom right (x1, y1) corners of the rectangle in the image.</returns>
        public static float[] ScaleLocation(float[] location, int imageWidth, int imageHeight)
        {
            float left = location[0] * imageWidth;
            float top = location[1] * imageHeight;
            float right = location[2] * imageWidth;
            float bottom = location[3] * imageHeight;
            return new float[] { left, top, right, bottom };
        }
    }
}

