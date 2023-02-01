//----------------------------------------------------------------------------
//  Copyright (C) 2004-2023 by EMGU Corporation. All rights reserved.       
//----------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using Emgu.TF.Util;

namespace Emgu.TF
{
    /// <summary>
    /// The interface for LogSink (tensorflow::TFLogSink)
    /// </summary>
    public interface ILogSink
    {
        /// <summary>
        /// Return a pointer to the native tensorflow::TFLogSink
        /// </summary>
        IntPtr LogSinkPtr { get; }
    }

}
