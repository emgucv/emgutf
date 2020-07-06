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
    /// The status
    /// </summary>
    public class Status : UnmanagedObject
    {
        /// <summary>
        /// The status code
        /// </summary>
        public enum Code
        {
            /// <summary>
            /// Ok
            /// </summary>
            Ok = 0,
            /// <summary>
            /// Canceled
            /// </summary>
            Cancelled = 1,
            /// <summary>
            /// Unknown
            /// </summary>
            Unknown = 2,
            /// <summary>
            /// Invalid argument
            /// </summary>
            InvalidArgument = 3,
            /// <summary>
            /// Deadline exceed
            /// </summary>
            DeadlineExceeded = 4,
            /// <summary>
            /// Not found
            /// </summary>
            NotFound = 5,
            /// <summary>
            /// Already exists
            /// </summary>
            AlreadyExists = 6,
            /// <summary>
            /// Permission denied
            /// </summary>
            PermissionDenied = 7,
            /// <summary>
            /// Unauthenticated
            /// </summary>
            Unauthenticated = 16,
            /// <summary>
            /// Resource exhausted
            /// </summary>
            ResourceExhausted = 8,
            /// <summary>
            /// Failed precondition
            /// </summary>
            FailedPrecondition = 9,
            /// <summary>
            /// Aborted
            /// </summary>
            Aborted = 10,
            /// <summary>
            /// Out of range
            /// </summary>
            OutOfRange = 11,
            /// <summary>
            /// Unimplemented
            /// </summary>
            Unimplemented = 12,
            /// <summary>
            /// Internal
            /// </summary>
            Internal = 13,
            /// <summary>
            /// Unavailable
            /// </summary>
            Unavailable = 14,
            /// <summary>
            /// Data loss
            /// </summary>
            DataLoss = 15,
        }

        /// <summary>
        /// Create a new Status
        /// </summary>
        public Status()
        {
            _ptr = TfInvoke.tfeNewStatus();
        }

        /// <summary>
        /// Get the message from the status
        /// </summary>
        public string Message
        {
            get
            {
                if (_ptr == IntPtr.Zero)
                    return "Status Object has been disposed.";
                IntPtr msgPtr = TfInvoke.tfeMessage(_ptr);
                return Marshal.PtrToStringAnsi(msgPtr);
            }
        }

        /// <summary>
        /// Get the status code
        /// </summary>
        public Code StatusCode
        {
            get
            {
                if (_ptr == IntPtr.Zero)
                    return Code.Unavailable;
                return TfInvoke.tfeGetCode(_ptr);
            }
        }

        /// <summary>
        /// Get the pointer to the native status
        /// </summary>
        public override IntPtr Ptr
        {
            get
            {
                if (_ptr == IntPtr.Zero)
                    return _ptr;

                return base.Ptr;
            }
        }

        /// <summary>
        /// Release the unmanaged memory associated with this status
        /// </summary>
        protected override void DisposeObject()
        {
            if (IntPtr.Zero != _ptr)
                TfInvoke.tfeDeleteStatus(ref _ptr);
        }
    }

    public static partial class TfInvoke
    {

        [DllImport(ExternLibrary, CallingConvention = TfInvoke.TFCallingConvention)]
        internal static extern IntPtr tfeNewStatus();

        [DllImport(ExternLibrary, CallingConvention = TfInvoke.TFCallingConvention)]
        internal static extern void tfeDeleteStatus(ref IntPtr status);

        [DllImport(ExternLibrary, CallingConvention = TfInvoke.TFCallingConvention)]
        internal static extern Status.Code tfeGetCode(IntPtr s);

        [DllImport(ExternLibrary, CallingConvention = TfInvoke.TFCallingConvention)]
        internal static extern IntPtr tfeMessage(IntPtr s);
    }
}
