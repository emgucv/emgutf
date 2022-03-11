//----------------------------------------------------------------------------
//  Copyright (C) 2004-2022 by EMGU Corporation. All rights reserved.       
//----------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using Emgu.TF.Util;

namespace Emgu.TF
{
    public static partial class TfInvoke
    {

        [DllImport(
            ExternLibrary,
            CallingConvention = TfInvoke.TfCallingConvention)]
        internal static extern IntPtr tfeLogForwarderSinkCreate(ref IntPtr logSinkPtr);

        [DllImport(
            ExternLibrary,
            CallingConvention = TfInvoke.TfCallingConvention)]
        internal static extern void tfeLogForwarderSinkRelease(ref IntPtr logSinkPtr);

    }

    public class LogForwarderSink : UnmanagedObject, ILogSink
    {
        private IntPtr _logSinkPtr;

        public LogForwarderSink()
        {
            _ptr = TfInvoke.tfeLogForwarderSinkCreate(ref _logSinkPtr);
        }

        protected override void DisposeObject()
        {
            if (_ptr != IntPtr.Zero)
            {
                TfInvoke.tfeLogForwarderSinkRelease(ref _ptr);
                _logSinkPtr = IntPtr.Zero;
            }

        }

        public IntPtr LogSinkPtr
        {
            get
            {
                return _logSinkPtr;
            }
        }
    }
}
