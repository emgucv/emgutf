//----------------------------------------------------------------------------
//  Copyright (C) 2004-2020 by EMGU Corporation. All rights reserved.       
//----------------------------------------------------------------------------

/*

using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using Emgu.TF.Util;

namespace Emgu.TF.Lite
{
    
    public class MutableOpResolver : Emgu.TF.Util.UnmanagedObject, IOpResolver
    {
        private IntPtr _opResolverPtr;
        
        public MutableOpResolver()
        {
            _ptr = TfLiteInvoke.tfeMutableOpResolverCreate(ref _opResolverPtr);
        }
        
        IntPtr IOpResolver.OpResolverPtr
        {
            get
            {
                return _opResolverPtr;
            }
        }

        /// <summary>
        /// Release all the unmanaged memory associated with this model
        /// </summary>
        protected override void DisposeObject()
        {
            if (IntPtr.Zero != _ptr)
            {
                TfLiteInvoke.tfeMutableOpResolverRelease(ref _ptr);
                _opResolverPtr = IntPtr.Zero;
            }
        }
    }

    public static partial class TfLiteInvoke
    {
        [DllImport(ExternLibrary, CallingConvention = TfLiteInvoke.TFCallingConvention)]
        internal static extern IntPtr tfeMutableOpResolverCreate(ref IntPtr opResolver);

        [DllImport(ExternLibrary, CallingConvention = TfLiteInvoke.TFCallingConvention)]
        internal static extern void tfeMutableOpResolverRelease(ref IntPtr resolver);

    }
}

*/