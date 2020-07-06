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
    /// The Library that can be used to load new Tensorflow modules.
    /// </summary>
    public class Library : UnmanagedObject
    {
        /// <summary>
        /// Load the library specified by libraryFilename and register the ops and
        /// kernels present in that library.
        /// </summary>
        /// <param name="libraryFilename">The library file name</param>
        /// <param name="status">The status</param>
        public Library(String libraryFilename, Status status = null)
        {
            using (StatusChecker checker = new StatusChecker(status))
                _ptr = TfInvoke.tfeLoadLibrary(libraryFilename, checker.Status);
        }

        /// <summary>
        /// Get the OpList of OpDefs defined in the library
        /// </summary>
        /// <returns>Returns a Buffer. The memory pointed to by the result is owned by
        /// lib_handle. The data in the buffer will be the serialized OpList proto for
        /// ops defined in the library.
        /// </returns>
        public Buffer GetOpList()
        {
            return new Buffer(TfInvoke.tfeGetOpList(_ptr), false);
        }

        /// <summary>
        /// Release the unmanaged memory associated with this Library.
        /// </summary>
        protected override void DisposeObject()
        {
            if (IntPtr.Zero != _ptr)
            {
                TfInvoke.tfeDeleteLibraryHandle(ref _ptr);
            }
        }
    }

    public static partial class TfInvoke
    {
        [DllImport(ExternLibrary, CallingConvention = TfInvoke.TFCallingConvention)]
        internal static extern IntPtr tfeLoadLibrary(
            [MarshalAs(TfInvoke.StringMarshalType)]
            String libraryFilename,
            IntPtr status);

        [DllImport(ExternLibrary, CallingConvention = TfInvoke.TFCallingConvention)]
        internal static extern IntPtr tfeGetOpList(IntPtr libHandle);

        [DllImport(ExternLibrary, CallingConvention = TfInvoke.TFCallingConvention)]
        internal static extern void tfeDeleteLibraryHandle(ref IntPtr libHandle);
    }
}
