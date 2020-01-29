//----------------------------------------------------------------------------
//  Copyright (C) 2004-2020 by EMGU Corporation. All rights reserved.       
//----------------------------------------------------------------------------

/*
using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;

namespace Emgu.TF.Lite
{
    public class StatefulNnApiDelegate : Emgu.TF.Util.UnmanagedObject, IDelegate
    {
        public StatefulNnApiDelegate()
        {
            _ptr = TfLiteInvoke.tfeStatefulNnApiDelegateCreate();
        }

        /// <summary>
        /// Pointer to the native Delegate object.
        /// </summary>
        IntPtr IDelegate.DelegatePtr
        {
            get
            {
                return TfLiteInvoke.tfeStatefulNnApiDelegateGetDelegate();
            }
        }

        /// <summary>
        /// Release all the unmanaged memory associated with this delegate
        /// </summary>
        protected override void DisposeObject()
        {
            if (IntPtr.Zero != _ptr)
                TfLiteInvoke.tfeStatefulNnApiDelegateRelease(ref _ptr);
        }
    }

    public static partial class TfLiteInvoke
    {
        [DllImport(ExternLibrary, CallingConvention = TfLiteInvoke.TFCallingConvention)]
        internal static extern IntPtr tfeStatefulNnApiDelegateCreate();

        [DllImport(ExternLibrary, CallingConvention = TfLiteInvoke.TFCallingConvention)]
        internal static extern void tfeStatefulNnApiDelegateRelease(ref IntPtr delegatePtr);

        [DllImport(ExternLibrary, CallingConvention = TfLiteInvoke.TFCallingConvention)]
        internal static extern IntPtr tfeStatefulNnApiDelegateGetDelegate();

        //[DllImport(ExternLibrary, CallingConvention = TfLiteInvoke.TFCallingConvention)]
        //[return: MarshalAs(TfLiteInvoke.BoolMarshalType)]
        //internal static extern bool tfeStatefulNnApiDelegateIsSupported(IntPtr delegatePtr);
        
    }
}
*/