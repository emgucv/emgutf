//----------------------------------------------------------------------------
//  Copyright (C) 2004-2023 by EMGU Corporation. All rights reserved.       
//----------------------------------------------------------------------------


using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;

namespace Emgu.TF.Lite
{
    /// <summary>
    /// Gpu Delegate V2 for Android
    /// </summary>
    public class GpuDelegateV2 : Emgu.TF.Util.UnmanagedObject, IDelegate
    {
        /// <summary>
        /// Gpu Delegate V2 for Android
        /// </summary>
        public GpuDelegateV2()
        {
            _ptr = TfLiteInvoke.tfeGpuDelegateV2Create();
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
                TfLiteInvoke.tfeTfLiteDelegateRelease(ref _ptr);
            }
        }
    }

    public static partial class TfLiteInvoke
    {
        [DllImport(ExternLibrary, CallingConvention = TfLiteInvoke.TfLiteCallingConvention)]
        internal static extern IntPtr tfeGpuDelegateV2Create();

        //[DllImport(ExternLibrary, CallingConvention = TfLiteInvoke.TfLiteCallingConvention)]
        //internal static extern void tfeGpuDelegateV2Delete(ref IntPtr delegatePtr);

        private static GpuDelegateV2 _gpuDelegateV2;

        /// <summary>
        /// Get the default GPU delegate V2 for Android
        /// </summary>
        public static GpuDelegateV2 DefaultGpuDelegateV2
        {
            get
            {
                if (_gpuDelegateV2 == null)
                {
                    GpuDelegateV2 d = new GpuDelegateV2();
                    if (d.Ptr != IntPtr.Zero)
                        _gpuDelegateV2 = d;
                }

                return _gpuDelegateV2;
            }

        }
    }
}
