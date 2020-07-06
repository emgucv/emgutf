//----------------------------------------------------------------------------
//  Copyright (C) 2004-2020 by EMGU Corporation. All rights reserved.       
//----------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Emgu.TF.Util;
using System.Runtime.InteropServices;

namespace Emgu.TF
{
    /// <summary>
    /// A tensorflow function
    /// </summary>
    public class Function : UnmanagedObject
    {
        /// <summary>
        /// Construct and return the function whose FunctionDef representation is
        /// serialized in <paramref name="proto"/>
        /// </summary>
        /// <param name="proto">Serialized FunctionDef</param>
        /// <param name="status">The status</param>
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
                TfInvoke.tfeDeleteFunction(ref _ptr);
        }

        /// <summary>
        /// Write out a serialized representation of this Function (as a FunctionDef protocol
        /// message) 
        /// </summary>
        /// <param name="outputFuncDef">a serialized representation of this Function (as a FunctionDef protocol message) </param>
        /// <param name="status">The status</param>
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
