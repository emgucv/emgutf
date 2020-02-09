//----------------------------------------------------------------------------
//  Copyright (C) 2004-2020 by EMGU Corporation. All rights reserved.       
//----------------------------------------------------------------------------


using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;

namespace Emgu.TF.Lite
{
    public class GpuDelegateV2 : Emgu.TF.Util.UnmanagedObject, IDelegate
    {
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
                TfLiteInvoke.tfeGpuDelegateV2Delete(ref _ptr);
            }
        }
    }

    public static partial class TfLiteInvoke
    {
        [DllImport(ExternLibrary, CallingConvention = TfLiteInvoke.TFCallingConvention)]
        internal static extern IntPtr tfeGpuDelegateV2Create();

        [DllImport(ExternLibrary, CallingConvention = TfLiteInvoke.TFCallingConvention)]
        internal static extern void tfeGpuDelegateV2Delete(ref IntPtr delegatePtr);

        private static GpuDelegateV2 _gpuDelegateV2;

        public static GpuDelegateV2 DefaultGpuDelegateV2
        {
            get
            {
                if (_gpuDelegateV2 == null)
                {
                    _gpuDelegateV2 = new GpuDelegateV2();
                }

                return _gpuDelegateV2;
            }

        }
    }
}
