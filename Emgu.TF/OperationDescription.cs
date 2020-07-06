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
    /// Operation being built. The underlying graph must outlive this
    /// </summary>
    public class OperationDescription : UnmanagedObject
    {
        internal OperationDescription(IntPtr ptr)
        {
            _ptr = ptr;
        }

        /// <summary>
        /// If this function succeeds:
        /// status is set to an OK value,
        /// an Operation is added to the graph,
        /// a non-null value pointing to the added operation is returned
        /// -- this value is valid until the underlying graph is deleted.
        /// Otherwise:
        /// status is set to a non-OK value,
        /// the graph is not modified,
        /// a null value is returned.
        /// In either case, it deletes the OperationDescription.
        /// </summary>
        /// <param name="status">The status</param>
        /// <returns>If success, the Operation that is added to the graph, otherwise null.</returns>
        public Operation FinishOperation(Status status = null)
        {
            if (_ptr == IntPtr.Zero)
                return null;

            using (StatusChecker checker = new StatusChecker(status))
            {
                Operation op = new Operation(TfInvoke.tfeFinishOperation(_ptr, checker.Status));
                _ptr = IntPtr.Zero;
                return op;
            }
        }

        /// <summary>
        /// Add the input to this operation description
        /// </summary>
        /// <param name="input">The input to the operation</param>
        public void AddInput(Output input)
        {
            TfInvoke.tfeAddInput(_ptr, input.Operation, input.Index);
        }

        /// <summary>
        /// Add the inputs to this operation description
        /// </summary>
        /// <param name="inputs">The inputs to the operation</param>
        public void AddInput(Output[] inputs)
        {
            IntPtr[] opPtrs = new IntPtr[inputs.Length];
            int[] indices = new int[inputs.Length];
            for (int i = 0; i < inputs.Length; i++)
            {
                opPtrs[i] = inputs[i].Operation.Ptr;
                indices[i] = inputs[i].Index;
            }
            GCHandle opHandle = GCHandle.Alloc(opPtrs, GCHandleType.Pinned);
            GCHandle idxHandle = GCHandle.Alloc(indices, GCHandleType.Pinned);
            TfInvoke.tfeAddInputList(_ptr, opHandle.AddrOfPinnedObject(), idxHandle.AddrOfPinnedObject(), inputs.Length);
            idxHandle.Free();
            opHandle.Free();
        }

        /// <summary>
        /// Set a long value as an attribute
        /// </summary>
        /// <param name="attrName">The attribute name</param>
        /// <param name="value">The value</param>
        public void SetAttr(String attrName, long value)
        {
            TfInvoke.tfeSetAttrInt(_ptr, attrName, value);
        }

        /// <summary>
        /// Set an array of long value as an attribute
        /// </summary>
        /// <param name="attrName">The attribute name</param>
        /// <param name="value">The value</param>
        public void SetAttr(String attrName, long[] value)
        {
            GCHandle handle = GCHandle.Alloc(value, GCHandleType.Pinned);
            TfInvoke.tfeSetAttrIntList(_ptr, attrName, handle.AddrOfPinnedObject(), value.Length);
            handle.Free();
        }

        /// <summary>
        /// Set a boolean value as an attribute
        /// </summary>
        /// <param name="attrName">The attribute name</param>
        /// <param name="value">The value</param>
        public void SetAttr(String attrName, bool value)
        {
            TfInvoke.tfeSetAttrBool(_ptr, attrName, value);
        }

        /// <summary>
        /// Set an array of boolean value as an attribute
        /// </summary>
        /// <param name="attrName">The attribute name</param>
        /// <param name="value">The value</param>
        public void SetAttr(String attrName, bool[] value)
        {
            GCHandle handle = GCHandle.Alloc(value, GCHandleType.Pinned);
            TfInvoke.tfeSetAttrBoolList(_ptr, attrName, handle.AddrOfPinnedObject(), value.Length);
            handle.Free();
        }

        /// <summary>
        /// Set a floating point value as an attribute
        /// </summary>
        /// <param name="attrName">The attribute name</param>
        /// <param name="value">The value</param>
        public void SetAttr(String attrName, float value)
        {
            TfInvoke.tfeSetAttrFloat(_ptr, attrName, value);
        }

        /// <summary>
        /// Set an array of floating point value as an attribute
        /// </summary>
        /// <param name="attrName">The attribute name</param>
        /// <param name="value">The value</param>
        public void SetAttr(String attrName, float[] value)
        {
            GCHandle handle = GCHandle.Alloc(value, GCHandleType.Pinned);
            TfInvoke.tfeSetAttrFloatList(_ptr, attrName, handle.AddrOfPinnedObject(), value.Length);
            handle.Free();
        }

        /// <summary>
        /// Set a string value as an attribute
        /// </summary>
        /// <param name="attrName">The attribute name</param>
        /// <param name="value">The value</param>
        public void SetAttr(String attrName, String value)
        {
            TfInvoke.tfeSetAttrString(_ptr, attrName, value, value.Length);
        }

        /// <summary>
        /// Set a string value as an attribute
        /// </summary>
        /// <param name="attrName">The attribute name</param>
        /// <param name="value">The value</param>
        public void SetAttr(String attrName, String[] value)
        {
            if (value == null)
            {
                TfInvoke.tfeSetAttrStringList(_ptr, attrName, IntPtr.Zero, IntPtr.Zero, 0);
            }
            else
            {
                IntPtr[] valuePtrs = new IntPtr[value.Length];
                IntPtr[] lengths = new IntPtr[value.Length];
                for (int i = 0; i < value.Length; i++)
                {
                    valuePtrs[i] = Marshal.StringToHGlobalAnsi(value[i]);
                    lengths[i] = new IntPtr(value[i].Length);
                }
                GCHandle valuesHandle = GCHandle.Alloc(valuePtrs, GCHandleType.Pinned);
                GCHandle lengthsHandle = GCHandle.Alloc(lengths, GCHandleType.Pinned);
                TfInvoke.tfeSetAttrStringList(_ptr, attrName, valuesHandle.AddrOfPinnedObject(), lengthsHandle.AddrOfPinnedObject(), value.Length);
                lengthsHandle.Free();
                valuesHandle.Free();

                for (int i = 0; i < valuePtrs.Length; i++)
                    Marshal.FreeHGlobal(valuePtrs[i]);
                
            }
        }

        /// <summary>
        /// Set a DataType value as an attribute
        /// </summary>
        /// <param name="attrName">The attribute name</param>
        /// <param name="type">The type</param>
        public void SetAttr(String attrName, DataType type)
        {
            TfInvoke.tfeSetAttrType(_ptr, attrName, type);
        }

        /// <summary>
        /// Set an array of DataType value as an attribute
        /// </summary>
        /// <param name="attrName">The attribute name</param>
        /// <param name="types">The types</param>
        public void SetAttr(String attrName, DataType[] types)
        {
            GCHandle handle = GCHandle.Alloc(types, GCHandleType.Pinned);
            TfInvoke.tfeSetAttrTypeList(_ptr, attrName, handle.AddrOfPinnedObject(), types.Length);
            handle.Free();
        }

        /// <summary>
        /// Set a shape as an attribute
        /// </summary>
        /// <param name="attrName">The attribute name</param>
        /// <param name="shape">The shape</param>
        public void SetAttrShape(String attrName, long[] shape)
        {
            if (shape == null)
            {
                TfInvoke.tfeSetAttrShape(_ptr, attrName, IntPtr.Zero, -1);
            }
            else
            {
                GCHandle handle = GCHandle.Alloc(shape, GCHandleType.Pinned);
                TfInvoke.tfeSetAttrShape(_ptr, attrName, handle.AddrOfPinnedObject(), shape.Length);
                handle.Free();
            }
        }

        /// <summary>
        /// Set a shape list as an attribute
        /// </summary>
        /// <param name="attrName">The attribute name</param>
        /// <param name="shapes">The shapes</param>
        public void SetAttrShapeList(String attrName, long[][] shapes)
        {
            if (shapes == null)
            {
                TfInvoke.tfeSetAttrShapeList(_ptr, attrName, IntPtr.Zero, IntPtr.Zero, -1);
            }
            else
            {
                GCHandle[] handles = new GCHandle[shapes.Length];
                IntPtr[] shapePtrs = new IntPtr[shapes.Length];
                int[] dims = new int[shapes.Length];
                for (int i = 0; i < shapes.Length; i++)
                {
                    handles[i] = GCHandle.Alloc(shapes[i], GCHandleType.Pinned);
                    shapePtrs[i] = handles[i].AddrOfPinnedObject();
                    dims[i] = shapes[i].Length;
                }
                GCHandle shapeHandles = GCHandle.Alloc(shapePtrs, GCHandleType.Pinned);
                GCHandle dimHandles = GCHandle.Alloc(dims, GCHandleType.Pinned);

                TfInvoke.tfeSetAttrShapeList(_ptr, attrName, shapeHandles.AddrOfPinnedObject(), dimHandles.AddrOfPinnedObject(), shapes.Length);

                dimHandles.Free();
                shapeHandles.Free();
                for (int i = 0; i < shapes.Length; i++)
                {
                    handles[i].Free();
                }
            }
        }

        /// <summary>
        /// Set a Tensor as an attribute
        /// </summary>
        /// <param name="attrName">The name of the attribute</param>
        /// <param name="tensor">The Tensor</param>
        /// <param name="status">The status</param>
        public void SetAttr(String attrName, Tensor tensor, Status status = null)
        {
            using (StatusChecker checker = new StatusChecker(status))
                TfInvoke.tfeSetAttrTensor(_ptr, attrName, tensor, checker.Status);
        }

        /// <summary>
        /// Specify the device
        /// </summary>
        /// <param name="device">The device name</param>
        public void SetDevice(String device)
        {
            TfInvoke.tfeSetDevice(_ptr, device);
        }

        /// <summary>
        /// Call once per control input to this Operation description
        /// </summary>
        /// <param name="input">The control input</param>
        public void AddControlInput(Operation input)
        {
            TfInvoke.tfeAddControlInput(_ptr, input);
        }

        /// <summary>
        /// Request this operation be co-located on the device where <paramref name="op"/>
        /// is placed.
        /// </summary>
        /// <param name="op">The other operation</param>
        public void ColocateWith(Operation op)
        {
            TfInvoke.tfeColocateWith(_ptr, op);
        }

        /// <summary>
        /// Release all the unmanaged data associated with this OperationDescription
        /// </summary>
        protected override void DisposeObject()
        {
            if (IntPtr.Zero != _ptr)
            {
                using (StatusChecker checker = new StatusChecker(null))
                {
                    FinishOperation(checker.Status);
                }
            }
        }
    }

    public static partial class TfInvoke
    {

        [DllImport(ExternLibrary, CallingConvention = TfInvoke.TFCallingConvention)]
        internal static extern IntPtr tfeFinishOperation(IntPtr desc, IntPtr status);

        [DllImport(ExternLibrary, CallingConvention = TfInvoke.TFCallingConvention)]
        internal static extern void tfeAddInput(IntPtr desc, IntPtr oper, int index);

        [DllImport(ExternLibrary, CallingConvention = TfInvoke.TFCallingConvention)]
        internal static extern void tfeAddInputList(IntPtr desc, IntPtr inputOps, IntPtr indices, int numInputs);

        [DllImport(ExternLibrary, CallingConvention = TfInvoke.TFCallingConvention)]
        internal static extern void tfeSetAttrInt(
            IntPtr desc,
            [MarshalAs(TfInvoke.StringMarshalType)]
            String attrName,
            Int64 value);

        [DllImport(ExternLibrary, CallingConvention = TfInvoke.TFCallingConvention)]
        internal static extern void tfeSetAttrIntList(
            IntPtr desc,
            [MarshalAs(TfInvoke.StringMarshalType)]
            String attrName,
            IntPtr values,
            int numValues);

        [DllImport(ExternLibrary, CallingConvention = TfInvoke.TFCallingConvention)]
        internal static extern void tfeSetAttrBool(
            IntPtr desc,
            [MarshalAs(TfInvoke.StringMarshalType)]
            String attrName,
            [MarshalAs(TfInvoke.BoolMarshalType)]
            bool value);

        [DllImport(ExternLibrary, CallingConvention = TfInvoke.TFCallingConvention)]
        internal static extern void tfeSetAttrBoolList(
            IntPtr desc,
            [MarshalAs(TfInvoke.StringMarshalType)] String attrName,
            IntPtr values,
            int numValues);

        [DllImport(ExternLibrary, CallingConvention = TfInvoke.TFCallingConvention)]
        internal static extern void tfeSetAttrFloat(
            IntPtr desc,
            [MarshalAs(TfInvoke.StringMarshalType)]
            String attrName,
            float value);

        [DllImport(ExternLibrary, CallingConvention = TfInvoke.TFCallingConvention)]
        internal static extern void tfeSetAttrFloatList(
            IntPtr desc,
            [MarshalAs(TfInvoke.StringMarshalType)]
            String attrName,
            IntPtr values,
            int numValues);

        [DllImport(ExternLibrary, CallingConvention = TfInvoke.TFCallingConvention)]
        internal static extern void tfeSetAttrString(
            IntPtr desc,
            [MarshalAs(TfInvoke.StringMarshalType)]
            String attrName,
            [MarshalAs(TfInvoke.StringMarshalType)]
            String value,
            int length);

        [DllImport(ExternLibrary, CallingConvention = TfInvoke.TFCallingConvention)]
        internal static extern void tfeSetAttrStringList(
            IntPtr desc,
            [MarshalAs(TfInvoke.StringMarshalType)]
            String attrName, 
            IntPtr values, 
            IntPtr lengths, 
            int numValues);

        [DllImport(ExternLibrary, CallingConvention = TfInvoke.TFCallingConvention)]
        internal static extern void tfeSetAttrType(
            IntPtr desc,
            [MarshalAs(TfInvoke.StringMarshalType)]
            String attrName,
            DataType value);

        [DllImport(ExternLibrary, CallingConvention = TfInvoke.TFCallingConvention)]
        internal static extern void tfeSetAttrTypeList(
            IntPtr desc,
            [MarshalAs(TfInvoke.StringMarshalType)]
            String attrName,
            IntPtr values,
            int numValues);


        [DllImport(ExternLibrary, CallingConvention = TfInvoke.TFCallingConvention)]
        internal static extern void tfeSetAttrShape(
            IntPtr desc,
            [MarshalAs(TfInvoke.StringMarshalType)]
            String attrName,
            IntPtr dims,
            int numDims);

        [DllImport(ExternLibrary, CallingConvention = TfInvoke.TFCallingConvention)]
        internal static extern void tfeSetAttrShapeList(
            IntPtr desc,
            [MarshalAs(TfInvoke.StringMarshalType)]
            String attrName,
            IntPtr dims,
            IntPtr numDims,
            int numShapes);

        [DllImport(ExternLibrary, CallingConvention = TfInvoke.TFCallingConvention)]
        internal static extern void tfeSetAttrTensor(
            IntPtr desc,
            [MarshalAs(TfInvoke.StringMarshalType)]
            String attrName,
            IntPtr value,
            IntPtr status);

        [DllImport(ExternLibrary, CallingConvention = TfInvoke.TFCallingConvention)]
        internal static extern void tfeSetDevice(
            IntPtr desc,
            [MarshalAs(TfInvoke.StringMarshalType)]
            String device);

        [DllImport(ExternLibrary, CallingConvention = TfInvoke.TFCallingConvention)]
        internal static extern void tfeAddControlInput(IntPtr desc, IntPtr input);

        [DllImport(ExternLibrary, CallingConvention = TfInvoke.TFCallingConvention)]
        internal static extern void tfeColocateWith(IntPtr desc, IntPtr op);
    }
}
