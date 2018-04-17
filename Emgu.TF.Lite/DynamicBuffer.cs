//----------------------------------------------------------------------------
//  Copyright (C) 2004-2018 by EMGU Corporation. All rights reserved.       
//----------------------------------------------------------------------------


using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using Emgu.TF.Util;

namespace Emgu.TF.Lite
{

    public class DynamicBuffer : Emgu.TF.Util.UnmanagedObject
    {
        public DynamicBuffer()
        {
            _ptr = TfLiteInvoke.tfeDynamicBufferCreate();
        }

        public void AddString(String str)
        {
            byte[] rawString = Encoding.ASCII.GetBytes(str);
            GCHandle handle = GCHandle.Alloc(rawString, GCHandleType.Pinned);
            TfLiteInvoke.tfeDynamicBufferAddString(_ptr, handle.AddrOfPinnedObject(), rawString.Length);
            handle.Free();
        }

        public void WriteToTensor(Tensor tensor)
        {
            TfLiteInvoke.tfeDynamicBufferWriteToTensor(_ptr, tensor);
        }
        
        /// <summary>
        /// Release all the unmanaged memory associated with this model
        /// </summary>
        protected override void DisposeObject()
        {
            if (IntPtr.Zero != _ptr)
            {
                TfLiteInvoke.tfeDynamicBufferRelease(ref _ptr);
            }
        }
    }


    public static partial class TfLiteInvoke
    {
        [DllImport(ExternLibrary, CallingConvention = TfLiteInvoke.TFCallingConvention)]
        internal static extern IntPtr tfeDynamicBufferCreate();

        [DllImport(ExternLibrary, CallingConvention = TfLiteInvoke.TFCallingConvention)]
        internal static extern void tfeDynamicBufferRelease(ref IntPtr buffer);

        [DllImport(ExternLibrary, CallingConvention = TfLiteInvoke.TFCallingConvention)]
        internal static extern void tfeDynamicBufferAddString(IntPtr buffer, IntPtr str, int len);

        [DllImport(ExternLibrary, CallingConvention = TfLiteInvoke.TFCallingConvention)]
        internal static extern void tfeDynamicBufferWriteToTensor(IntPtr buffer, IntPtr tensor);


    }
}
