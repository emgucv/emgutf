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
    /// Represents a specific input of an operation.
    /// </summary>
    public class Input
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
        /// Create an input by specifying the operation and index.
        /// </summary>
        /// <param name="operation">The operation</param>
        /// <param name="index">The index of the input within the operation</param>
        public Input(Operation operation, int index)
        {
            Operation = operation;
            Index = index;
        }

        /// <summary>
        /// Get the input type of the specific input index
        /// </summary>
        /// <returns>The input type of the specific input index</returns>
        public DataType InputType
        {
            get
            {
                return TfInvoke.tfeOperationInputType(Operation.Ptr, Index);
            }
        }
    }

    public static partial class TfInvoke
    {
        [DllImport(ExternLibrary, CallingConvention = TfInvoke.TFCallingConvention)]
        internal static extern DataType tfeOperationInputType(IntPtr oper, int idx);
    }
}
