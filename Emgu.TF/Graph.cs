//----------------------------------------------------------------------------
//  Copyright (C) 2004-2020 by EMGU Corporation. All rights reserved.       
//----------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Text;
using Emgu.TF.Util;
using System.Runtime.InteropServices;
using System.Collections;

namespace Emgu.TF
{
    /// <summary>
    /// Tensorflow Graph
    /// </summary>
    public partial class Graph : UnmanagedObject, IEnumerable<Operation> 
    {
        
        /// <summary>
        /// Create a new Graph
        /// </summary>
        public Graph()
        {
            _ptr = TfInvoke.tfeNewGraph();
        }

        /// <summary>
        /// Sets the shape of the Tensor referenced by <paramref name="output"/> in graph to
        /// the shape described by <paramref name="dims"/>.
        /// </summary>
        /// <param name="output">The output</param>
        /// <param name="dims">The shape</param>
        /// <param name="status">The status</param>
        public void SetTensorShape(Output output, int[] dims, Status status = null)
        {
            GCHandle handle = GCHandle.Alloc(dims, GCHandleType.Pinned);
            using (StatusChecker checker = new StatusChecker(status))
                TfInvoke.tfeGraphSetTensorShape(_ptr, output.Operation, output.Index, handle.AddrOfPinnedObject(), dims.Length, checker.Status);
        }

        /// <summary>
        /// Returns the shape of the Tensor
        /// </summary>
        /// <param name="output">The output</param>
        /// <param name="status">The status</param>
        /// <returns>The shape of the Tensor</returns>
        public int[] GetTensorShape(Output output, Status status = null)
        {
            using (StatusChecker checker = new StatusChecker(status))
            {
                int numDim = TfInvoke.tfeGraphGetTensorNumDims(_ptr, output.Operation, output.Index, checker.Status);
                if (numDim < 0)
                    return null;
                else if (numDim == 0)
                    return new int[0];

                int[] dims = new int[numDim];
                GCHandle handle = GCHandle.Alloc(dims, GCHandleType.Pinned);
                TfInvoke.tfeGraphGetTensorShape(_ptr, output.Operation, output.Index, handle.AddrOfPinnedObject(), numDim, checker.Status);
                handle.Free();
                return dims;
            }
        }

        /// <summary>
        /// Release all the unmanaged memory associated with the graph
        /// </summary>
        protected override void DisposeObject()
        {
            if (IntPtr.Zero != _ptr)
                TfInvoke.tfeDeleteGraph(ref _ptr);
        }

        /// <summary>
        /// Import the graph serialized in <paramref name="graphDef"/> into the current graph.
        /// Convenience function for when no return outputs have been added.
        /// </summary>
        /// <param name="graphDef">The GraphDef to be imported</param>
        /// <param name="options">The import options</param>
        /// <param name="status">The status</param>
        public void ImportGraphDef(Buffer graphDef, ImportGraphDefOptions options, Status status = null)
        {
            using (StatusChecker checker = new StatusChecker(status))
                TfInvoke.tfeGraphImportGraphDef(_ptr, graphDef, options, checker.Status);
        }

        /// <summary>
        /// Operation will only be added to graph when FinishOperation() is
        /// called (assuming FinishOperation() does not return an error).
        /// Graph must not be deleted until after FinishOperation() is
        /// called.
        /// </summary>
        /// <param name="opType">The operation type</param>
        /// <param name="opName">The name of the operation</param>
        /// <returns>A new operation description</returns>
        public OperationDescription NewOperation(String opType, String opName)
        {
            return new OperationDescription(TfInvoke.tfeNewOperation(_ptr, opType, opName));
        }

        /// <summary>
        /// Returns the operation in the graph with the specific name. Returns null if
        /// no operation found.
        /// </summary>
        /// <param name="opName">The name of the operation</param>
        /// <returns>
        /// Returns the operation in the graph with the specific name. Returns null if
        /// no operation found.
        /// </returns>
        public Operation OperationByName(String opName)
        {
            IntPtr ptr = TfInvoke.tfeGraphOperationByName(_ptr, opName);
            if (ptr == IntPtr.Zero)
                return null;
            return new Operation(ptr);
        }

        /// <summary>
        /// Returns the operation in the graph with the specific name. Returns null if
        /// no operation found.
        /// </summary>
        /// <param name="name">The name of the operation</param>
        /// <returns>
        /// Returns the operation in the graph with the specific name. Returns null if
        /// no operation found.
        /// </returns>
        public Operation this[string name]
        {
            get { return OperationByName(name); }
        }

        /// <summary>
        /// Iterate through the operations of a graph.
        /// </summary>
        /// <param name="pos">The position pointer that can be used to iterate though the operations of this graph. Use IntPtr.Zero to get the first operation</param>
        /// <returns>The next operation from the position</returns>
        public Operation NextOperation(ref IntPtr pos)
        {
            return new Operation(TfInvoke.tfeGraphNextOperation(_ptr, ref pos));
        }

        /// <summary>
        /// Write out a serialized representation of `graph` (as a GraphDef protocol
        /// message).
        /// </summary>
        /// <param name="outputGraphDef">The buffer to store the GraphDef</param>
        /// <param name="status">The status</param>
        public void ToGraphDef(Buffer outputGraphDef, Status status = null)
        {
            using (StatusChecker checker = new StatusChecker(status))
                TfInvoke.tfeGraphToGraphDef(_ptr, outputGraphDef, checker.Status);
        }

        /// <summary>
        /// Returns the serialized VersionDef proto for this graph.
        /// </summary>
        /// <return>The serialized VersionDef proto for this graph.</return>
        /// <param name="status">The status</param>
        public Buffer Versions(Status status = null)
        {
            using (StatusChecker checker = new StatusChecker(status))
            {
                return new Buffer(
                    TfInvoke.tfeGraphVersions(_ptr, checker.Status),
                    true);
            }
        }

        /// <summary>
        /// Get the operation definition in protobuf format
        /// </summary>
        /// <param name="opName">The name of the operation</param>
        /// <param name="status">The status</param>
        /// <returns>The operation definition in protobuf format</returns>
        public Buffer GetOpDef(
            String opName,
            Status status = null)
        {
            using (StatusChecker checker = new StatusChecker(status))
            {
                Buffer buffer = new Buffer();
                TfInvoke.tfeGraphGetOpDef(_ptr, opName, buffer, checker.Status);
                return buffer;
            }
        }

        /// <summary>
        /// Returns the number of TF_Functions registered in the graph
        /// </summary>
        public int NumFunctions
        {
            get
            {
                return TfInvoke.tfeGraphNumFunctions(_ptr);
            }
        }

        /// <summary>
        /// Get the names of the operations
        /// </summary>
        /// <param name="opNames">If provided, this HashSet will be filled with names of operations</param>
        /// <param name="potentialInputs">If provided, this HashSet will be filled with names of operations that may be a input operation.</param>
        /// <param name="potentialOutputs">If provided, this HashSet will be filled with names of operations that may be an output operation.</param>
        public void GetOpNames(
            HashSet<string> opNames = null,
            HashSet<string> potentialInputs = null,
            HashSet<string> potentialOutputs = null)
        {
            foreach (Operation op in this)
            {
                String name = op.Name;

                if (potentialInputs != null)
                {
                    if (op.NumInputs == 0 && op.OpType.Equals("Placeholder"))
                    {
                        potentialInputs.Add(name);
                    }
                }

                if (potentialOutputs != null)
                    foreach (Output output in op.Outputs)
                    {
                        int[] shape = GetTensorShape(output);
                        if (output.NumConsumers == 0)
                        {
                            potentialOutputs.Add(name);
                        }
                    }

                opNames?.Add(name);
            }
        }

        /// <summary>
        /// Get an enumerator of the Operations in this Graph
        /// </summary>
        /// <returns>An enumerator of the Operations in this Graph</returns>
        public IEnumerator<Operation> GetEnumerator()
        {
            IntPtr pos = IntPtr.Zero;
            Operation op = NextOperation(ref pos);
            while (!op.Empty)
            {
                yield return op;
                op = NextOperation(ref pos);
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }

    public static partial class TfInvoke
    {

        [DllImport(ExternLibrary, CallingConvention = TfInvoke.TFCallingConvention)]
        internal static extern IntPtr tfeNewGraph();

        [DllImport(ExternLibrary, CallingConvention = TfInvoke.TFCallingConvention)]
        internal static extern void tfeDeleteGraph(ref IntPtr buffer);

        [DllImport(ExternLibrary, CallingConvention = TfInvoke.TFCallingConvention)]
        internal static extern void tfeGraphImportGraphDef(IntPtr graph, IntPtr graph_def, IntPtr options, IntPtr status);

        [DllImport(ExternLibrary, CallingConvention = TfInvoke.TFCallingConvention)]
        internal static extern void tfeGraphToGraphDef(IntPtr graph, IntPtr outputGraphDef, IntPtr status);

        [DllImport(ExternLibrary, CallingConvention = TfInvoke.TFCallingConvention)]
        internal static extern IntPtr tfeNewOperation(
            IntPtr graph,
            [MarshalAs(StringMarshalType)]
            String opType,
            [MarshalAs(StringMarshalType)]
            String opName);

        [DllImport(ExternLibrary, CallingConvention = TfInvoke.TFCallingConvention)]
        internal static extern IntPtr tfeGraphOperationByName(
            IntPtr graph,
            [MarshalAs(StringMarshalType)]
            String opName);

        [DllImport(ExternLibrary, CallingConvention = TfInvoke.TFCallingConvention)]
        internal static extern IntPtr tfeGraphNextOperation(IntPtr graph, ref IntPtr pos);

        [DllImport(ExternLibrary, CallingConvention = TfInvoke.TFCallingConvention)]
        internal static extern void tfeGraphSetTensorShape(IntPtr graph, IntPtr outputOperation, int idx, IntPtr dims, int numDims, IntPtr status);

        [DllImport(ExternLibrary, CallingConvention = TfInvoke.TFCallingConvention)]
        internal static extern int tfeGraphGetTensorNumDims(IntPtr graph, IntPtr outputOperation, int idx, IntPtr status);

        [DllImport(ExternLibrary, CallingConvention = TfInvoke.TFCallingConvention)]
        internal static extern void tfeGraphGetTensorShape(IntPtr graph, IntPtr outputOperation, int idx, IntPtr dims, int numDims, IntPtr status);

        [DllImport(ExternLibrary, CallingConvention = TfInvoke.TFCallingConvention)]
        internal static extern void tfeGraphGetOpDef(
            IntPtr graph,
            [MarshalAs(StringMarshalType)]
            String opName, 
            IntPtr outputOpDef, 
            IntPtr status);

        [DllImport(ExternLibrary, CallingConvention = TfInvoke.TFCallingConvention)]
        internal static extern IntPtr tfeGraphVersions(IntPtr graph, IntPtr status);

        [DllImport(ExternLibrary, CallingConvention = TfInvoke.TFCallingConvention)]
        internal static extern int tfeGraphNumFunctions(IntPtr g);
    }
}
