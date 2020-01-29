//----------------------------------------------------------------------------
//  Copyright (C) 2004-2020 by EMGU Corporation. All rights reserved.       
//----------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Text;
using Emgu.TF.Util;
using System.Runtime.InteropServices;

namespace Emgu.TF
{
    /// <summary>
    /// ImportGraphDefOptions holds options that can be passed to Graph.ImportGraphDef
    /// </summary>
    public class ImportGraphDefOptions : UnmanagedObject
    {
        /// <summary>
        /// Create an empty GraphDefOptions
        /// </summary>
        public ImportGraphDefOptions()
        {
            _ptr = TfInvoke.tfeNewImportGraphDefOptions();
        }

        /// <summary>
        /// Release all the memory associated with this GraphDefOptions
        /// </summary>
        protected override void DisposeObject()
        {
            if (IntPtr.Zero != _ptr)
            {
                TfInvoke.tfeDeleteImportGraphDefOptions(ref _ptr);
            }
        }

        /// <summary>
        /// Set the prefix to be prepended to the names of nodes in `graph_def` that will
        /// be imported into `graph`.
        /// </summary>
        /// <param name="prefix">The node prefix</param>
        public void SetPrefix(String prefix)
        {
            TfInvoke.tfeImportGraphDefOptionsSetPrefix(_ptr, prefix);
        }

        /// <summary>
        /// Set the execution device for nodes in GraphDef.
        /// Only applies to nodes where a device was not already explicitly specified.
        /// </summary>
        /// <param name="device">The device name</param>
        public void SetDefaultDevice(String device)
        {
            TfInvoke.tfeImportGraphDefOptionsSetDefaultDevice(_ptr, device);
        }

        /// <summary>
        /// Set any imported nodes with input <paramref name="srcName"/>:<paramref name="srcIndex"/> to have that input
        /// replaced with <paramref name="dst"/>.
        /// </summary>
        /// <param name="srcName">Refers to a node in the graph to be imported</param>
        /// <param name="srcIndex">The source node index.</param>
        /// <param name="dst">References a node already existing in the graph being imported into</param>
        public void AddInputMapping(String srcName, int srcIndex, Output dst)
        {
            TfInvoke.tfeImportGraphDefOptionsAddInputMapping(
                _ptr, 
                srcName,
                srcIndex,
                dst.Operation,
                dst.Index);
        }

        /// <summary>
        /// Set any imported nodes with control input <paramref name="srcName"/> to have that input
        /// replaced with <paramref name="dst"/>
        /// </summary>
        /// <param name="srcName">Refers to a node in the graph to be imported</param>
        /// <param name="dst">References an operation already existing in the graph being imported into</param>
        public void RemapControlDependency(String srcName, Operation dst)
        {
            TfInvoke.tfeImportGraphDefOptionsRemapControlDependency(_ptr, srcName, dst);
        }

        /// <summary>
        /// Cause the imported graph to have a control dependency on <paramref name="oper"/>
        /// </summary>
        /// <param name="oper">The opration that the graph will have a control dependecy on. Should exist in the graph being imported into.</param>
        public void AddControlDependency(Operation oper)
        {
            TfInvoke.tfeImportGraphDefOptionsAddControlDependency(_ptr, oper);
        }

        /// <summary>
        /// Add an output in graph_def to be returned via the `return_outputs` output
        /// parameter. If the output is remapped via an input
        /// mapping, the corresponding existing tensor in graph will be returned.
        /// </summary>
        /// <param name="operName">The name of the operation</param>
        /// <param name="index">The index</param>
        public void AddReturnOutput(String operName, int index)
        {
            TfInvoke.tfeImportGraphDefOptionsAddReturnOutput(_ptr, operName, index);
        }

        /// <summary>
        /// Get the number of return outputs
        /// </summary>
        public int NumReturnOutputs
        {
            get { return TfInvoke.tfeImportGraphDefOptionsNumReturnOutputs(_ptr); }
        }
    }

    public static partial class TfInvoke
    {

        [DllImport(ExternLibrary, CallingConvention = TfInvoke.TFCallingConvention)]
        internal static extern IntPtr tfeNewImportGraphDefOptions();

        [DllImport(ExternLibrary, CallingConvention = TfInvoke.TFCallingConvention)]
        internal static extern void tfeDeleteImportGraphDefOptions(ref IntPtr options);

        [DllImport(ExternLibrary, CallingConvention = TfInvoke.TFCallingConvention)]
        internal static extern void tfeImportGraphDefOptionsSetPrefix(
            IntPtr opts,
            [MarshalAs(StringMarshalType)]
            String prefix);

        [DllImport(ExternLibrary, CallingConvention = TfInvoke.TFCallingConvention)]
        internal static extern void tfeImportGraphDefOptionsSetDefaultDevice(
            IntPtr opts,
            [MarshalAs(StringMarshalType)]
            String device);

        [DllImport(ExternLibrary, CallingConvention = TfInvoke.TFCallingConvention)]
        internal static extern void tfeImportGraphDefOptionsAddInputMapping(
            IntPtr opts,
            [MarshalAs(StringMarshalType)]
            String srcName,
            int srcIndex,
            IntPtr dstOp,
            int dstOpIdx);

        [DllImport(ExternLibrary, CallingConvention = TfInvoke.TFCallingConvention)]
        internal static extern void tfeImportGraphDefOptionsRemapControlDependency(
            IntPtr opts,
            [MarshalAs(StringMarshalType)]
            String srcName,
            IntPtr dst);

        [DllImport(ExternLibrary, CallingConvention = TfInvoke.TFCallingConvention)]
        internal static extern void tfeImportGraphDefOptionsAddControlDependency(
            IntPtr opts, IntPtr oper);

        [DllImport(ExternLibrary, CallingConvention = TfInvoke.TFCallingConvention)]
        internal static extern void tfeImportGraphDefOptionsAddReturnOutput(
            IntPtr opts,
            [MarshalAs(StringMarshalType)]
            String operName, 
            int index);

        [DllImport(ExternLibrary, CallingConvention = TfInvoke.TFCallingConvention)]
        internal static extern int tfeImportGraphDefOptionsNumReturnOutputs(IntPtr opts);
    }
}
