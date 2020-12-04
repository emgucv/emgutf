//----------------------------------------------------------------------------
//  Copyright (C) 2004-2020 by EMGU Corporation. All rights reserved.       
//----------------------------------------------------------------------------


using System;
using Emgu.TF.Lite;
using System.Runtime.InteropServices;

namespace Emgu.TF
{
    /// <summary>
    /// TfInvoke for iOS
    /// </summary>
    public static class TfLiteInvokeIOS
    {
        private static readonly bool _libraryLoaded;

        static TfLiteInvokeIOS()
        {
            _libraryLoaded = TfLiteInvoke.CheckLibraryLoaded();
            if (_libraryLoaded)
                TfLiteInvoke.RedirectError(TfLiteInvokeIOS.TfliteErrorHandlerThrowException);
        }
        [ObjCRuntime.MonoPInvokeCallback(typeof(TfLiteInvoke.TfliteErrorCallback))]
        private static int TfliteErrorHandler(
           int status,
           IntPtr errMsg)
        {
            String msg = Marshal.PtrToStringAnsi(errMsg);
            throw new Exception(msg);
        }

        /// <summary>
        /// Return true if the class is loaded.
        /// </summary>
        public static bool CheckLibraryLoaded()
        {
            return _libraryLoaded;
        }


        /// <summary>
        /// The default error handler for tensorflow lite
        /// </summary>

        public static readonly TfLiteInvoke.TfliteErrorCallback TfliteErrorHandlerThrowException = (TfLiteInvoke.TfliteErrorCallback)TfliteErrorHandler;
    }
}
