//----------------------------------------------------------------------------
//  Copyright (C) 2004-2021 by EMGU Corporation. All rights reserved.       
//----------------------------------------------------------------------------

/*

using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;

namespace Emgu.TF.Lite
{
    /// <summary>
    /// GPU delegate for iOS using metal
    /// </summary>
    public class GpuDelegate : Emgu.TF.Util.UnmanagedObject, IDelegate
    {
        /// <summary>
        /// GPU delegate for iOS using metal
        /// </summary>
        public GpuDelegate()
        {
            _ptr = TfLiteInvoke.tfeGpuDelegateCreate();
        }

        /// <summary>
        /// Pointer to the native Delegate object.
        /// </summary>
        IntPtr IDelegate.DelegatePtr
        {
            get { return _ptr; }
        }

        /// <summary>
        /// Release all the unmanaged memory associated with this delegate
        /// </summary>
        protected override void DisposeObject()
        {
            if (IntPtr.Zero != _ptr)
            {
                TfLiteInvoke.tfeGpuDelegateDelete(ref _ptr);
            }
        }
    }

    public static partial class TfLiteInvoke
    {
        [DllImport(ExternLibrary, CallingConvention = TfLiteInvoke.TfLiteCallingConvention)]
        internal static extern IntPtr tfeGpuDelegateCreate();

        [DllImport(ExternLibrary, CallingConvention = TfLiteInvoke.TfLiteCallingConvention)]
        internal static extern void tfeGpuDelegateDelete(ref IntPtr delegatePtr);

        private static GpuDelegate _gpuDelegate;

        /// <summary>
        /// Get the default GPU delegate for iOS using metal
        /// </summary>
        public static GpuDelegate DefaultGpuDelegate
        {
            get
            {
                if (_gpuDelegate == null)
                {
                    GpuDelegate d = new GpuDelegate();
                    if (d.Ptr != IntPtr.Zero)
                        _gpuDelegate = d;
                }

                return _gpuDelegate;
            }

        }
    }
}

*/