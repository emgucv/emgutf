//----------------------------------------------------------------------------
//  Copyright (C) 2004-2019 by EMGU Corporation. All rights reserved.       
//----------------------------------------------------------------------------

using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;

namespace Emgu.TF.Lite
{
    public class NNAPIDelegate : Emgu.TF.Util.UnmanagedObject, IDelegate
    {
        public NNAPIDelegate()
        {
            _ptr = TfLiteInvoke.tfeNNAPIDelegateCreate();
        }

        /// <summary>
        /// Pointer to the native Delegate object.
        /// </summary>
        IntPtr IDelegate.DelegatePtr
        {
            get
            {
                return TfLiteInvoke.tfeNNAPIDelegateGetDelegate();
            }
        }

        /// <summary>
        /// Release all the unmanaged memory associated with this delegate
        /// </summary>
        protected override void DisposeObject()
        {
            if (IntPtr.Zero != _ptr)
                TfLiteInvoke.tfeNNAPIDelegateRelease(ref _ptr);
        }


        /// <summary>
        /// Return true if NNAPI is supported in the current platform
        /// </summary>
        public bool IsSupported
        {
            get
            {
                return TfLiteInvoke.tfeNNAPIDelegateIsSupported(_ptr);
            }
        }
    }

    public static partial class TfLiteInvoke
    {
        [DllImport(ExternLibrary, CallingConvention = TfLiteInvoke.TFCallingConvention)]
        internal static extern IntPtr tfeNNAPIDelegateCreate();

        [DllImport(ExternLibrary, CallingConvention = TfLiteInvoke.TFCallingConvention)]
        internal static extern void tfeNNAPIDelegateRelease(ref IntPtr delegatePtr);

        [DllImport(ExternLibrary, CallingConvention = TfLiteInvoke.TFCallingConvention)]
        internal static extern IntPtr tfeNNAPIDelegateGetDelegate();

        [DllImport(ExternLibrary, CallingConvention = TfLiteInvoke.TFCallingConvention)]
        [return: MarshalAs(TfLiteInvoke.BoolMarshalType)]
        internal static extern bool tfeNNAPIDelegateIsSupported(IntPtr delegatePtr);
        
    }
}
