//----------------------------------------------------------------------------
//  Copyright (C) 2004-2020 by EMGU Corporation. All rights reserved.       
//----------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Text;
using Emgu.TF.Util;
using System.Runtime.InteropServices;

namespace Emgu.TF
{
    /// <summary>
    /// Tensorflow session options
    /// </summary>
    public class SessionOptions : UnmanagedObject
    {
        /// <summary>
        /// Construct a new session options
        /// </summary>
        public SessionOptions()
        {
            _ptr = TfInvoke.tfeNewSessionOptions();
        }

        /// <summary>
        /// Release the unmanaged memory associated with this session options.
        /// </summary>
        protected override void DisposeObject()
        {
            if (IntPtr.Zero != _ptr)
            {
                TfInvoke.tfeDeleteSessionOptions(ref _ptr);
            }
        }

        /// <summary>
        /// Set the target in TF_SessionOptions.options.
        /// </summary>
        /// <param name="target">
        /// target can be empty, a single entry, or a comma separated list of entries.
        /// Each entry is in one of the following formats :
        /// "local"
        /// ip:port
        /// host:port
        /// </param>
        public void SetTarget(String target)
        {
            TfInvoke.tfeSetTarget(_ptr, target);
        }

        /// <summary>
        /// Set the config in TF_SessionOptions.options.
        /// If config was not parsed successfully as a ConfigProto, record the
        /// error information in <paramref name="status"/>.
        /// </summary>
        /// <param name="proto">Config should be a serialized tensorflow.ConfigProto proto.</param>
        /// <param name="status">The status</param>
        public void SetConfig(byte[] proto, Status status = null)
        {
            using (StatusChecker checker = new StatusChecker(status))
            {
                GCHandle handle = GCHandle.Alloc(proto, GCHandleType.Pinned);
                TfInvoke.tfeSetConfig(_ptr, handle.AddrOfPinnedObject(), proto.Length, checker.Status);
                handle.Free();
            }
        }
    }

    public static partial class TfInvoke
    {

        [DllImport(ExternLibrary, CallingConvention = TfInvoke.TFCallingConvention)]
        internal static extern IntPtr tfeNewSessionOptions();

        [DllImport(ExternLibrary, CallingConvention = TfInvoke.TFCallingConvention)]
        internal static extern void tfeDeleteSessionOptions(ref IntPtr options);

        [DllImport(ExternLibrary, CallingConvention = TfInvoke.TFCallingConvention)]
        internal static extern void tfeSetTarget(
            IntPtr options,
            [MarshalAs(StringMarshalType)]
            String target);

        [DllImport(ExternLibrary, CallingConvention = TfInvoke.TFCallingConvention)]
        internal static extern void tfeSetConfig(IntPtr options, IntPtr proto, int protoLen, IntPtr status);
    }
}
