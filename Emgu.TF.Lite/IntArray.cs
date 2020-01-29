//----------------------------------------------------------------------------
//  Copyright (C) 2004-2020 by EMGU Corporation. All rights reserved.       
//----------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using Emgu.TF.Util;

namespace Emgu.TF.Lite
{
    /// <summary>
    /// A tensorflow integer array
    /// </summary>
    public class IntArray : Emgu.TF.Util.UnmanagedObject
    {
        private bool _needDispose;

        /// <summary>
        /// Create an int array of the given size
        /// </summary>
        /// <param name="size">The size of the IntArray</param>
        public IntArray(int size)
        {
            _needDispose = true;
            _ptr = TfLiteInvoke.tfeIntArrayCreate(size);
        }

        internal IntArray(IntPtr ptr, bool needDispose)
        {
            _ptr = ptr;
            _needDispose = needDispose;
        }

        /// <summary>
        /// Get a copy of the data in this integer array
        /// </summary>
        public int[] Data
        {
            get
            {
                int size = TfLiteInvoke.tfeIntArrayGetSize(_ptr);
                int[] d = new int[size];
                IntPtr dataPtr = TfLiteInvoke.tfeIntArrayGetData(_ptr);
                Marshal.Copy(dataPtr, d, 0, size);
                return d;
            }
        }

        /// <summary>
        /// Release all the unmanaged memory associated with this IntArray
        /// </summary>
        protected override void DisposeObject()
        {

            if (IntPtr.Zero != _ptr)
            {
                if (_needDispose)
                    TfLiteInvoke.tfeIntArrayRelease(ref _ptr);
                else
                    _ptr = IntPtr.Zero;
            }
        }
    }

    public static partial class TfLiteInvoke
    {
        [DllImport(ExternLibrary, CallingConvention = TfLiteInvoke.TFCallingConvention)]
        internal static extern IntPtr tfeIntArrayCreate(int size);

        [DllImport(ExternLibrary, CallingConvention = TfLiteInvoke.TFCallingConvention)]
        internal static extern int tfeIntArrayGetSize(IntPtr v);
        [DllImport(ExternLibrary, CallingConvention = TfLiteInvoke.TFCallingConvention)]
        internal static extern IntPtr tfeIntArrayGetData(IntPtr v);
        [DllImport(ExternLibrary, CallingConvention = TfLiteInvoke.TFCallingConvention)]
        internal static extern void tfeIntArrayRelease(ref IntPtr v);

    }
}
