//----------------------------------------------------------------------------
//  Copyright (C) 2004-2020 by EMGU Corporation. All rights reserved.       
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
        private static float[] ScaleLocation(float[] location, int imageWidth, int imageHeight)
        {
            float left = location[0] * imageWidth;
            float top = location[1] * imageHeight;
            float right = location[2] * imageWidth;
            float bottom = location[3] * imageHeight;
            return new float[] { left, top, right, bottom };
        }
    }
}

