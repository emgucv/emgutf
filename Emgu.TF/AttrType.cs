//----------------------------------------------------------------------------
//  Copyright (C) 2004-2020 by EMGU Corporation. All rights reserved.       
//----------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Emgu.TF.Util;
using System.Runtime.InteropServices;

namespace Emgu.TF
{
    /// <summary>
    /// Describes the type of the value of an attribute on an operation.
    /// </summary>
    public enum AttrType
    {
        /// <summary>
        /// String
        /// </summary>
        String = 0,
        /// <summary>
        /// int
        /// </summary>
        Int = 1,
        /// <summary>
        /// float
        /// </summary>
        Float = 2,
        /// <summary>
        /// bool
        /// </summary>
        Bool = 3,
        /// <summary>
        /// type
        /// </summary>
        Type = 4,
        /// <summary>
        /// shape
        /// </summary>
        Shape = 5,
        /// <summary>
        /// tensor
        /// </summary>
        Tensor = 6,
        /// <summary>
        /// Placeholder
        /// </summary>
        Placeholder = 7,
        /// <summary>
        /// func
        /// </summary>
        Func = 8
    }
}
