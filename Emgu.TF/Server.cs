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
    /// In-process TensorFlow server functionality, for use in distributed training.
    /// A Server instance encapsulates a set of devices and a Session target that
    /// can participate in distributed training. A server belongs to a cluster
    /// (specified by a ClusterSpec), and corresponds to a particular task in a
    /// named job. The server can communicate with any other server in the same
    /// cluster.
    /// </summary>
    public class Server : UnmanagedObject
    {
        /// <summary>
        /// Construct and return the tensorflow Server
        /// </summary>
        /// <param name="proto">Serialized FunctionDef</param>
        /// <param name="status">The status</param>
        public Server(byte[] proto, Status status = null)
        {
            using (StatusChecker checker = new StatusChecker(status))
            {
                GCHandle handle = GCHandle.Alloc(proto, GCHandleType.Pinned);
                _ptr = TfInvoke.tfeNewServer(handle.AddrOfPinnedObject(), proto.Length, checker.Status);
                handle.Free();
            }
        }

        /// <summary>
        /// Starts an in-process TensorFlow server.
        /// </summary>
        /// <param name="status">The status</param>
        public void Start(Status status = null)
        {
            using (StatusChecker checker = new StatusChecker(status))
            {
                TfInvoke.tfeServerStart(_ptr, checker.Status);
            }
        }

        /// <summary>
        /// Stops an in-process TensorFlow server.
        /// </summary>
        /// <param name="status">The status</param>
        public void Stop(Status status = null)
        {
            using (StatusChecker checker = new StatusChecker(status))
            {
                TfInvoke.tfeServerStop(_ptr, checker.Status);
            }
        }

        /// <summary>
        /// Blocks until the server has been successfully stopped
        /// </summary>
        /// <param name="status">The status</param>
        public void Join(Status status = null)
        {
            using (StatusChecker checker = new StatusChecker(status))
            {
                TfInvoke.tfeServerJoin(_ptr, checker.Status);
            }
        }

        /// <summary>
        /// Returns the target string that can be provided to TF_SetTarget() to connect a TF_Session to server
        /// </summary>
        public String Target
        {
            get
            {
                IntPtr targetPtr = TfInvoke.tfeServerTarget(_ptr);
                return Marshal.PtrToStringAnsi(targetPtr);
            }
        }

        /// <summary>
        /// Release all the unmanaged memory associated with this Server
        /// </summary>
        protected override void DisposeObject()
        {
            if (IntPtr.Zero != _ptr)
                TfInvoke.tfeDeleteServer(ref _ptr);
        }

    }

    
    public static partial class TfInvoke
    {
        [DllImport(ExternLibrary, CallingConvention = TfInvoke.TFCallingConvention)]
        internal static extern IntPtr tfeNewServer(IntPtr proto, int protoLen, IntPtr status);

    
        [DllImport(ExternLibrary, CallingConvention = TfInvoke.TFCallingConvention)]
        internal static extern void tfeDeleteServer(ref IntPtr function);

        [DllImport(ExternLibrary, CallingConvention = TfInvoke.TFCallingConvention)]
        internal static extern void tfeServerStart(IntPtr server, IntPtr status);
        [DllImport(ExternLibrary, CallingConvention = TfInvoke.TFCallingConvention)]
        internal static extern void tfeServerStop(IntPtr server, IntPtr status);
        [DllImport(ExternLibrary, CallingConvention = TfInvoke.TFCallingConvention)]
        internal static extern void tfeServerJoin(IntPtr server, IntPtr status);
        [DllImport(ExternLibrary, CallingConvention = TfInvoke.TFCallingConvention)]
        internal static extern IntPtr tfeServerTarget(IntPtr server);

    }
}
