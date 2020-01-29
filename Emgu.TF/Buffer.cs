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
    /// Buffer holds a pointer to a block of data and its associated length.
    /// Typically, the data consists of a serialized protocol buffer, but other data
    /// may also be held in a buffer.
    /// </summary>
    public class Buffer : UnmanagedObject
    {
        private bool _needDispose;

        /// <summary>
        /// Create a new empty buffer
        /// </summary>
        public Buffer()
        {
            _needDispose = true;
            _ptr = TfInvoke.tfeNewBuffer();
        }

        internal Buffer(IntPtr ptr, bool needDispose)
        {
            _ptr = ptr;
            _needDispose = needDispose;
        }

        /// <summary>
        /// Create a new Buffer from raw protobuf data
        /// </summary>
        /// <param name="rawProtoBuf">The raw protobuf data</param>
        /// <returns>A Tensorflow buffer</returns>
        public static Buffer FromString(byte[] rawProtoBuf)
        {
            GCHandle handle = GCHandle.Alloc(rawProtoBuf, GCHandleType.Pinned);
            Buffer buffer = new Buffer(TfInvoke.tfeNewBufferFromString(handle.AddrOfPinnedObject(), rawProtoBuf.Length), true);
            handle.Free();
            return buffer;
        }

        /// <summary>
        /// Get the pointer to the unmanaged data
        /// </summary>
        public IntPtr DataPtr
        {
            get { return TfInvoke.tfeBufferGetData(_ptr); }
        }

        /// <summary>
        /// The length of the Data in bytes
        /// </summary>
        public int Length
        {
            get { return TfInvoke.tfeBufferGetLength(_ptr); }
        }

        /// <summary>
        /// Get a copy of the data as a memory stream
        /// </summary>
        /// <returns>A copy of the data as a Memory stream</returns>
        public MemoryStream GetMemoryStream()
        {
            return new MemoryStream(Data);
        }

        /// <summary>
        /// Get the raw data as an array of byte.
        /// </summary>
        public byte[] Data
        {
            get
            {
                byte[] bytes = new byte[Length];
                Marshal.Copy(DataPtr, bytes, 0, bytes.Length);
                return bytes;
            }
        }

        /// <summary>
        /// Release all the unmanaged memory associated with this Buffer
        /// </summary>
        protected override void DisposeObject()
        {
            if (IntPtr.Zero != _ptr)
            {
                if (_needDispose)
                    TfInvoke.tfeDeleteBuffer(ref _ptr);
                else
                    _ptr = IntPtr.Zero;
            }
        }
    }

    /// <summary>
    /// Entry points to the native Tensorflow library.
    /// </summary>
    public static partial class TfInvoke
    {
        
        [DllImport(ExternLibrary, CallingConvention = TfInvoke.TFCallingConvention)]
        internal static extern IntPtr tfeNewBuffer();

        [DllImport(ExternLibrary, CallingConvention = TfInvoke.TFCallingConvention)]
        internal static extern void tfeDeleteBuffer(ref IntPtr buffer);

        [DllImport(ExternLibrary, CallingConvention = TfInvoke.TFCallingConvention)]
        internal static extern IntPtr tfeNewBufferFromString(IntPtr proto, int protoLen);

        [DllImport(ExternLibrary, CallingConvention = TfInvoke.TFCallingConvention)]
        internal static extern IntPtr tfeBufferGetData(IntPtr buffer);

        [DllImport(ExternLibrary, CallingConvention = TfInvoke.TFCallingConvention)]
        internal static extern int tfeBufferGetLength(IntPtr buffer);
    }
}
