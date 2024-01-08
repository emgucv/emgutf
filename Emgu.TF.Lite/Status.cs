//----------------------------------------------------------------------------
//  Copyright (C) 2004-2024 by EMGU Corporation. All rights reserved.       
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
        Error = 1,

        /// <summary>
        /// Generally referring to an error from a TfLiteDelegate itself. 
        /// </summary>
        DelegateError = 2,

        /// <summary>
        /// Generally referring to an error in applying a delegate due to
        /// incompatibility between runtime and delegate, e.g., this error is returned
        /// when trying to apply a TfLite delegate onto a model graph that's already
        /// immutable.
        /// </summary>
        ApplicationError = 3,

        /// <summary>
        /// Generally referring to serialized delegate data not being found.
        /// See tflite::delegates::Serialization.
        /// </summary>
        DelegateDataNotFound = 4,

        /// <summary>
        /// Generally referring to data-writing issues in delegate serialization.
        /// See tflite::delegates::Serialization.
        /// </summary>
        DelegateDataWriteError = 5,
    }
}
