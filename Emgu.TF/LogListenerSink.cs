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
        internal static extern void tfeLogListenerSinkGet(IntPtr sink, IntPtr msg);

        [DllImport(
            ExternLibrary,
            CallingConvention = TfInvoke.TfCallingConvention)]
        internal static extern int tfeLogListenerSinkGetLogSize(IntPtr sink);

        [DllImport(
            ExternLibrary,
            CallingConvention = TfInvoke.TfCallingConvention)]
        internal static extern void tfeLogListenerSinkClear(IntPtr sink);


        [DllImport(
            ExternLibrary,
            CallingConvention = TfInvoke.TfCallingConvention)]
        internal static extern IntPtr tfeLogListenerSinkCreate(ref IntPtr logSinkPtr);

        [DllImport(
            ExternLibrary,
            CallingConvention = TfInvoke.TfCallingConvention)]
        internal static extern void tfeLogListenerSinkRelease(ref IntPtr logSinkPtr);

    }

    /// <summary>
    /// A LogSink that can be used to listen to logs
    /// </summary>
    public class LogListenerSink : UnmanagedObject, ILogSink
    {
        private IntPtr _logSinkPtr;
        private bool _autoRemoveLogSink;

        /// <summary>
        /// Create a log sink. By default it is not registered to received log. Use TfInboke.AddLogSink to register it.
        /// </summary>
        /// <param name="autoRegisterLogSink">If true, it will register the LogSink right after it is created, and will de-register the LogSink right before it is disposed.</param>
        public LogListenerSink(bool autoRegisterLogSink = false)
        {
            _ptr = TfInvoke.tfeLogListenerSinkCreate(ref _logSinkPtr);
            
            if (autoRegisterLogSink)
            {
                TfInvoke.AddLogSink(this);
            }
            _autoRemoveLogSink = autoRegisterLogSink;
        }

        /// <summary>
        /// Get the text that has been logged so far. It doesn't clear the log. Use the Clear() function to clear the log.
        /// </summary>
        /// <returns>Text that has been logged.</returns>
        public String GetLog()
        {
            int size = TfInvoke.tfeLogListenerSinkGetLogSize(_ptr);
            IntPtr buffer = Marshal.AllocHGlobal(size + 1024); //Add 1024 bytes as extra buffer
            TfInvoke.tfeLogListenerSinkGet(_ptr, buffer);
            String msg = System.Runtime.InteropServices.Marshal.PtrToStringAnsi(buffer);
            Marshal.FreeHGlobal(buffer);
            return msg;
        }

        /// <summary>
        /// Clear the log
        /// </summary>
        public void Clear()
        {
            TfInvoke.tfeLogListenerSinkClear(_ptr);
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

                TfInvoke.tfeLogListenerSinkRelease(ref _ptr);
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
