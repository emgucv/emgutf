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

    public class LogListenerSink : UnmanagedObject, ILogSink
    {
        private IntPtr _logSinkPtr;
        private bool _autoRemoveLogSink;

        public LogListenerSink(bool autoRegisterLogSink = false)
        {
            _ptr = TfInvoke.tfeLogListenerSinkCreate(ref _logSinkPtr);
            if (autoRegisterLogSink)
            {
                TfInvoke.AddLogSink(this);
            }
        }

        public String GetLog()
        {
            int size = TfInvoke.tfeLogListenerSinkGetLogSize(_ptr);
            IntPtr buffer = Marshal.AllocHGlobal(size + 1024); //Add 1024 bytes as extra buffer
            TfInvoke.tfeLogListenerSinkGet(_ptr, buffer);
            String msg = System.Runtime.InteropServices.Marshal.PtrToStringAnsi(buffer);
            Marshal.FreeHGlobal(buffer);
            return msg;
        }

        
        public void Clear()
        {
            TfInvoke.tfeLogListenerSinkClear(_ptr);
        }
        

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

        public IntPtr LogSinkPtr
        {
            get
            {
                return _logSinkPtr;
            }
        }
    }
}
