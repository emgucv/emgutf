//----------------------------------------------------------------------------
//  Copyright (C) 2004-2019 by EMGU Corporation. All rights reserved.       
//----------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using Emgu.TF.Util;
using System.Diagnostics;

namespace Emgu.TF.Lite
{
    /// <summary>
    /// The tensorflow lite interpreter.
    /// </summary>
    public class Interpreter : Emgu.TF.Util.UnmanagedObject
    {

        /// <summary>
        /// Create an interpreter from a flatbuffer model
        /// </summary>
        /// <param name="flatBufferModel">The flat buffer model.</param>
        /// <param name="resolver">An instance that implements the Resolver interface which maps custom op names and builtin op codes to op registrations.</param>
        public Interpreter(FlatBufferModel flatBufferModel, IOpResolver resolver = null)
        {
            if (resolver == null)
            {
                using (BuildinOpResolver buildinResolver = new BuildinOpResolver())
                {
                    _ptr = TfLiteInvoke.tfeInterpreterCreateFromModel(flatBufferModel.Ptr, ((IOpResolver) buildinResolver).OpResolverPtr);
                }
            } else
                _ptr = TfLiteInvoke.tfeInterpreterCreateFromModel(flatBufferModel.Ptr, resolver.OpResolverPtr);
        }

        /// <summary>
        /// Update allocations for all tensors. This will redim dependent tensors using
        /// the input tensor dimensionality as given. This is relatively expensive.
        /// If you know that your sizes are not changing, you need not call this.
        /// </summary>
        /// <returns>Status of success or failure.</returns>
        public Status AllocateTensors()
        {
            return TfLiteInvoke.tfeInterpreterAllocateTensors(_ptr);
        }

        /// <summary>
        /// Invoke the interpreter (run the whole graph in dependency order).
        /// </summary>
        /// <returns>Status of success or failure.</returns>
        /// <remarks>It is possible that the interpreter is not in a ready state
        /// to evaluate (i.e. if a ResizeTensor() has been performed without an
        /// AllocateTensors().
        /// </remarks>
        public Status Invoke()
        {
            return TfLiteInvoke.tfeInterpreterInvoke(_ptr);
        }

        /// <summary>
        /// Get the number of tensors in the model.
        /// </summary>
        public int TensorSize
        {
            get
            {
                return TfLiteInvoke.tfeInterpreterTensorSize(_ptr);
            }
        }

        /// <summary>
        /// Get the number of ops in the model.
        /// </summary>
        public int NodeSize
        {
            get
            {
                return TfLiteInvoke.tfeInterpreterNodesSize(_ptr);
            }
        }

        /// <summary>
        /// Get a tensor data structure.
        /// </summary>
        /// <param name="index">The index of the tensor</param>
        /// <returns>The tensor in the specific index</returns>
        public Tensor GetTensor(int index)
        {
            return new Tensor(TfLiteInvoke.tfeInterpreterGetTensor(_ptr, index), false);
        }

        /// <summary>
        /// Get an array of all the input tensors
        /// </summary>
        public Tensor[] Inputs
        {
            get
            {
                int[] inputIdx = InputIndices;
                Tensor[] inputs = new Tensor[inputIdx.Length];
                for (int i = 0; i < inputs.Length; i++)
                    inputs[i] = GetTensor(inputIdx[i]);
                return inputs;
            }
        }

        /// <summary>
        /// Get an array of all the output tensors
        /// </summary>
        public Tensor[] Outputs
        {
            get
            {
                int[] outputIdx = OutputIndices;
                Tensor[] inputs = new Tensor[outputIdx.Length];
                for (int i = 0; i < inputs.Length; i++)
                    inputs[i] = GetTensor(outputIdx[i]);
                return inputs;
            }

        }

        /// <summary>
        /// Get the list of tensor index of the inputs tensors.
        /// </summary>
        public int[] InputIndices
        {
            get
            {
                int size = TfLiteInvoke.tfeInterpreterGetInputSize(_ptr);
                int[] input = new int[size];
                GCHandle handle = GCHandle.Alloc(input, GCHandleType.Pinned);
                TfLiteInvoke.tfeInterpreterGetInput(_ptr, handle.AddrOfPinnedObject());
                handle.Free();
                return input;
            }
        }

        /// <summary>
        /// Get the list of tensor index of the outputs tensors.
        /// </summary>
        public int[] OutputIndices
        {
            get
            {
                int size = TfLiteInvoke.tfeInterpreterGetOutputSize(_ptr);
                int[] output = new int[size];
                GCHandle handle = GCHandle.Alloc(output, GCHandleType.Pinned);
                int outputSize = TfLiteInvoke.tfeInterpreterGetOutput(_ptr, handle.AddrOfPinnedObject());
                Debug.Assert(outputSize == size, "Output size do not match!");
                handle.Free();
                return output;
            }
        }

        /// <summary>
        /// Enable or disable the NN API (Android Neural Network API)
        /// </summary>
        /// <param name="enable">If true, enable the NN API (Android Neural Network API). If false, disable it.</param>
        public void UseNNAPI(bool enable)
        {
            TfLiteInvoke.tfeInterpreterUseNNAPI(_ptr, enable);
        }

        /// <summary>
        /// Set the number of threads available to the interpreter.
        /// </summary>
        /// <param name="numThreads">The number of threads</param>
        public void SetNumThreads(int numThreads)
        {
            TfLiteInvoke.tfeInterpreterSetNumThreads(_ptr, numThreads);
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
        internal static extern IntPtr tfeInterpreterCreateFromModel(IntPtr model, IntPtr opResolver);

        [DllImport(ExternLibrary, CallingConvention = TfLiteInvoke.TFCallingConvention)]
        internal static extern Status tfeInterpreterAllocateTensors(IntPtr interpreter);

        [DllImport(ExternLibrary, CallingConvention = TfLiteInvoke.TFCallingConvention)]
        internal static extern Status tfeInterpreterInvoke(IntPtr interpreter);

        [DllImport(ExternLibrary, CallingConvention = TfLiteInvoke.TFCallingConvention)]
        internal static extern IntPtr tfeInterpreterGetTensor(IntPtr interpreter, int index);

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
        internal static extern int tfeInterpreterGetOutput(IntPtr interpreter, IntPtr output);

        [DllImport(ExternLibrary, CallingConvention = TfLiteInvoke.TFCallingConvention)]
        internal static extern IntPtr tfeInterpreterGetOutputName(IntPtr interpreter, int index);

        [DllImport(ExternLibrary, CallingConvention = TfLiteInvoke.TFCallingConvention)]
        internal static extern void tfeInterpreterRelease(ref IntPtr interpreter);

        [DllImport(ExternLibrary, CallingConvention = TfLiteInvoke.TFCallingConvention)]
        internal static extern void tfeInterpreterUseNNAPI(
            IntPtr interpreter, 
            [MarshalAs(TfLiteInvoke.BoolMarshalType)]
            bool enable);

        [DllImport(ExternLibrary, CallingConvention = TfLiteInvoke.TFCallingConvention)]
        internal static extern void tfeInterpreterSetNumThreads(IntPtr interpreter, int numThreads);
    }
}
