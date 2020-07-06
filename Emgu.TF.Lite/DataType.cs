//----------------------------------------------------------------------------
//  Copyright (C) 2004-2020 by EMGU Corporation. All rights reserved.       
//----------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using Emgu.TF.Util;

namespace Emgu.TF.Lite
{
    /// <summary>
    /// Types supported by tensor
    /// </summary>
    public enum DataType
    {
        /// <summary>
        /// No type
        /// </summary>
        NoType = 0,
        /// <summary>
        /// single precision float
        /// </summary>
        Float32 = 1,
        /// <summary>
        /// Int32
        /// </summary>
        Int32 = 2,
        /// <summary>
        /// UInt8
        /// </summary>
        UInt8 = 3,
        /// <summary>
        /// Int64
        /// </summary>
        Int64 = 4,
        /// <summary>
        /// String
        /// </summary>
        String = 5,
        /// <summary>
        /// Bool
        /// </summary>
        Bool = 6,
        /// <summary>
        /// Bool
        /// </summary>
        Int16 = 7,
        /// <summary>
        /// Complex64
        /// </summary>
        Complex64
    }
}
