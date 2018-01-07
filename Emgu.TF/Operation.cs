//----------------------------------------------------------------------------
//  Copyright (C) 2004-2018 by EMGU Corporation. All rights reserved.       
//----------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using Emgu.TF.Util;

namespace Emgu.TF
{
    /// <summary>
    /// Tensorflow operation
    /// </summary>
    public class Operation
    {
        private IntPtr _ptr;

        internal Operation(IntPtr ptr)
        {
            _ptr = ptr;
        }

        /// <summary>
        /// Get the unmanaged pointer to the Operation
        /// </summary>
        public IntPtr Ptr
        {
            get { return _ptr; }
        }

        /// <summary>
        /// Implicit operator for IntPtr
        /// </summary>
        /// <param name="obj">The Operation</param>
        /// <returns>The Operation pointer for this object</returns>
        public static implicit operator IntPtr(Operation obj)
        {
            return obj == null ? IntPtr.Zero : obj._ptr;
        }

        /// <summary>
        /// Implicit operator for Output
        /// </summary>
        /// <param name="obj">The operation</param>
        /// <returns>The first output of this operation</returns>
        public static implicit operator Output(Operation obj)
        {
            return obj == null ? null : new Output(obj, 0);
        }

        /// <summary>
        /// Get the name of the operation
        /// </summary>
        public String Name
        {
            get { return Marshal.PtrToStringAnsi(TfInvoke.tfeOperationName(_ptr)); }
        }

        /// <summary>
        /// Get the operation type
        /// </summary>
        public String OpType
        {
            get { return Marshal.PtrToStringAnsi(TfInvoke.tfeOperationOpType(_ptr)); }
        }

        /// <summary>
        /// Get the name of the device this operation runs on
        /// </summary>
        public String Device
        {
            get { return Marshal.PtrToStringAnsi(TfInvoke.tfeOperationDevice(_ptr)); }
        }

        /// <summary>
        /// Get the number of outputs for this operation
        /// </summary>
        public int NumOutputs
        {
            get { return TfInvoke.tfeOperationNumOutputs(_ptr); }
        }

        /// <summary>
        /// Get the output type of the specific output index
        /// </summary>
        /// <param name="index">the output index</param>
        /// <returns>The output type of the specific output index</returns>
        public DataType GetOutputType(int index)
        {
            return TfInvoke.tfeOperationOutputType(_ptr, index);
        }

        /// <summary>
        /// Get the number of inputs for this operation
        /// </summary>
        public int NumInputs
        {
            get { return TfInvoke.tfeOperationNumInputs(_ptr); }
        }

        /// <summary>
        /// Get the input type of the specific input index
        /// </summary>
        /// <param name="index">The input index</param>
        /// <returns>The input type of the specific input index</returns>
        public DataType GetInputType(int index)
        {
            return TfInvoke.tfeOperationInputType(_ptr, index);
        }
    }

    public static partial class TfInvoke
    {

        [DllImport(ExternLibrary, CallingConvention = TfInvoke.TFCallingConvention)]
        internal static extern IntPtr tfeOperationName(IntPtr oper);


        [DllImport(ExternLibrary, CallingConvention = TfInvoke.TFCallingConvention)]
        internal static extern IntPtr tfeOperationOpType(IntPtr oper);

        [DllImport(ExternLibrary, CallingConvention = TfInvoke.TFCallingConvention)]
        internal static extern IntPtr tfeOperationDevice(IntPtr oper);

        [DllImport(ExternLibrary, CallingConvention = TfInvoke.TFCallingConvention)]
        internal static extern int tfeOperationNumOutputs(IntPtr oper);

        [DllImport(ExternLibrary, CallingConvention = TfInvoke.TFCallingConvention)]
        internal static extern DataType tfeOperationOutputType(IntPtr oper, int idx);

        [DllImport(ExternLibrary, CallingConvention = TfInvoke.TFCallingConvention)]
        internal static extern int tfeOperationNumInputs(IntPtr oper);
        [DllImport(ExternLibrary, CallingConvention = TfInvoke.TFCallingConvention)]
        internal static extern DataType tfeOperationInputType(IntPtr oper, int idx);
    }
}
