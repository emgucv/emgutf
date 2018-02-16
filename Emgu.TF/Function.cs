//----------------------------------------------------------------------------
//  Copyright (C) 2004-2018 by EMGU Corporation. All rights reserved.       
//----------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Emgu.TF.Util;
using System.Runtime.InteropServices;

namespace Emgu.TF
{

    public class Function : UnmanagedObject
    {
        public Function(byte[] proto, Status status = null)
        {
            using (StatusChecker checker = new StatusChecker(status))
            {
                GCHandle handle = GCHandle.Alloc(proto, GCHandleType.Pinned);
                _ptr = TfInvoke.tfeFunctionImportFunctionDef(handle.AddrOfPinnedObject(), proto.Length, checker.Status);
                handle.Free();
            }
        }

        /// <summary>
        /// Release all the unmanaged memory associated with this Buffer
        /// </summary>
        protected override void DisposeObject()
        {
            if (IntPtr.Zero != _ptr)
                TfInvoke.tfeDeleteBuffer(ref _ptr);
        }

        public void ToFunctionDef(Buffer outputFuncDef, Status status = null)
        {
            using (StatusChecker checker = new StatusChecker(status))
                TfInvoke.tfeFunctionToFunctionDef(_ptr, outputFuncDef, checker.Status);
        }

    }

    
    public static partial class TfInvoke
    {
        [DllImport(ExternLibrary, CallingConvention = TfInvoke.TFCallingConvention)]
        internal static extern IntPtr tfeFunctionImportFunctionDef(IntPtr proto, int protoLen, IntPtr status);

    
        [DllImport(ExternLibrary, CallingConvention = TfInvoke.TFCallingConvention)]
        internal static extern void tfeDeleteFunction(ref IntPtr function);

        [DllImport(ExternLibrary, CallingConvention = TfInvoke.TFCallingConvention)]
        internal static extern void tfeFunctionToFunctionDef(IntPtr func, IntPtr outputFuncDef, IntPtr status);

    }
}
