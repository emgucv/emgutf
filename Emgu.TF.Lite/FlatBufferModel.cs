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
    
    public class FlatBufferModel : Emgu.TF.Util.UnmanagedObject
    {
        
        public FlatBufferModel(String filename)
        {
            _ptr = TfLiteInvoke.tfeFlatBufferModelBuildFromFile(filename);
        }

        public FlatBufferModel(byte[] buffer)
        {
            GCHandle handle = GCHandle.Alloc(buffer, GCHandleType.Pinned);
            try
            {
                _ptr = TfLiteInvoke.tfeFlatBufferModelBuildFromBuffer(handle.AddrOfPinnedObject(), buffer.Length);
            }
            finally
            {
                handle.Free();
            }
        }

        public bool Initialized
        {
            get
            {
                return TfLiteInvoke.tfeFlatBufferModelInitialized(_ptr);
            }
        }

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
