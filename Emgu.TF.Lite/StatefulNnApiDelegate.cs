//----------------------------------------------------------------------------
//  Copyright (C) 2004-2020 by EMGU Corporation. All rights reserved.       
//----------------------------------------------------------------------------


using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;

namespace Emgu.TF.Lite
{
    /// <summary>
    /// Stateful NNAPI delegate
    /// </summary>
    public class StatefulNnApiDelegate : Emgu.TF.Util.UnmanagedObject, IDelegate
    {
        private IntPtr _delegatePtr;

        /// <summary>
        /// Create a Stategul NNAPI delegate
        /// </summary>
        public StatefulNnApiDelegate()
        {
            _ptr = TfLiteInvoke.tfeStatefulNnApiDelegateCreate(ref _delegatePtr);
        }

        /// <summary>
        /// Pointer to the native Delegate object.
        /// </summary>
        IntPtr IDelegate.DelegatePtr
        {
            get { return _delegatePtr; }
        }

        /// <summary>
        /// Release all the unmanaged memory associated with this delegate
        /// </summary>
        protected override void DisposeObject()
        {
            if (IntPtr.Zero != _ptr)
            {
                TfLiteInvoke.tfeStatefulNnApiDelegateRelease(ref _ptr);
                _delegatePtr = IntPtr.Zero;
            }
        }
    }

    public static partial class TfLiteInvoke
    {
        [DllImport(ExternLibrary, CallingConvention = TfLiteInvoke.TFCallingConvention)]
        internal static extern IntPtr tfeStatefulNnApiDelegateCreate(ref IntPtr delegatePtr);

        [DllImport(ExternLibrary, CallingConvention = TfLiteInvoke.TFCallingConvention)]
        internal static extern void tfeStatefulNnApiDelegateRelease(ref IntPtr delegatePtr);

        private static StatefulNnApiDelegate _nnApiDelegate;

        /// <summary>
        /// Get the default stateful NNAPI delegate.
        /// </summary>
        public static StatefulNnApiDelegate DefaultNnApiDelegate
        {
            get
            {
                if (_nnApiDelegate == null)
                {
                    _nnApiDelegate = new StatefulNnApiDelegate();
                }

                return _nnApiDelegate;
            }

        }
    }
}
