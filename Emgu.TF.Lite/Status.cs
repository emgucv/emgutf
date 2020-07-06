//----------------------------------------------------------------------------
//  Copyright (C) 2004-2020 by EMGU Corporation. All rights reserved.       
//----------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Runtime.InteropServices;
using Emgu.TF.Util;

namespace Emgu.TF.Lite
{
    /// <summary>
    /// Tensorflow lite status
    /// </summary>
    public enum Status
    {
        /// <summary>
        /// Ok
        /// </summary>
        Ok = 0,
        /// <summary>
        /// Error
        /// </summary>
        Error = 1
    }
}
