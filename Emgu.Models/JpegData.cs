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
    /// The Jpeg Data
    /// </summary>
    public class JpegData
    {
        /// <summary>
        /// The width of the image
        /// </summary>
        public int Width { get; set; }
        /// <summary>
        /// The height of the image
        /// </summary>
        public int Height { get; set; }
        /// <summary>
        /// The raw jpeg data
        /// </summary>
        public byte[] Raw { get; set; }
    }
}
