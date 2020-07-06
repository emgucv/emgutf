//----------------------------------------------------------------------------
//  Copyright (C) 2004-2020 by EMGU Corporation. All rights reserved.       
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

        /// <summary>
        /// Get the metadata of the attribute
        /// </summary>
        /// <param name="attrName">The name of the attribute</param>
        /// <param name="status">The status</param>
        /// <returns>The attribute meta data</returns>
        public AttrMetadata GetAttrMetadata(String attrName, Status status = null)
        {
            using (StatusChecker checker = new StatusChecker(status))
            {
                AttrMetadata meta = new AttrMetadata();
                TfInvoke.tfeOperationGetAttrMetadata(_ptr, attrName, ref meta, checker.Status);
                return meta;
            }
        }

        /// <summary>
        /// Get the bool value of the attribute
        /// </summary>
        /// <param name="attrName">The name of the attribute</param>
        /// <param name="status">The status</param>
        /// <returns>The bool value of the attribute</returns>
        public bool GetAttrBool(String attrName, Status status = null)
        {
            using (StatusChecker checker = new StatusChecker(status))
            {
                return TfInvoke.tfeOperationGetAttrBool(_ptr, attrName, checker.Status);
            }
        }

        /// <summary>
        /// Get the shape value of the attribute
        /// </summary>
        /// <param name="attrName">The name of the attribute</param>
        /// <param name="status">The status</param>
        /// <returns>The shape</returns>
        public Int64[] GetAttrShape(String attrName, Status status = null)
        {
            using (StatusChecker checker = new StatusChecker(status))
            {
                AttrMetadata meta = GetAttrMetadata(attrName, status);
                if (meta.Type != AttrType.Shape)
                    throw new ArgumentException(String.Format("Attribute {0} ({1}) is not a shape", attrName, meta.Type));

                if (meta.TotalSize == 0)
                    return null;

                Int64[] shape = new Int64[meta.IsList? meta.ListSize : meta.TotalSize];
                GCHandle handle = GCHandle.Alloc(shape, GCHandleType.Pinned);
                try
                {
                    TfInvoke.tfeOperationGetAttrShape(_ptr, attrName, handle.AddrOfPinnedObject(), shape.Length,
                        checker.Status);
                }
                finally
                {
                    handle.Free();
                }

                return shape;
            }
        }

        /// <summary>
        /// Get the attribute type
        /// </summary>
        /// <param name="attrName">The name of the attribute</param>
        /// <param name="status">The status</param>
        /// <returns>The type of the attribute</returns>
        public DataType GetAttrType(String attrName, Status status = null)
        {
            using (StatusChecker checker = new StatusChecker(status))
            {
                AttrMetadata meta = GetAttrMetadata(attrName, status);
                if (meta.Type != AttrType.Type)
                    throw new ArgumentException(String.Format("Attribute {0} ({1}) is not a type", attrName, meta.Type));
                
                return TfInvoke.tfeOperationGetAttrType(_ptr, attrName, checker.Status);
            }
        }

        /// <summary>
        /// Get the tensor shape proto value of the attribute
        /// </summary>
        /// <param name="attrName">The name of the attribute</param>
        /// <param name="status">The status</param>
        /// <returns>The buffer that contains the TensorShapeProto </returns>
        public Buffer GetAttrTensorShapeProto(String attrName, Status status = null)
        {
            using (StatusChecker checker = new StatusChecker(status))
            {
                AttrMetadata meta = GetAttrMetadata(attrName, status);
                if (meta.Type != AttrType.Shape)
                    throw new ArgumentException(String.Format("Attribute {0} ({1}) is not a shape", attrName, meta.Type));

                Buffer buffer = new Buffer();
                TfInvoke.tfeOperationGetAttrTensorShapeProto(_ptr, attrName, buffer, checker.Status);
                return buffer;
            }
        }

        /// <summary>
        /// Get the protobuf value of the attribute
        /// </summary>
        /// <param name="attrName">The name of the attribute</param>
        /// <param name="status">The attribute</param>
        /// <returns>The buffer that contains the protobuf value</returns>
        public Buffer GetAttrValueProto(String attrName, Status status = null)
        {
            using (StatusChecker checker = new StatusChecker(status))
            {
                Buffer buffer = new Buffer();
                TfInvoke.tfeOperationGetAttrValueProto(_ptr, attrName, buffer, checker.Status);
                return buffer;
            }
        }

        /// <summary>
        /// Get the value of the attribute that is a Tensor
        /// </summary>
        /// <param name="attrName">The name of the attribute</param>
        /// <param name="status">The status</param>
        /// <returns>The Tensor value of the attribute</returns>
        public Tensor GetAttrTensor(String attrName, Status status = null)
        {
            using (StatusChecker checker = new StatusChecker(status))
            {
                AttrMetadata meta = GetAttrMetadata(attrName, status);
                if (meta.Type != AttrType.Tensor)
                    throw new ArgumentException(String.Format("Attribute {0} ({1}) is not a Tensor", attrName, meta.Type));

                IntPtr tensorPtr = TfInvoke.tfeOperationGetAttrTensor(_ptr, attrName, checker.Status);
                return new Tensor(tensorPtr);
            }
        }

        /// <summary>
        /// Get the value of the attribute that is a String
        /// </summary>
        /// <param name="attrName">The name of the attribute</param>
        /// <param name="status">The status</param>
        /// <returns>The string value of the attribute</returns>
        public String GetAttrString(String attrName, Status status = null)
        {
            using (StatusChecker checker = new StatusChecker(status))
            {
                AttrMetadata meta = GetAttrMetadata(attrName, status);
                if (meta.Type != AttrType.String)
                    throw new ArgumentException(String.Format("Attribute {0} ({1}) is not a String", attrName, meta.Type));

                IntPtr s = Marshal.AllocHGlobal((int)meta.TotalSize);
                try
                {
                    TfInvoke.tfeOperationGetAttrString(_ptr, attrName, s, (int)meta.TotalSize, checker.Status);
                    return Marshal.PtrToStringAnsi(s);
                }
                finally
                {
                    Marshal.FreeHGlobal(s);
                }
                
            }
        }

        /// <summary>
        /// Get the value of the attribute that is a list of Int64
        /// </summary>
        /// <param name="attrName">The name of the attribute</param>
        /// <param name="status">The status</param>
        /// <returns>A list ofInt64</returns>
        public Int64[] GetAttrIntList(String attrName, Status status = null)
        {
            using (StatusChecker checker = new StatusChecker(status))
            {
                AttrMetadata meta = GetAttrMetadata(attrName, status);
                if (!((meta.Type == AttrType.Int) && meta.IsList))
                    throw new ArgumentException(String.Format("Attribute {0} ({1}) is not a List of Int", attrName, meta.Type));

                Int64[] list = new Int64[meta.ListSize];
                GCHandle handle = GCHandle.Alloc(list, GCHandleType.Pinned);
                try
                {
                    TfInvoke.tfeOperationGetAttrIntList(_ptr, attrName, handle.AddrOfPinnedObject(), list.Length, checker.Status);
                }
                finally
                {
                    handle.Free();
                }
                return list;

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

        [DllImport(ExternLibrary, CallingConvention = TfInvoke.TFCallingConvention)]
        internal static extern void tfeOperationGetAttrMetadata(
            IntPtr oper, 
            [MarshalAs(TfInvoke.StringMarshalType)]
            String attrName, 
            ref AttrMetadata meta, 
            IntPtr status);

        [DllImport(ExternLibrary, CallingConvention = TfInvoke.TFCallingConvention)]
        internal static extern DataType tfeOperationGetAttrType(
            IntPtr oper,
            [MarshalAs(TfInvoke.StringMarshalType)]
            String  attrName, 
            IntPtr status);

        [DllImport(ExternLibrary, CallingConvention = TfInvoke.TFCallingConvention)]
        [return:MarshalAs(TfInvoke.BoolMarshalType)]
        internal static extern bool tfeOperationGetAttrBool(
            IntPtr oper,
            [MarshalAs(TfInvoke.StringMarshalType)]
            String  attrName,
            IntPtr status);

        [DllImport(ExternLibrary, CallingConvention = TfInvoke.TFCallingConvention)]
        internal static extern void tfeOperationGetAttrShape(
            IntPtr oper, 
            [MarshalAs(TfInvoke.StringMarshalType)]
            String attrName, 
            IntPtr value,
            int numDims,
            IntPtr status);

        [DllImport(ExternLibrary, CallingConvention = TfInvoke.TFCallingConvention)]
        internal static extern void tfeOperationGetAttrTensorShapeProto(
            IntPtr oper,
            [MarshalAs(TfInvoke.StringMarshalType)]
            String attrName,
            IntPtr value,
            IntPtr status);

        [DllImport(ExternLibrary, CallingConvention = TfInvoke.TFCallingConvention)]
        internal static extern void tfeOperationGetAttrValueProto(
            IntPtr oper,
            [MarshalAs(TfInvoke.StringMarshalType)]
            String attrName,
            IntPtr outputAttrValue, 
            IntPtr status);

        [DllImport(ExternLibrary, CallingConvention = TfInvoke.TFCallingConvention)]
        internal static extern IntPtr tfeOperationGetAttrTensor(
            IntPtr oper,
            [MarshalAs(TfInvoke.StringMarshalType)]
            String attrName,
            IntPtr status);

        [DllImport(ExternLibrary, CallingConvention = TfInvoke.TFCallingConvention)]
        internal static extern void tfeOperationGetAttrString(
            IntPtr oper,
            [MarshalAs(TfInvoke.StringMarshalType)]
            String attrName, 
            IntPtr value, 
            int maxLength, 
            IntPtr status);

        [DllImport(ExternLibrary, CallingConvention = TfInvoke.TFCallingConvention)]
        internal static extern void tfeOperationGetAttrIntList(
            IntPtr oper,
            [MarshalAs(TfInvoke.StringMarshalType)]
            String attrName,
            IntPtr value,
            int maxLength,
            IntPtr status);
    }
}
