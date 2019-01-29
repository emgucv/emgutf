//----------------------------------------------------------------------------
//  Copyright (C) 2004-2019 by EMGU Corporation. All rights reserved.       
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
        /// Return true if the operation is empty
        /// </summary>
        public bool Empty
        {
            get
            {
                return _ptr == IntPtr.Zero;
            }
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
            get
            {
                if (Empty)
                    return null;
                return Marshal.PtrToStringAnsi(TfInvoke.tfeOperationName(_ptr));
            }
        }

        /// <summary>
        /// Get the operation type
        /// </summary>
        public String OpType
        {
            get
            {
                if (Empty)
                    return null;
                return Marshal.PtrToStringAnsi(TfInvoke.tfeOperationOpType(_ptr));
            }
        }

        /// <summary>
        /// Get the name of the device this operation runs on
        /// </summary>
        public String Device
        {
            get
            {
                if (Empty)
                    return null;
                return Marshal.PtrToStringAnsi(TfInvoke.tfeOperationDevice(_ptr));
            }
        }

        /// <summary>
        /// Get the number of outputs for this operation
        /// </summary>
        public int NumOutputs
        {
            get
            {
                if (Empty)
                    return 0;
                return TfInvoke.tfeOperationNumOutputs(_ptr);
            }
        }

        /// <summary>
        /// Get the number of inputs for this operation
        /// </summary>
        public int NumInputs
        {
            get
            {
                if (Empty)
                    return 0;
                return TfInvoke.tfeOperationNumInputs(_ptr);
            }
        }

        /// <summary>
        /// Get the inputs
        /// </summary>
        public Input[] Inputs
        {
            get
            {
                if (Empty)
                    return null;

                Input[] result = new Input[NumInputs];
                for (int i = 0; i < result.Length; i++)
                    result[i] = new Input(this, i);
                return result;   
            }
        }

        /// <summary>
        /// Get the outputs
        /// </summary>
        public Output[] Outputs
        {
            get
            {
                if (Empty)
                    return null;

                Output[] result = new Output[NumOutputs];
                for (int i = 0; i < result.Length; i++)
                    result[i] = new Output(this, i);
                return result;
            }
        }

        /// <summary>
        /// Get the number of control inputs
        /// </summary>
        public int NumControlInputs
        {
            get
            {
                if (Empty)
                    return 0;
                return TfInvoke.tfeOperationNumControlInputs(_ptr);
            }
        }

        /// <summary>
        /// Get list of all control inputs to an operation.
        /// </summary>
        public Operation[] ControlInputs
        {
            get
            {
                int numControlInputs = NumControlInputs;
                if (numControlInputs == 0)
                    return new Operation[0];

                IntPtr[] ops = new IntPtr[numControlInputs];
                GCHandle opsHandle = GCHandle.Alloc(ops, GCHandleType.Pinned);
                TfInvoke.tfeOperationGetControlInputs(_ptr, opsHandle.AddrOfPinnedObject(), NumControlInputs);
                opsHandle.Free();

                Operation[] operations = new Operation[numControlInputs];
                for (int i = 0; i < numControlInputs; i++)
                    operations[i] = new Operation(ops[i]);
                return operations;
            }
        }

        /// <summary>
        /// Get the number of control outputs.
        /// </summary>
        public int NumControlOutputs
        {
            get
            {
                if (Empty)
                    return 0;
                return TfInvoke.tfeOperationNumControlOutputs(_ptr);
            }
        }

        /// <summary>
        /// Get the list of operations that have the current operation as a control input.
        /// </summary>
        public Operation[] ControlOutputs
        {
            get
            {
                int numControlOutputs = NumControlOutputs;
                if (numControlOutputs == 0)
                    return new Operation[0];

                IntPtr[] ops = new IntPtr[numControlOutputs];
                GCHandle opsHandle = GCHandle.Alloc(ops, GCHandleType.Pinned);
                TfInvoke.tfeOperationGetControlOutputs(_ptr, opsHandle.AddrOfPinnedObject(), NumControlOutputs);
                opsHandle.Free();

                Operation[] operations = new Operation[numControlOutputs];
                for (int i = 0; i < numControlOutputs; i++)
                    operations[i] = new Operation(ops[i]);
                return operations;
            }
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
        internal static extern int tfeOperationNumInputs(IntPtr oper);

        [DllImport(ExternLibrary, CallingConvention = TfInvoke.TFCallingConvention)]
        internal static extern int tfeOperationNumControlInputs(IntPtr oper);

        [DllImport(ExternLibrary, CallingConvention = TfInvoke.TFCallingConvention)]
        internal static extern int tfeOperationGetControlInputs(IntPtr oper, IntPtr controlInputs, int maxControlInputs);

        [DllImport(ExternLibrary, CallingConvention = TfInvoke.TFCallingConvention)]
        internal static extern int tfeOperationNumControlOutputs(IntPtr oper);

        [DllImport(ExternLibrary, CallingConvention = TfInvoke.TFCallingConvention)]
        internal static extern int tfeOperationGetControlOutputs(IntPtr oper, IntPtr controlOutputs, int maxControlOutputs);
    }
}
