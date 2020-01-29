//----------------------------------------------------------------------------
//  Copyright (C) 2004-2020 by EMGU Corporation. All rights reserved.       
//----------------------------------------------------------------------------

using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;

namespace Emgu.TF.Lite
{
    /// <summary>
    /// Abstract interface that returns a pointer to the delegate
    /// </summary>
    public interface IDelegate
    {
        /// <summary>
        /// Pointer to the native Delegate object.
        /// </summary>
        IntPtr DelegatePtr { get; }
    }
}
