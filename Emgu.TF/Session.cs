//----------------------------------------------------------------------------
//  Copyright (C) 2004-2019 by EMGU Corporation. All rights reserved.       
//----------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Text;
using Emgu.TF.Util;
using System.Runtime.InteropServices;

namespace Emgu.TF
{
    /// <summary>
    /// API for driving Graph execution
    /// </summary>
    public class Session : UnmanagedObject
    {

        /// <summary>
        /// Return a new execution session with the associated graph.
        /// </summary>
        /// <param name="graph">Graph must be a valid graph (not deleted or null).  This function will
        /// prevent the graph from being deleted until Session is deleted.
        /// Does not take ownership of opts.
        /// </param>
        /// <param name="sessionOptions">The session options</param>
        /// <param name="status">The status</param>
        public Session(Graph graph, SessionOptions sessionOptions = null, Status status = null)
        {
            using (StatusChecker checker = new StatusChecker(status))
                _ptr = TfInvoke.tfeNewSession(graph, sessionOptions, checker.Status);
        }

        /// <summary>
        /// Close a session.
        /// Contacts any other processes associated with the session, if applicable.
        /// </summary>
        /// <param name="status">The status</param>
        public void Close(Status status = null)
        {
            using (StatusChecker checker = new StatusChecker(status))
                TfInvoke.tfeCloseSession(_ptr, checker.Status);
        }

        /// <summary>
        /// Release the unmanaged memory associated with this Session.
        /// </summary>
        protected override void DisposeObject()
        {
            if (IntPtr.Zero != _ptr)
            {
                using (StatusChecker checker = new StatusChecker(null))
                    TfInvoke.tfeDeleteSession(ref _ptr, checker.Status);
            }
        }

        /// <summary>
        /// Run the graph associated with the session starting with the supplied inputs
        /// (inputs[0,ninputs-1] with corresponding values in input_values[0,ninputs-1]).
        /// </summary>
        /// <param name="inputs">The input nodes</param>
        /// <param name="inputValues">The input values</param>
        /// <param name="outputs">The output nodes</param>
        /// <param name="targetOperations">Optional target operations</param>
        /// <param name="runOptions"></param>
        /// May be NULL, in which case it will be ignored; or
        /// non-NULL, in which case it must point to a `TF_Buffer` containing the
        /// serialized representation of a `RunOptions` protocol buffer.
        /// <param name="runMetadata">
        /// May be NULL, in which case it will be ignored; or
        /// non-NULL, in which case it must point to an empty, freshly allocated
        /// `TF_Buffer` that may be updated to contain the serialized representation
        /// of a `RunMetadata` protocol buffer.
        /// </param>
        /// <param name="status">The status</param>
        /// <returns>On success, the tensors corresponding to outputs[0,noutputs-1] are placed in the returned Tensors.</returns>
        public Tensor[] Run(Output[] inputs, Tensor[] inputValues, Output[] outputs, Operation[] targetOperations = null, Buffer runOptions = null, Buffer runMetadata = null, Status status = null)
        {

            IntPtr[] inputOps = Array.ConvertAll(inputs, i => i.Operation.Ptr);
            int[] inputIdx = Array.ConvertAll(inputs, i => i.Index);
            IntPtr[] inputTensors = Array.ConvertAll(inputValues, i => i.Ptr);

            GCHandle inputOpsHandle = GCHandle.Alloc(inputOps, GCHandleType.Pinned);
            GCHandle inputIdxHandle = GCHandle.Alloc(inputIdx, GCHandleType.Pinned);
            GCHandle inputTensorsHandle = GCHandle.Alloc(inputTensors, GCHandleType.Pinned);

            IntPtr[] outputOps = Array.ConvertAll(outputs, o => o.Operation.Ptr);
            int[] outputIdx = Array.ConvertAll(outputs, o => o.Index);
            IntPtr[] outputTensors = new IntPtr[outputs.Length];
            GCHandle outputOpsHandle = GCHandle.Alloc(outputOps, GCHandleType.Pinned);
            GCHandle outputIdxHandle = GCHandle.Alloc(outputIdx, GCHandleType.Pinned);
            GCHandle outputTensorsHandle = GCHandle.Alloc(outputTensors, GCHandleType.Pinned);

            IntPtr targetOpsPtr = IntPtr.Zero;
            int ntargets = 0;
            IntPtr[] targetOpsPtrArray = null;
            GCHandle targetOpsHandle = new GCHandle();
            if (targetOperations != null)
            {
                targetOpsPtrArray = Array.ConvertAll(targetOperations, o => o.Ptr);
                targetOpsHandle = GCHandle.Alloc(targetOpsPtrArray, GCHandleType.Pinned);
                targetOpsPtr = targetOpsHandle.AddrOfPinnedObject();
                ntargets = targetOperations.Length;
            }
            using (StatusChecker checker = new StatusChecker(status))
            {
                TfInvoke.tfeSessionRun(
                    _ptr,
                    runOptions,
                    inputOpsHandle.AddrOfPinnedObject(),
                    inputIdxHandle.AddrOfPinnedObject(),
                    inputTensorsHandle.AddrOfPinnedObject(),
                    inputs.Length,
                    outputOpsHandle.AddrOfPinnedObject(),
                    outputIdxHandle.AddrOfPinnedObject(),
                    outputTensorsHandle.AddrOfPinnedObject(),
                    outputs.Length,
                    targetOpsPtr,
                    ntargets,
                    runMetadata,
                    checker.Status
                );
            }
            inputOpsHandle.Free();
            inputIdxHandle.Free();
            inputTensorsHandle.Free();

            if (targetOperations != null)
            {
                targetOpsHandle.Free();
            }

            outputOpsHandle.Free();
            outputIdxHandle.Free();
            outputTensorsHandle.Free();
            return Array.ConvertAll(outputTensors, t => new Tensor(t));

        }

        /// <summary>
        /// Lists all devices in a session
        /// </summary>
        /// <param name="status">The status</param>
        /// <returns>All devices in the current session</returns>
        public Device[] ListDevices(Status status = null)
        {
            using (StatusChecker checker = new StatusChecker(status))
            {
                byte[] nameBuffer = new byte[2048];
                byte[] typeBuffer = new byte[2048];
                Int64[] memorySizeBuffer = new Int64[128];
                GCHandle nameHandle = GCHandle.Alloc(nameBuffer, GCHandleType.Pinned);
                GCHandle typeHandle = GCHandle.Alloc(typeBuffer, GCHandleType.Pinned);
                GCHandle memorySizeHandle = GCHandle.Alloc(memorySizeBuffer, GCHandleType.Pinned);

                TfInvoke.tfeSessionListDevices(
                    _ptr, 
                    nameHandle.AddrOfPinnedObject(), 
                    typeHandle.AddrOfPinnedObject(),
                    memorySizeHandle.AddrOfPinnedObject(),
                    checker.Status);

                nameHandle.Free();
                typeHandle.Free();
                memorySizeHandle.Free();

                String nameResult = System.Text.Encoding.ASCII.GetString(nameBuffer);
                String[] names = nameResult.TrimEnd('\0', '\n').Split('\n');

                String typeResult = System.Text.Encoding.ASCII.GetString(typeBuffer);
                String[] types = typeResult.TrimEnd('\0', '\n').Split('\n');

                Device[] devices = new Device[names.Length];
                for (int i = 0; i < devices.Length; i++)
                {
                    Device d = new Device();
                    d.Name = names[i];
                    d.Type = types[i];
                    d.MemoryBytes = memorySizeBuffer[i];
                    devices[i] = d;
                }
                return devices;
            }
        }

        /// <summary>
        /// The device for a session.
        /// </summary>
        public class Device
        {
            /// <summary>
            /// The name of the device
            /// </summary>
            public String Name { get; set; }
            /// <summary>
            /// The type of the device
            /// </summary>
            public String Type { get; set; }
            /// <summary>
            /// The amount of memory associated with a given device, in bytes.
            /// </summary>
            public Int64 MemoryBytes { get; set; }

            /// <summary>
            /// The amount of memory associated with a given device, in GB.
            /// </summary>
            public double MemoryGB { get { return MemoryBytes / (1024.0 * 1024.0 * 1024.0); } }
        }
    }


    public static partial class TfInvoke
    {

        [DllImport(ExternLibrary, CallingConvention = TfInvoke.TFCallingConvention)]
        internal static extern IntPtr tfeNewSession(IntPtr graph, IntPtr opts, IntPtr status);

        [DllImport(ExternLibrary, CallingConvention = TfInvoke.TFCallingConvention)]
        internal static extern void tfeDeleteSession(ref IntPtr session, IntPtr status);

        [DllImport(ExternLibrary, CallingConvention = TfInvoke.TFCallingConvention)]
        internal static extern void tfeCloseSession(IntPtr session, IntPtr status);

        [DllImport(ExternLibrary, CallingConvention = TfInvoke.TFCallingConvention)]
        internal static extern void tfeSessionRun(
            IntPtr session, IntPtr runOptions,
            IntPtr inputOps, IntPtr inputIdx, IntPtr inputValues, int ninputs,
            IntPtr outputOps, IntPtr outputIdx, IntPtr outputValues, int noutputs,
            IntPtr targetOpers, int ntargets,
            IntPtr runMetadata, IntPtr status);

        [DllImport(ExternLibrary, CallingConvention = TfInvoke.TFCallingConvention)]
        internal static extern void tfeSessionListDevices(IntPtr session, IntPtr nameBuffer, IntPtr typeBuffer, IntPtr memorySizeBuffer, IntPtr status);
    }
}
