//----------------------------------------------------------------------------
//  Copyright (C) 2004-2021 by EMGU Corporation. All rights reserved.       
//----------------------------------------------------------------------------


using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;

namespace Emgu.TF.Lite
{
    /// <summary>
    /// XNNPackDelegate
    /// </summary>
    public class XNNPackDelegate : Emgu.TF.Util.UnmanagedObject, IDelegate
    {
        /// <summary>
        /// XNNPackDelegate
        /// </summary>
        public XNNPackDelegate()
        {
            _ptr = TfLiteInvoke.tfeXNNPackDelegateCreateDefault();
        }

        /// <summary>
        /// XNNPackDelegate
        /// </summary>
        public XNNPackDelegate(int numThreads)
        {
            _ptr = TfLiteInvoke.tfeXNNPackDelegateCreate(numThreads);
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
        internal static extern IntPtr tfeXNNPackDelegateCreateDefault();

        [DllImport(ExternLibrary, CallingConvention = TfLiteInvoke.TfLiteCallingConvention)]
        internal static extern IntPtr tfeXNNPackDelegateCreate(int numThreads);

        [DllImport(ExternLibrary, CallingConvention = TfLiteInvoke.TfLiteCallingConvention)]
        internal static extern void tfeTfLiteDelegateRelease(ref IntPtr delegatePtr);

        private static XNNPackDelegate _xnnPackDelegate;

        /// <summary>
        /// Get the default XNNPackDelegate
        /// </summary>
        public static XNNPackDelegate DefaultXNNPackDelegate
        {
            get
            {
                if (_xnnPackDelegate == null)
                {
                    XNNPackDelegate d = new XNNPackDelegate();
                    if (d.Ptr != IntPtr.Zero)
                        _xnnPackDelegate = d;
                }

                return _xnnPackDelegate;
            }

        }

        public static bool HasXNNPack
        {
            get
            {
                XNNPackDelegate d = DefaultXNNPackDelegate;
                return d != null;
            }
        }
    }
}
