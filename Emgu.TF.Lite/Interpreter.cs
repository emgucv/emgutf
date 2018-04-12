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
    
    public class Interpreter : Emgu.TF.Util.UnmanagedObject
    {
        
        public Interpreter()
        {
            _ptr = TfLiteInvoke.tfeInterpreterCreate();
        }

        public Status AllocateTensors()
        {
            return TfLiteInvoke.tfeInterpreterAllocateTensors(_ptr);
        }

        public Status Invoke()
        {
            return TfLiteInvoke.tfeInterpreterInvoke(_ptr);
        }

        public IntPtr GetInputTensorPtr(int index)
        {
            return TfLiteInvoke.tfeInterpreterInputTensor(_ptr, index);
        }

        public IntPtr GetOutputTensorPtr(int index)
        {
            return TfLiteInvoke.tfeInterpreterOuputTensor(_ptr, index);
        }

        public int TensorSize
        {
            get
            {
                return TfLiteInvoke.tfeInterpreterTensorSize(_ptr);
            }
        }

        public int NodeSize
        {
            get
            {
                return TfLiteInvoke.tfeInterpreterNodesSize(_ptr);
            }
        }

        public int[] GetInput()
        {
            int size = TfLiteInvoke.tfeInterpreterGetInputSize(_ptr);
            int[] input = new int[size];
            GCHandle handle = GCHandle.Alloc(input, GCHandleType.Pinned);
            TfLiteInvoke.tfeInterpreterGetInput(_ptr, handle.AddrOfPinnedObject());
            handle.Free();
            return input;
        }

        public int[] GetOutput()
        {
            int size = TfLiteInvoke.tfeInterpreterGetOutputSize(_ptr);
            int[] output = new int[size];
            GCHandle handle = GCHandle.Alloc(output, GCHandleType.Pinned);
            TfLiteInvoke.tfeInterpreterGetOutput(_ptr, handle.AddrOfPinnedObject());
            handle.Free();
            return output;
        }

        public String GetInputName(int index)
        {
            IntPtr namePtr = TfLiteInvoke.tfeInterpreterGetInputName(_ptr, index);
            return Marshal.PtrToStringAnsi(namePtr);
        }

        public String GetOutputName(int index)
        {
            IntPtr namePtr = TfLiteInvoke.tfeInterpreterGetOutputName(_ptr, index);
            return Marshal.PtrToStringAnsi(namePtr);
        }

        /// <summary>
        /// Release all the unmanaged memory associated with this interpreter
        /// </summary>
        protected override void DisposeObject()
        {
            if (IntPtr.Zero != _ptr)
                TfLiteInvoke.tfeInterpreterRelease(ref _ptr);
        }
    }

    public static partial class TfLiteInvoke
    {
        [DllImport(ExternLibrary, CallingConvention = TfLiteInvoke.TFCallingConvention)]
        internal static extern IntPtr tfeInterpreterCreate();

        [DllImport(ExternLibrary, CallingConvention = TfLiteInvoke.TFCallingConvention)]
        internal static extern Status tfeInterpreterAllocateTensors(IntPtr interpreter);

        [DllImport(ExternLibrary, CallingConvention = TfLiteInvoke.TFCallingConvention)]
        internal static extern Status tfeInterpreterInvoke(IntPtr interpreter);

        [DllImport(ExternLibrary, CallingConvention = TfLiteInvoke.TFCallingConvention)]
        internal static extern IntPtr tfeInterpreterInputTensor(IntPtr interpreter, int index);

        [DllImport(ExternLibrary, CallingConvention = TfLiteInvoke.TFCallingConvention)]
        internal static extern IntPtr tfeInterpreterOuputTensor(IntPtr interpreter, int index);

        [DllImport(ExternLibrary, CallingConvention = TfLiteInvoke.TFCallingConvention)]
        internal static extern int tfeInterpreterTensorSize(IntPtr interpreter);

        [DllImport(ExternLibrary, CallingConvention = TfLiteInvoke.TFCallingConvention)]
        internal static extern int tfeInterpreterNodesSize(IntPtr interpreter);

        [DllImport(ExternLibrary, CallingConvention = TfLiteInvoke.TFCallingConvention)]
        internal static extern int tfeInterpreterGetInputSize(IntPtr interpreter);
        [DllImport(ExternLibrary, CallingConvention = TfLiteInvoke.TFCallingConvention)]
        internal static extern void tfeInterpreterGetInput(IntPtr interpreter, IntPtr input);

        [DllImport(ExternLibrary, CallingConvention = TfLiteInvoke.TFCallingConvention)]
        internal static extern IntPtr tfeInterpreterGetInputName(IntPtr interpreter, int index);

        [DllImport(ExternLibrary, CallingConvention = TfLiteInvoke.TFCallingConvention)]
        internal static extern int tfeInterpreterGetOutputSize(IntPtr interpreter);
        [DllImport(ExternLibrary, CallingConvention = TfLiteInvoke.TFCallingConvention)]
        internal static extern void tfeInterpreterGetOutput(IntPtr interpreter, IntPtr output);

        [DllImport(ExternLibrary, CallingConvention = TfLiteInvoke.TFCallingConvention)]
        internal static extern IntPtr tfeInterpreterGetOutputName(IntPtr interpreter, int index);

        [DllImport(ExternLibrary, CallingConvention = TfLiteInvoke.TFCallingConvention)]
        internal static extern void tfeInterpreterRelease(ref IntPtr interpreter);


    }
}
