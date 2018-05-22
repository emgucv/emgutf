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
    /// <summary>
    /// An RAII object that represents a read-only tflite model, copied from disk,
    /// or mmapped. This uses flatbuffers as the serialization format.
    /// </summary>   
    public class FlatBufferModel : Emgu.TF.Util.UnmanagedObject
    {
        /// <summary>
        /// Builds a model based on a file.
        /// </summary>   
        /// <param name="filename">The name of the file where the FlatBufferModel will be loaded from.</param>
        public FlatBufferModel(String filename)
        {
            _ptr = TfLiteInvoke.tfeFlatBufferModelBuildFromFile(filename);
        }

        private byte[] _buffer = null;
        private GCHandle _handle;

        /// <summary>
        /// Builds a model based on a pre-loaded flatbuffer.
        /// </summary>   
        /// <param name="buffer">The buffer where the FlatBufferModel will be loaded from.</param>
        public FlatBufferModel(byte[] buffer)
        {
            _buffer = new byte[buffer.Length];
            Array.Copy(buffer, _buffer, _buffer.Length);
            _handle = GCHandle.Alloc(_buffer, GCHandleType.Pinned);
            try
            {
                _ptr = TfLiteInvoke.tfeFlatBufferModelBuildFromBuffer(_handle.AddrOfPinnedObject(), buffer.Length);
            } catch
            {
                _handle.Free();
                _buffer = null;
                throw;
            }

        }

        /// <summary>
        /// Returns true if the model is initialized
        /// </summary>   
        public bool Initialized
        {
            get
            {
                return TfLiteInvoke.tfeFlatBufferModelInitialized(_ptr);
            }
        }

        /// <summary>
        ///Returns true if the model identifier is correct (otherwise false and
        /// reports an error).
        /// </summary> 
        public bool CheckModelIdentifier()
        {
            return TfLiteInvoke.tfeFlatBufferModelCheckModelIdentifier(_ptr);
        }

        /// <summary>
        /// Release all the unmanaged memory associated with this model
        /// </summary>
        protected override void DisposeObject()
        {
            if (IntPtr.Zero != _ptr)
                TfLiteInvoke.tfeFlatBufferModelRelease(ref _ptr);
            if (_buffer != null)
            {
                _handle.Free();
                _buffer = null;

            }
        }
    }

    public static partial class TfLiteInvoke
    {
        [DllImport(ExternLibrary, CallingConvention = TfLiteInvoke.TFCallingConvention)]
        internal static extern IntPtr tfeFlatBufferModelBuildFromFile(
            [MarshalAs(StringMarshalType)]
            String filename);

        [DllImport(ExternLibrary, CallingConvention = TfLiteInvoke.TFCallingConvention)]
        internal static extern IntPtr tfeFlatBufferModelBuildFromBuffer(IntPtr buffer, int bufferSize);

        [DllImport(ExternLibrary, CallingConvention = TfLiteInvoke.TFCallingConvention)]
        internal static extern void tfeFlatBufferModelRelease(ref IntPtr model);


        [DllImport(ExternLibrary, CallingConvention = TfLiteInvoke.TFCallingConvention)]
        [return: MarshalAs(BoolMarshalType)]
        internal static extern bool tfeFlatBufferModelInitialized(IntPtr model);

        [DllImport(ExternLibrary, CallingConvention = TfLiteInvoke.TFCallingConvention)]
        [return: MarshalAs(BoolMarshalType)]
        internal static extern bool tfeFlatBufferModelCheckModelIdentifier(IntPtr model);

    }
}
