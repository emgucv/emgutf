//----------------------------------------------------------------------------
//  Copyright (C) 2004-2018 by EMGU Corporation. All rights reserved.       
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
    }
}