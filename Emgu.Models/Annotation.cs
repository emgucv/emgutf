//----------------------------------------------------------------------------
//  Copyright (C) 2004-2020 by EMGU Corporation. All rights reserved.       
//----------------------------------------------------------------------------

using System;
using System.Collections.Generic;

using System.Text;
using System.IO;


namespace Emgu.Models
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

}
