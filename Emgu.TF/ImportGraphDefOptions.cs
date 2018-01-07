//----------------------------------------------------------------------------
//  Copyright (C) 2004-2018 by EMGU Corporation. All rights reserved.       
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

        public void AddInputMapping(String srcName, int srcIndex, Output dst)
        {
            TfInvoke.tfeImportGraphDefOptionsAddInputMapping(
                _ptr, 
                srcName,
                srcIndex,
                dst.Operation,
                dst.Index);
        }

        public void RemapControlDependency(String srcName, Operation dst)
        {
            TfInvoke.tfeImportGraphDefOptionsRemapControlDependency(_ptr, srcName, dst);
        }

        public void AddControlDependency(Operation oper)
        {
            TfInvoke.tfeImportGraphDefOptionsAddControlDependency(_ptr, oper);
        }

        public void AddReturnOutput(String operName, int index)
        {
            TfInvoke.tfeImportGraphDefOptionsAddReturnOutput(_ptr, operName, index);
        }

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
