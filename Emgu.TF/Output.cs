//----------------------------------------------------------------------------
//  Copyright (C) 2004-2020 by EMGU Corporation. All rights reserved.       
//----------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace Emgu.TF
{
    /// <summary>
    /// Represents a specific output of an operation.
    /// </summary>
    public class Output
    {
        /// <summary>
        /// The Operation
        /// </summary>
        public Operation Operation;

        /// <summary>
        /// The index of the input within the operation
        /// </summary>
        public int Index;

        /// <summary>
        /// Create an output by specifying the operation and index.
        /// </summary>
        /// <param name="operation">The Operation</param>
        /// <param name="index">The index of the input within the operation</param>
        public Output(Operation operation, int index)
        {
            Operation = operation;
            Index = index;
        }

        /// <summary>
        /// Get the output type
        /// </summary>
        public DataType OutputType
        {
            get
            {
                return TfInvoke.tfeOperationOutputType(Operation.Ptr, Index);
            }
        }

        /// <summary>
        /// Get the number of comsumers
        /// </summary>
        public int NumConsumers
        {
            get
            {
                return TfInvoke.tfeOperationOutputNumConsumers(Operation.Ptr, Index);
            }
        }

        /// <summary>
        /// Get the consumers for this Output
        /// </summary>
        public Input[] Consumers
        {
            get
            {
                int numComsumers = NumConsumers;
                IntPtr[] operations = new IntPtr[numComsumers];
                int[] inputIdx = new int[numComsumers];
                GCHandle opHandle = GCHandle.Alloc(operations, GCHandleType.Pinned);
                GCHandle idxHandle = GCHandle.Alloc(inputIdx, GCHandleType.Pinned);
                TfInvoke.tfeOperationOutputConsumers(Operation.Ptr, Index, opHandle.AddrOfPinnedObject(), idxHandle.AddrOfPinnedObject(), numComsumers);
                opHandle.Free();
                idxHandle.Free();

                Input[] result = new Input[numComsumers];
                for (int i = 0; i < numComsumers; i++)
                {
                    result[i] = new Input(new Operation(operations[i]), inputIdx[i]);
                }
                return result;
            }
        }
    }

    public static partial class TfInvoke
    {

        [DllImport(ExternLibrary, CallingConvention = TfInvoke.TFCallingConvention)]
        internal static extern DataType tfeOperationOutputType(IntPtr oper, int idx);

        [DllImport(ExternLibrary, CallingConvention = TfInvoke.TFCallingConvention)]
        internal static extern int tfeOperationOutputNumConsumers(IntPtr oper, int idx);

        [DllImport(ExternLibrary, CallingConvention = TfInvoke.TFCallingConvention)]
        internal static extern int tfeOperationOutputConsumers(IntPtr operOut, int outIdx, IntPtr consumers, IntPtr inputIdx, int maxConsumers);
    }
}