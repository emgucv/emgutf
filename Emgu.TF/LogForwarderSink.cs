//----------------------------------------------------------------------------
//  Copyright (C) 2004-2024 by EMGU Corporation. All rights reserved.       
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

    /// <summary>
    /// A LogSink, when registered, automatically forward all logs to TfInvoke.LogMsgReceived event handler
    /// </summary>
    public class LogForwarderSink : UnmanagedObject, ILogSink
    {
        private IntPtr _logSinkPtr;
        private bool _autoRemoveLogSink;

        /// <summary>
        /// Create a log sink, when registered, automatically forward all logs to TfInvoke.LogMsgReceived event handler.
        /// By default it is not registered to received log.
        /// Use TfInboke.AddLogSink to register it.
        /// </summary>
        /// <param name="autoRegisterLogSink">If true, it will register the LogSink right after it is created, and will de-register the LogSink right before it is disposed.</param>
        public LogForwarderSink(bool autoRegisterLogSink = false)
        {
            _ptr = TfInvoke.tfeLogForwarderSinkCreate(ref _logSinkPtr);

            if (autoRegisterLogSink)
            {
                TfInvoke.AddLogSink(this);
            }
            _autoRemoveLogSink = autoRegisterLogSink;
        }

        /// <summary>
        /// Release all the memory associated with this LogSink. 
        /// </summary>
        protected override void DisposeObject()
        {
            if (_ptr != IntPtr.Zero)
            {
                if (_autoRemoveLogSink)
                {
                    TfInvoke.RemoveLogSink(this);
                }

                TfInvoke.tfeLogForwarderSinkRelease(ref _ptr);
                _logSinkPtr = IntPtr.Zero;
            }
        }

        /// <summary>
        /// Get the native LogSink pointer
        /// </summary>
        public IntPtr LogSinkPtr
        {
            get
            {
                return _logSinkPtr;
            }
        }
    }
}
