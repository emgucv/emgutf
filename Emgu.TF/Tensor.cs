//----------------------------------------------------------------------------
//  Copyright (C) 2004-2020 by EMGU Corporation. All rights reserved.       
//----------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using Emgu.TF.Util;
using System.Runtime.InteropServices;

namespace Emgu.TF
{
    /// <summary>
    /// The tensor flow datatype.
    /// </summary>
    public enum DataType
    {
        /// <summary>
        /// Float
        /// </summary>
        Float = 1,
        /// <summary>
        /// Double
        /// </summary>
        Double = 2,
        /// <summary>
        /// Int32 tensors are always in 'host' memory.
        /// </summary>
        Int32 = 3,  
        /// <summary>
        /// Uint8
        /// </summary>
        Uint8 = 4,
        /// <summary>
        /// Int16
        /// </summary>
        Int16 = 5,
        /// <summary>
        /// Int8
        /// </summary>
        Int8 = 6,
        /// <summary>
        /// String
        /// </summary>
        String = 7,
        /// <summary>
        /// Single-precision complex
        /// </summary>
        Complex64 = 8,
        /// <summary>
        /// Old identifier kept for API backwards compatibility
        /// </summary>
        Complex = 8,    
        /// <summary>
        /// Int64
        /// </summary>
        Int64 = 9,
        /// <summary>
        /// Boolean
        /// </summary>
        Bool = 10,
        /// <summary>
        /// Quantized int8
        /// </summary>
        Qint8 = 11,
        /// <summary>
        /// Quantized uint8
        /// </summary>
        Quint8 = 12,
        /// <summary>
        /// Quantized int32
        /// </summary>
        Qint32 = 13,
        /// <summary>
        /// Float32 truncated to 16 bits.  Only for cast ops.
        /// </summary>
        Bfloat16 = 14,
        /// <summary>
        /// Quantized int16
        /// </summary>
        Qint16 = 15,
        /// <summary>
        /// Quantized uint16
        /// </summary>
        Quint16 = 16,   
        /// <summary>
        /// Uint16
        /// </summary>
        Uint16 = 17,
        /// <summary>
        /// Double-precision complex
        /// </summary>
        Complex128 = 18,  
        /// <summary>
        /// Half
        /// </summary>
        Half = 19,
        /// <summary>
        /// Resource
        /// </summary>
        Resource = 20,
        /// <summary>
        /// Variant
        /// </summary>
        Variant = 21,
        /// <summary>
        /// Uint32
        /// </summary>
        Uint32 = 22,
        /// <summary>
        /// Uint64
        /// </summary>
        Uint64 = 23,
    }

    public static partial class TfInvoke
    {
        /// <summary>
        /// Get the equivalent native type from Tensorflow DataType
        /// </summary>
        /// <param name="dataType">The tensorflow DataType</param>
        /// <returns>The equivalent native type</returns>
        public static Type GetNativeType(DataType dataType)
        {
            switch (dataType)
            {
                case DataType.Float:
                    return typeof(float);
                case DataType.Double:
                    return typeof(double);
                case DataType.Int16:
                    return typeof(Int16);
                case DataType.Int32:
                    return typeof(int);
                case DataType.Int64:
                    return typeof(Int64);
                case DataType.Uint8:
                    return typeof(byte);
                case DataType.Uint16:
                    return typeof(UInt16);
                case DataType.Uint32:
                    return typeof(UInt32);
                case DataType.Uint64:
                    return typeof(UInt64);
                case DataType.String:
                    return typeof(Byte);
                default:
                    return null;
            }
        }
    }

    /// <summary>
    /// Tensor holds a multi-dimensional array of elements of a single data type.
    /// For all types other than STRING, the data buffer stores elements
    /// in row major order.  E.g. if data is treated as a vector of TF_DataType:
    ///
    ///   element 0:   index (0, ..., 0)
    ///   element 1:   index (0, ..., 1)
    ///   ...
    ///
    /// The format for TF_STRING tensors is:
    ///   start_offset: array[uint64]
    ///   data:         byte[...]
    ///
    ///   The string length (as a varint), followed by the contents of the string
    ///   is encoded at data[start_offset[i]]]. StringEncode and StringDecode
    ///   facilitate this encoding.
    /// </summary>
    public class Tensor : UnmanagedObject
    {

        internal Tensor(IntPtr ptr)
        {
            _ptr = ptr;
        }

        /// <summary>
        /// Create a single element tensor
        /// </summary>
        /// <param name="dataType">The Type of the Tensor</param>
        /// <param name="sizeInBytes">The size in bytes</param>
        private Tensor(DataType dataType, int sizeInBytes)
        {
            _ptr = TfInvoke.tfeAllocateTensor(dataType, IntPtr.Zero, 0, sizeInBytes);
        }

        /// <summary>
        /// Allocate a new tensor. The caller must set the Tensor values by writing them to the DataPointer
        /// with length ByteSize.
        /// </summary>
        /// <param name="dataType">The type of data</param>
        /// <param name="dims">The size for each of the dimension of the tensor</param>
        public Tensor(DataType dataType, int[] dims)
        {
            int size = dims.Length > 0 ? 1 : 0;
            for (int i = 0; i < dims.Length; i++)
            {
                size *= dims[i];
            }

            size *= TfInvoke.DataTypeSize(dataType);

            GCHandle dimHandle = GCHandle.Alloc(dims, GCHandleType.Pinned);
            _ptr = TfInvoke.tfeAllocateTensor(dataType, dimHandle.AddrOfPinnedObject(), dims.Length, size);
            dimHandle.Free();
        }

        /// <summary>
        /// Convert a byte array to a Tensor
        /// </summary>
        /// <param name="value">The byte array</param>
        /// <param name="status">Optional status</param>
        /// <returns>The tensor</returns>
        public static Tensor FromString(byte[] value, Status status = null)
        {
            using (StatusChecker checker = new StatusChecker(status))
            {
                int length = TfInvoke.tfeStringEncodedSize(value.Length);
                Tensor tensor = new Tensor(DataType.String, length + 8);
                IntPtr ptr = tensor.DataPointer;
                Marshal.WriteInt64(ptr, 0);
                GCHandle handle = GCHandle.Alloc(value, GCHandleType.Pinned);
                TfInvoke.tfeStringEncode(handle.AddrOfPinnedObject(), value.Length, new IntPtr(ptr.ToInt64() + 8),
                    length,
                    checker.Status);
                handle.Free();
                return tensor;
            }
        }

        /// <summary>
        /// Decode a string encoded
        /// </summary>
        /// <param name="status">The status</param>
        /// <returns>The decoded string.</returns>
        public byte[] DecodeString(Status status = null)
        {
            using (StatusChecker checker = new StatusChecker(status))
            {
                IntPtr ptr = DataPointer;
                IntPtr stringPtr = new IntPtr();
                IntPtr stringLen = new IntPtr();
                TfInvoke.tfeStringDecode(new IntPtr(ptr.ToInt64() + 8), ByteSize - 8, ref stringPtr, ref stringLen,
                    checker.Status);
                int len = stringLen.ToInt32();
                byte[] bytes = new byte[len];
                Marshal.Copy(stringPtr, bytes, 0, bytes.Length);
                return bytes;
            }
        }

        /// <summary>
        /// Create a Tensor that consist of a single int16 value
        /// </summary>
        /// <param name="value">The value</param>
        public Tensor(Int16 value) :
            this(DataType.Int16, sizeof(Int16))
        {
            Marshal.WriteInt16(DataPointer, value);
        }

        /// <summary>
        /// Create a Tensor that consist of a single int value
        /// </summary>
        /// <param name="value">The value</param>
        public Tensor(int value) :
            this(DataType.Int32, sizeof(int))
        {
            Marshal.WriteInt32(DataPointer, value);
        }

        /// <summary>
        /// Create a Tensor that consist of a single int64 value
        /// </summary>
        /// <param name="value">The value</param>
        public Tensor(Int64 value) :
            this(DataType.Int64, sizeof(int))
        {
            Marshal.WriteInt64(DataPointer, value);
        }

        /// <summary>
        /// Create a Tensor that consist of a single float value
        /// </summary>
        /// <param name="value">The value</param>
        public Tensor(float value) :
            this(DataType.Float, sizeof(float))
        {
            Marshal.Copy(new float[] { value }, 0, DataPointer, 1);
        }

        /// <summary>
        /// Create a Tensor that consist of a single float value
        /// </summary>
        /// <param name="value">The value</param>
        public Tensor(double value) :
            this(DataType.Double, sizeof(float))
        {
            Marshal.Copy(new double[] { value }, 0, DataPointer, 1);
        }

        /// <summary>
        /// Create a Tensor that consist of an array of int value
        /// </summary>
        /// <param name="value">The value</param>
        public Tensor(int[] value) :
            this(DataType.Int32, new[] { value.Length })
        {
            Marshal.Copy(value, 0, DataPointer, value.Length);
        }

        /// <summary>
        /// Create a Tensor that consist of an array of float value
        /// </summary>
        /// <param name="value">The value</param>
        public Tensor(float[] value) :
            this(DataType.Float, new[] { value.Length })
        {
            Marshal.Copy(value, 0, DataPointer, value.Length);
        }

        /// <summary>
        /// Create a Tensor that consist of an array of double value
        /// </summary>
        /// <param name="value">The value</param>
        public Tensor(double[] value) :
            this(DataType.Double, new[] { value.Length })
        {
            Marshal.Copy(value, 0, DataPointer, value.Length);
        }

        /// <summary>
        /// Create a Tensor that consist of an array of UInt16 value
        /// </summary>
        /// <param name="value">The value</param>
        public Tensor(UInt16[] value) :
            this(DataType.Uint16, new[] { value.Length })
        {
            GCHandle handle = GCHandle.Alloc(value, GCHandleType.Pinned);
            Emgu.TF.TfInvoke.tfeMemcpy(this.DataPointer, handle.AddrOfPinnedObject(), value.Length * sizeof(UInt16));
            handle.Free();
        }

        /// <summary>
        /// Create a Tensor that consist of an array of UInt32 value
        /// </summary>
        /// <param name="value">The value</param>
        public Tensor(UInt32[] value) :
            this(DataType.Uint32, new[] { value.Length })
        {
            GCHandle handle = GCHandle.Alloc(value, GCHandleType.Pinned);
            Emgu.TF.TfInvoke.tfeMemcpy(this.DataPointer, handle.AddrOfPinnedObject(), value.Length * sizeof(UInt32));
            handle.Free();
        }

        /// <summary>
        /// Create a Tensor that consist of an array of UInt64 value
        /// </summary>
        /// <param name="value">The value</param>
        public Tensor(UInt64[] value) :
            this(DataType.Uint64, new[] { value.Length })
        {
            GCHandle handle = GCHandle.Alloc(value, GCHandleType.Pinned);
            Emgu.TF.TfInvoke.tfeMemcpy(this.DataPointer, handle.AddrOfPinnedObject(), value.Length * sizeof(UInt64));
            handle.Free();
        }

        /// <summary>
        /// Release the unmanaged memory associated with this tensor
        /// </summary>
        protected override void DisposeObject()
        {
            if (IntPtr.Zero != _ptr)
                TfInvoke.tfeDeleteTensor(ref _ptr);
        }

        /// <summary>
        /// Get the native pointer to the tensor data
        /// </summary>
        public IntPtr DataPointer
        {
            get { return TfInvoke.tfeTensorData(_ptr); }
        }

        /// <summary>
        /// Return the size of the underlying data in bytes.
        /// </summary>
        public int ByteSize
        {
            get { return TfInvoke.tfeTensorByteSize(_ptr); }
        }

        /// <summary>
        /// Return the dimension of the tensor
        /// </summary>
        public int[] Dim
        {
            get
            {
                int dims = TfInvoke.tfeNumDims(_ptr);
                int[] dim = new int[dims];
                GCHandle handle = GCHandle.Alloc(dim, GCHandleType.Pinned);
                TfInvoke.tfeGetDim(_ptr, handle.AddrOfPinnedObject(), dims);
                handle.Free();
                return dim;
            }
        }

        /// <summary>
        /// Get the tensor data as a jagged array
        /// </summary>
        public Array JaggedData
        {
            get { return GetData( true ); }
        }

        /// <summary>
        /// Get the tensor data as a managed array
        /// </summary>
        public Array Data
        {
            get { return GetData( false ); }
        }

        /// <summary>
        /// Get the tensor data as a flatten single dimension array
        /// </summary>
        /// <typeparam name="T">The type of the data array</typeparam>
        /// <returns>The tensor data as a flatten single dimension array</returns>
        public T[] Flat<T>() where T : struct
        {
            Type t = TfInvoke.GetNativeType(this.Type);
            if (typeof(T) != t)
            {
                throw new ArgumentException(String.Format("Tensor type {0} is incompatible with the generic type {1}", this.Type, t.ToString()));
            }
            return GetData(false) as T[];
        }

        /// <summary>
        /// Get a copy of the tensor data as a managed array
        /// </summary>
        /// <param name="jagged">If true, return a jagged array, otherwise, a flatten single dimension array</param>
        /// <returns>A copy of the tensor data as a managed array</returns>
        public Array GetData(bool jagged = true)
        {
            DataType type = this.Type;
            Type t = TfInvoke.GetNativeType(type);
            if (t == null)
                return null;

            Array array;
            int byteSize = ByteSize;

            if (jagged)
            {
                int[] dim = this.Dim;
                array = Array.CreateInstance(t, dim);
            }
            else
            {
                int len = byteSize / Marshal.SizeOf(t);
                array = Array.CreateInstance(t, len);
            }
            /*
            int[] dim = Dim;
            int byteSize = ByteSize;
            Array array;

            if (dim.Length == 0)
            {
                int len = byteSize/ Marshal.SizeOf(t);
                array = Array.CreateInstance(t, len);
            } else if (jagged)
                array = Array.CreateInstance(t, dim);
            else
            {
                int len = 0;
                if (dim.Length > 0)
                    len = dim[0];
                for (int i = 1; i < dim.Length; i++)
                    len *= dim[i];
                array = Array.CreateInstance(t, len);
            }*/
            GCHandle handle = GCHandle.Alloc(array, GCHandleType.Pinned);
            TfInvoke.tfeMemcpy(handle.AddrOfPinnedObject(), DataPointer, byteSize);
            handle.Free();
            return array;
        }

        /// <summary>
        /// Get the type of data from this tensor
        /// </summary>
        public DataType Type
        {
            get { return TfInvoke.tfeTensorType(_ptr); }
        }
    }

    public static partial class TfInvoke
    {
        [DllImport(ExternLibrary, CallingConvention = TfInvoke.TFCallingConvention)]
        internal static extern IntPtr tfeAllocateTensor(DataType tfDataType, IntPtr dims, int numDims, int len);

        [DllImport(ExternLibrary, CallingConvention = TfInvoke.TFCallingConvention)]
        internal static extern void tfeDeleteTensor(ref IntPtr tensor);

        [DllImport(ExternLibrary, CallingConvention = TfInvoke.TFCallingConvention)]
        internal static extern IntPtr tfeTensorData(IntPtr tensor);

        [DllImport(ExternLibrary, CallingConvention = TfInvoke.TFCallingConvention)]
        internal static extern int tfeTensorByteSize(IntPtr tensor);


        [DllImport(ExternLibrary, CallingConvention = TfInvoke.TFCallingConvention)]
        internal static extern int tfeNumDims(IntPtr tensor);
        [DllImport(ExternLibrary, CallingConvention = TfInvoke.TFCallingConvention)]
        internal static extern void tfeGetDim(IntPtr tensor, IntPtr dims, int numDims);

        [DllImport(ExternLibrary, CallingConvention = TfInvoke.TFCallingConvention)]
        internal static extern DataType tfeTensorType(IntPtr tensor);
    }
}
