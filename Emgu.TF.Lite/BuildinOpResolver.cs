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
    
    public class BuildinOpResolver : Emgu.TF.Util.UnmanagedObject, IOpResolver
    {
        private IntPtr _opResolverPtr;
        
        public BuildinOpResolver()
        {
            _ptr = TfLiteInvoke.tfeBuiltinOpResolverCreate(ref _opResolverPtr);
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
                TfLiteInvoke.tfeBuiltinOpResolverRelease(ref _ptr);
                _opResolverPtr = IntPtr.Zero;
            }
        }
    }

    public static partial class TfLiteInvoke
    {
        [DllImport(ExternLibrary, CallingConvention = TfLiteInvoke.TFCallingConvention)]
        internal static extern IntPtr  tfeBuiltinOpResolverCreate(ref IntPtr opResolver);

        [DllImport(ExternLibrary, CallingConvention = TfLiteInvoke.TFCallingConvention)]
        internal static extern void tfeBuiltinOpResolverRelease(ref IntPtr resolver);

    }
}
