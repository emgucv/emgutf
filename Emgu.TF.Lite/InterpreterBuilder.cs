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

    public class InterpreterBuilder : Emgu.TF.Util.UnmanagedObject
    {

        public InterpreterBuilder(FlatBufferModel flatBufferModel, IOpResolver resolver)
        {
            _ptr = TfLiteInvoke.tfeInterpreterBuilderCreate(flatBufferModel, resolver.OpResolverPtr);
        }

        public Status Build(Interpreter interpreter)
        {
            return TfLiteInvoke.tfeInterpreterBuilderBuild(_ptr, interpreter);
        }

        /// <summary>
        /// Release all the unmanaged memory associated with this model
        /// </summary>
        protected override void DisposeObject()
        {
            if (IntPtr.Zero != _ptr)
                TfLiteInvoke.tfeInterpreterBuilderRelease(ref _ptr);
        }
    }

    public static partial class TfLiteInvoke
    {
        [DllImport(ExternLibrary, CallingConvention = TfLiteInvoke.TFCallingConvention)]
        internal static extern IntPtr tfeInterpreterBuilderCreate(IntPtr model, IntPtr opResolver);

        [DllImport(ExternLibrary, CallingConvention = TfLiteInvoke.TFCallingConvention)]
        internal static extern void tfeInterpreterBuilderRelease(ref IntPtr builder);

        [DllImport(ExternLibrary, CallingConvention = TfLiteInvoke.TFCallingConvention)]
        internal static extern Status tfeInterpreterBuilderBuild(IntPtr builder, IntPtr interpreter);

    }
}
*/