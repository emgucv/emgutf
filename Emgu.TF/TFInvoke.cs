﻿//----------------------------------------------------------------------------
//  Copyright (C) 2004-2024 by EMGU Corporation. All rights reserved.       
//----------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using Emgu.TF.Util;

namespace Emgu.TF
{
    public static partial class TfInvoke
    {
        private static readonly bool _libraryLoaded;

        /// <summary>
        /// Check to make sure all the unmanaged libraries are loaded
        /// </summary>
        /// <returns>True if library loaded</returns>
        public static bool Init()
        {
            return _libraryLoaded;
        }

        /// <summary>
        /// The Tensorflow native api calling convention
        /// </summary>
        public const CallingConvention TfCallingConvention = CallingConvention.Cdecl;

        /// <summary>
        /// The string marshal type
        /// </summary>
        public const UnmanagedType StringMarshalType = UnmanagedType.LPStr;

        /// <summary>
        /// Represent a bool value in C++
        /// </summary>
        public const UnmanagedType BoolMarshalType = UnmanagedType.U1;

        /// <summary>
        /// Represent a int value in C++
        /// </summary>
        public const UnmanagedType BoolToIntMarshalType = UnmanagedType.Bool;

        private static String FindValidSubfolders(String baseFolder, List<String> subfolderOptions)
        {
            foreach (String sfo in subfolderOptions)
            {
                if (Directory.Exists(Path.Combine(baseFolder, sfo)))
                {
                    return sfo;
                }
            }
            return String.Empty;
        }

#if !(UNITY_EDITOR || UNITY_STANDALONE || UNITY_ANDROID)
        /// <summary>
        /// Attempts to load Tensorflow modules from the specific location
        /// </summary>
        /// <param name="loadDirectory">The directory where the unmanaged modules will be loaded. If it is null, the default location will be used.</param>
        /// <param name="unmanagedModules">The names of Tensorflow modules. </param>
        /// <returns>True if all the modules has been loaded successfully</returns>
        /// <remarks>If <paramref name="loadDirectory"/> is null, the default location on windows is the dll's path appended by either "x64" or "x86", depends on the applications current mode.</remarks>
        public static bool LoadUnmanagedModules(String loadDirectory, params String[] unmanagedModules)
        {
#if UNITY_WSA || UNITY_STANDALONE || UNITY_EDITOR || UNITY_ANDROID || UNITY_IOS || UNITY_WEBGL
            if (loadDirectory != null)
            {
                throw new NotImplementedException("Loading modules from a specific directory is not implemented on the current platform");
            }
            //Let unity handle the library loading
            return true;
#else
            String oldDir = String.Empty;

            if (loadDirectory == null)
            {
                List<String> subfolderOptions = new List<string>();

                if (System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(System.Runtime.InteropServices.OSPlatform.OSX))
                {
                    subfolderOptions.Add(Path.Combine("runtimes", "osx", "native"));
                }
                else if (System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(System.Runtime.InteropServices.OSPlatform.Linux))
                {
                    switch (RuntimeInformation.ProcessArchitecture)
                    {
                        case Architecture.X86:
                            subfolderOptions.Add(Path.Combine("runtimes", "linux-x86", "native"));
                            subfolderOptions.Add(Path.Combine("runtimes", "ubuntu-x86", "native"));
                            subfolderOptions.Add("x86");
                            break;
                        case Architecture.X64:
                            subfolderOptions.Add(Path.Combine("runtimes", "linux-x64", "native"));
                            subfolderOptions.Add(Path.Combine("runtimes", "ubuntu-x64", "native"));
                            subfolderOptions.Add("x64");
                            break;
                        case Architecture.Arm:
                            subfolderOptions.Add(Path.Combine("runtimes", "linux-arm", "native"));
                            subfolderOptions.Add(Path.Combine("runtimes", "ubuntu-arm", "native"));
                            subfolderOptions.Add("arm");
                            break;
                        case Architecture.Arm64:
                            subfolderOptions.Add(Path.Combine("runtimes", "linux-arm64", "native"));
                            subfolderOptions.Add(Path.Combine("runtimes", "ubuntu-arm64", "native"));
                            subfolderOptions.Add("arm64");
                            break;
                    }
                }
                else if (System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(System.Runtime.InteropServices.OSPlatform.Windows))
                {
                    String existingDllDirectory = Emgu.TF.Util.Toolbox.GetDllDirectory();
                    if (existingDllDirectory != null)
                        subfolderOptions.Add(existingDllDirectory);

                    switch (RuntimeInformation.ProcessArchitecture)
                    {
                        case Architecture.X86:
                            throw new Exception("Emgu TF is only compatible with 64 bit mode in Windows (not compatible with 32 bit x86 mode)");
                        //subfolderOptions.Add(Path.Combine("runtimes", "win-x86", "native"));
                        //subfolderOptions.Add("x86");
                        //break;
                        case Architecture.X64:
                            subfolderOptions.Add(Path.Combine("runtimes", "win-x64", "native"));
                            subfolderOptions.Add("x64");
                            break;
                        case Architecture.Arm:
                            subfolderOptions.Add(Path.Combine("runtimes", "win-arm", "native"));
                            subfolderOptions.Add("arm");
                            break;
                        case Architecture.Arm64:
                            subfolderOptions.Add(Path.Combine("runtimes", "win-arm64", "native"));
                            subfolderOptions.Add("arm64");
                            break;
                    }
                }

                String subfolder = String.Empty;

                System.Reflection.Assembly asm = typeof(TfInvoke).Assembly; //System.Reflection.Assembly.GetExecutingAssembly();
                if ((String.IsNullOrEmpty(asm.Location) || !File.Exists(asm.Location)))
                {
                    if (String.IsNullOrEmpty(AppDomain.CurrentDomain.BaseDirectory))
                    {
                        loadDirectory = String.Empty;
                    }
                    else
                    {
                        //we may be running in a debugger visualizer under a unit test in this case
                        String baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
                        DirectoryInfo baseDirectoryInfo = new DirectoryInfo(baseDirectory);
                        String debuggerVisualizerPath = String.Empty;
                        if (baseDirectoryInfo.Parent != null)
                            debuggerVisualizerPath = Path.Combine(baseDirectoryInfo.Parent.FullName, "Packages", "Debugger", "Visualizers");

                        if (!debuggerVisualizerPath.Equals(String.Empty) && Directory.Exists(debuggerVisualizerPath))
                            loadDirectory = debuggerVisualizerPath;
                        else
                        {
                            loadDirectory = baseDirectoryInfo.FullName;
                        }
                        subfolder = FindValidSubfolders(loadDirectory, subfolderOptions);
                    }
                }
                else
                {
                    loadDirectory = Path.GetDirectoryName(asm.Location);
                    if (loadDirectory != null)
                    {
                        subfolder = FindValidSubfolders(loadDirectory, subfolderOptions);
                    }
                }

                if (!String.IsNullOrEmpty(subfolder))
                {
                    if (Directory.Exists(Path.Combine(loadDirectory, subfolder)))
                    {
                        loadDirectory = Path.Combine(loadDirectory, subfolder);
                    }
                    else
                    {
                        loadDirectory = Path.Combine(Path.GetFullPath("."), subfolder);
                    }
                }

                System.Reflection.Assembly monoAndroidAssembly = Emgu.TF.Util.Toolbox.FindAssembly("Mono.Android.dll");
                if (monoAndroidAssembly == null)
                {
                    //Not running on Android
                    if (!Directory.Exists(loadDirectory))
                    {
                        //try to find an alternative loadDirectory path
                        //The following code should handle finding the asp.NET BIN folder 
                        if (String.IsNullOrEmpty(asm.Location) || !File.Exists(asm.Location))
                        {
                            System.Diagnostics.Trace.WriteLine(String.Format("asm.Location is invalid: '{0}'", asm.Location));
                        }
                        else
                        {
                            FileInfo file = new FileInfo(asm.Location);
                            DirectoryInfo directory = file.Directory;
                            if ((directory != null) && (!String.IsNullOrEmpty(subfolder)) && Directory.Exists(Path.Combine(directory.FullName, subfolder)))
                            {
                                loadDirectory = Path.Combine(directory.FullName, subfolder);
                            }
                            else if (directory != null && Directory.Exists(directory.FullName))
                            {
                                loadDirectory = directory.FullName;
                            }
                        }
                    }

                }
            }

            bool setDllDirectorySuccess = false;
            if (!String.IsNullOrEmpty(loadDirectory) && Directory.Exists(loadDirectory))
            {
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    setDllDirectorySuccess = Emgu.TF.Util.Toolbox.SetDllDirectory(loadDirectory);
                    if (!setDllDirectorySuccess)
                    {
                        System.Diagnostics.Trace.WriteLine(String.Format("Failed to set dll directory: {0}", loadDirectory));
                    }
                }
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                {

                }
                else if (Emgu.TF.Util.Toolbox.FindAssembly("Xamarin.iOS.dll") != null)
                {
                    //do nothing
                    System.Diagnostics.Trace.WriteLine("iOS required static linking, setting load directory is not supported");
                }
                else
                {
                    oldDir = Environment.CurrentDirectory;
                    Environment.CurrentDirectory = loadDirectory;
                }
            }

            if (setDllDirectorySuccess)
            {
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    System.Diagnostics.Trace.WriteLine(
                        String.Format(
                            "Loading TF binary from default locations. Current directory: {0}; Additional load folder: {1}",
                            Environment.CurrentDirectory,
                            loadDirectory));
                }

            }
            else
            {
                System.Diagnostics.Trace.WriteLine(
                    String.Format(
                        "Loading TF binary from default locations. Current directory: {0}",
                        Environment.CurrentDirectory));
            }

            System.Diagnostics.Trace.WriteLine(String.Format("Loading tensorflow binary from {0}", loadDirectory));

            bool success = true;
            string prefix = string.Empty;
            String formatString = GetModuleFormatString();
            foreach (String module in unmanagedModules)
            {
                string mName = String.Format(formatString, module);

                String fullPath = Path.Combine(prefix, mName);

                //Use absolute path for Windows Desktop
                fullPath = Path.Combine(loadDirectory, fullPath);

                bool fileExist = File.Exists(fullPath);
                if (!fileExist)
                    System.Diagnostics.Trace.WriteLine(String.Format("File {0} do not exist.", fullPath));

                bool optionalComponent = false;

                bool loaded = false;

                if (fileExist)
                {
                    //Try to load using the full path
                    System.Diagnostics.Trace.WriteLine(String.Format("Found full path {0} for {1}. Trying to load it.", fullPath, mName));
                    loaded = !IntPtr.Zero.Equals(Toolbox.LoadLibrary(fullPath));
                    if (loaded)
                        System.Diagnostics.Trace.WriteLine(String.Format("{0} loaded.", mName));
                    else
                        System.Diagnostics.Trace.WriteLine(String.Format("Failed to load {0} from {1}.", mName, fullPath));
                }
                if (!loaded)
                {
                    //Try to load without the full path
                    System.Diagnostics.Trace.WriteLine(String.Format("Trying to load {0} using default path.", mName));
                    loaded = !IntPtr.Zero.Equals(Toolbox.LoadLibrary(mName));
                    if (loaded)
                        System.Diagnostics.Trace.WriteLine(String.Format("{0} loaded using default path", mName));
                    else
                        System.Diagnostics.Trace.WriteLine(String.Format("Failed to load {0} using default path", mName));
                }

                if (!loaded)
                    System.Diagnostics.Trace.WriteLine(String.Format("!!! Failed to load {0}.", mName));

                if (!optionalComponent)
                    success &= loaded;

            }

            if (success)
            {
                bool IsGoogleCudaEnabled = TfInvoke.IsGoogleCudaEnabled;
                String version = Emgu.TF.TfInvoke.Version;
                String[] devices = ListAllPhysicalDevices();
                System.Diagnostics.Trace.WriteLine(String.Format("Successfully loaded tensorflow {0} binary from {1}; IsGoogleCudaEnabled = {2}; PhysicalDevices=[{3}]",
                    version,
                    loadDirectory,
                    IsGoogleCudaEnabled,
                    String.Join(",", devices)));

                //AddLogListenerSink();
                RegisterLogListener(_defaultLogListener);
                TfInvoke.LogMsgReceived += TfInvoke_LogMsgReceived;
            }
            else
            {
                System.Diagnostics.Trace.WriteLine(String.Format("Failed to load tensorflow binary from {0}", loadDirectory));
            }

            if (!oldDir.Equals(String.Empty))
            {
                Environment.CurrentDirectory = oldDir;
            }

            return success;
#endif
        }

        private static void TfInvoke_LogMsgReceived(object sender, string e)
        {
            Trace.WriteLine(String.Format("TF LOG: {0}", e));
        }

        /// <summary>
        /// Attempts to load tensorflow modules from the specific location
        /// </summary>
        /// <param name="modules">The names of tensorflow modules.</param>
        /// <param name="loadDirectory">The path to load the opencv modules. If null, will use the default path.</param>
        /// <returns>True if all the modules has been loaded successfully</returns>
        public static bool DefaultLoadUnmanagedModules(String[] modules, String loadDirectory = null)
        {
            bool libraryLoaded = true;

            System.Reflection.Assembly monoAndroidAssembly = Emgu.TF.Util.Toolbox.FindAssembly("Mono.Android.dll");
            if (monoAndroidAssembly != null)
            {
                //Running on Android
                Type javaSystemType = monoAndroidAssembly.GetType("Java.Lang.JavaSystem");
                if (javaSystemType != null)
                {
                    System.Reflection.MethodInfo loadLibraryMethodInfo = javaSystemType.GetMethod("LoadLibrary");
                    if (loadLibraryMethodInfo != null)
                    {
                        foreach (String module in modules)
                        {
                            try
                            {
                                String fullModulePath;
                                if (loadDirectory != null)
                                {
                                    fullModulePath = Path.Combine(loadDirectory, module);
                                }
                                else
                                {
                                    fullModulePath = module;
                                }
                                Trace.WriteLine(string.Format("Trying to load {0} ({1} bit).", fullModulePath,
                                    IntPtr.Size * 8));
                                loadLibraryMethodInfo.Invoke(null, new object[] { fullModulePath });
                                //Java.Lang.JavaSystem.LoadLibrary(module);
                                Trace.WriteLine(string.Format("Loaded {0}.", fullModulePath));
                            }
                            catch (Exception e)
                            {
                                libraryLoaded = false;
                                Trace.WriteLine(String.Format("Failed to load {0}: {1}", module, e.Message));
                            }
                        }
                        return libraryLoaded;
                    }
                }
            }

            if (!System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(System.Runtime.InteropServices
                .OSPlatform.OSX))
            {
                //String formatString = GetModuleFormatString();
                //for (int i = 0; i < modules.Length; ++i)
                //    modules[i] = String.Format(formatString, modules[i]);

                libraryLoaded &= LoadUnmanagedModules(loadDirectory, modules);
            }
            return libraryLoaded;
        }

        /// <summary>
        /// Get the module format string.
        /// </summary>
        /// <returns>On Windows, "{0}".dll will be returned; On Linux, "lib{0}.so" will be returned; Otherwise {0} is returned.</returns>
        public static String GetModuleFormatString()
        {
            String formatString = "{0}";
            if (System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(System.Runtime.InteropServices.OSPlatform.Windows))
                formatString = "{0}.dll";
            else if (System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(System.Runtime.InteropServices.OSPlatform.Linux))
                formatString = "lib{0}.so";
            else if (System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(System.Runtime.InteropServices.OSPlatform.OSX))
                formatString = "lib{0}.dylib";
            return formatString;
        }
#endif

        /// <summary>
        /// Static Constructor to setup tensorflow environment
        /// </summary>
        static TfInvoke()
        {
            Trace.WriteLine(String.Format("Emgu TF Running in {0} bit mode.", IntPtr.Size * 8));
            List<String> modules = TfInvoke.TensorflowModuleList;
            modules.RemoveAll(String.IsNullOrEmpty);

#if (UNITY_EDITOR || UNITY_STANDALONE || UNITY_ANDROID)
            _libraryLoaded = true;
#else
            _libraryLoaded = DefaultLoadUnmanagedModules(modules.ToArray());
#endif

            if (!_libraryLoaded)
            {
                Trace.WriteLine("Failed to load native binary. Please make sure a proper Emgu.TF.runtime.{platform} nuget package is added, or make sure the native binary can be found in the folder of executable.");
            }

            try
            {
                String version = Version;
            }
            catch (DllNotFoundException e)
            {
                String errMsg =
                    "Unable to load native binary. Please make sure a proper Emgu.TF.runtime.{platform} nuget package is added, or make sure the native binary can be found in the folder of the executable.";
                Trace.WriteLine(errMsg);
                throw new DllNotFoundException(errMsg, e);
            }

            _defaultLogForwarderSink = new Emgu.TF.LogForwarderSink(true);
        }

        /// <summary>
        /// Get the tensorflow version
        /// </summary>
        public static String Version
        {
            get
            {
                IntPtr ptr = tfeGetVersion();
                return Marshal.PtrToStringAnsi(ptr);
            }
        }

        [DllImport(ExternLibrary, CallingConvention = TfInvoke.TfCallingConvention)]
        private static extern IntPtr tfeGetVersion();

        /// <summary>
        /// Get the size of the datatype in bytes.
        /// </summary>
        /// <param name="dt">The data type</param>
        /// <returns>The size of the data type in bytes</returns>
        [DllImport(ExternLibrary, CallingConvention = TfInvoke.TfCallingConvention, EntryPoint = "tfeDataTypeSize")]
        public static extern int DataTypeSize(DataType dt);

        /// <summary>
        /// Retrieves a protocol buffer that contains the list of all supported TensorFlow operations.
        /// </summary>
        /// <returns>
        /// A <see cref="Buffer"/> object containing the serialized protocol buffer with the list of supported operations.
        /// </returns>
        /// <remarks>
        /// The returned buffer holds a pointer to the serialized data and its associated length.
        /// It is the caller's responsibility to manage the lifecycle of the buffer appropriately.
        /// </remarks>
        public static Buffer GetAllOpList()
        {
            return new Buffer(tfeGetAllOpList(), true);
        }
        [DllImport(ExternLibrary, CallingConvention = TfInvoke.TfCallingConvention)]
        private static extern IntPtr tfeGetAllOpList();

        /*
        [DllImport(ExternLibrary, CallingConvention = TfInvoke.TFCallingConvention)]
        internal static extern int tfeStringEncodedSize(int len);

        [DllImport(ExternLibrary, CallingConvention = TfInvoke.TFCallingConvention)]
        internal static extern int tfeStringEncode(IntPtr src, int srcLen, IntPtr dst, int dstLen, IntPtr status);

        [DllImport(ExternLibrary, CallingConvention = TfInvoke.TFCallingConvention)]
        internal static extern int tfeStringDecode(IntPtr src, int srcLen, ref IntPtr dst, ref IntPtr dstLen, IntPtr status);
        */

        /// <summary>
        /// Native implementation of the memory copy function
        /// </summary>
        /// <param name="dst">The destination pointer</param>
        /// <param name="src">The source pointer</param>
        /// <param name="length">The number of bytes to copy</param>
        public static void Memcpy(IntPtr dst, IntPtr src, int length)
        {
            tfeMemcpy(dst, src, length);
        }

        [DllImport(ExternLibrary, CallingConvention = TfInvoke.TfCallingConvention)]
        internal static extern void tfeMemcpy(IntPtr dst, IntPtr src, int length);

        /// <summary>
        /// Returns true if CUDA is enabled.
        /// </summary>
        public static bool IsGoogleCudaEnabled
        {
            get { return tfeIsGoogleCudaEnabled(); }
        }
        [DllImport(ExternLibrary, CallingConvention = TfInvoke.TfCallingConvention)]
        [return: MarshalAs(TfInvoke.BoolMarshalType)]
        private static extern bool tfeIsGoogleCudaEnabled();

        /// <summary>
        /// Returns true if Tensorflow is built with ROCm
        /// </summary>
        public static bool IsBuiltWithROCm
        {
            get { return tfeIsBuiltWithROCm(); }
        }
        [DllImport(ExternLibrary, CallingConvention = TfInvoke.TfCallingConvention)]
        [return: MarshalAs(TfInvoke.BoolMarshalType)]
        private static extern bool tfeIsBuiltWithROCm();

        /// <summary>
        /// Returns true if Tensorflow is built with Nvcc
        /// </summary>
        public static bool IsBuiltWithNvcc
        {
            get { return tfeIsBuiltWithNvcc(); }
        }
        [DllImport(ExternLibrary, CallingConvention = TfInvoke.TfCallingConvention)]
        [return: MarshalAs(TfInvoke.BoolMarshalType)]
        private static extern bool tfeIsBuiltWithNvcc();

        /// <summary>
        /// Returns true if Gpu supports HalfMatMulAndConv
        /// </summary>
        public static bool GpuSupportsHalfMatMulAndConv
        {
            get { return tfeGpuSupportsHalfMatMulAndConv(); }
        }
        [DllImport(ExternLibrary, CallingConvention = TfInvoke.TfCallingConvention)]
        [return: MarshalAs(TfInvoke.BoolMarshalType)]
        private static extern bool tfeGpuSupportsHalfMatMulAndConv();

        /// <summary>
        /// Returns true if MKL is enabled
        /// </summary>
        public static bool IsMklEnabled
        {
            get { return tfeIsMklEnabled(); }
        }
        [DllImport(ExternLibrary, CallingConvention = TfInvoke.TfCallingConvention)]
        [return: MarshalAs(TfInvoke.BoolMarshalType)]
        private static extern bool tfeIsMklEnabled();

        /// <summary>
        /// Returns true if the operation is registered.
        /// </summary>
        /// <param name="operationName">The name of the operation</param>
        /// <returns>True if the operation is registered</returns>
        public static bool OpIsRegistered(String operationName)
        {
            return tfeOpIsRegistered(operationName);
        }
        [DllImport(ExternLibrary, CallingConvention = TfInvoke.TfCallingConvention)]
        [return: MarshalAs(TfInvoke.BoolMarshalType)]
        private static extern bool tfeOpIsRegistered(
            [MarshalAs(TfInvoke.StringMarshalType)]
            String operationName);

        /// <summary>
        /// Returns true if the operation has a kernel
        /// </summary>
        /// <param name="operationName">The name of the operation</param>
        /// <returns>True if the operation has a kernel</returns>
        public static bool OpHasKernel(String operationName)
        {
            return tfeOpHasKernel(operationName);
        }
        [DllImport(ExternLibrary, CallingConvention = TfInvoke.TfCallingConvention)]
        [return: MarshalAs(TfInvoke.BoolMarshalType)]
        private static extern bool tfeOpHasKernel(
            [MarshalAs(TfInvoke.StringMarshalType)]
            String operationName);


        /// <summary>
        /// Get a list of all physical devices
        /// </summary>
        /// <param name="status">Optional status</param>
        /// <returns>A list of all physical devices</returns>
        public static String[] ListAllPhysicalDevices(Status status = null)
        {
            using (StatusChecker checker = new StatusChecker(status))
            {
                byte[] nameBuffer = new byte[2048];

                GCHandle nameHandle = GCHandle.Alloc(nameBuffer, GCHandleType.Pinned);

                TfInvoke.tfeListAllPhysicalDevices(
                    nameHandle.AddrOfPinnedObject(),
                    checker.Status);

                nameHandle.Free();
                String nameResult = System.Text.Encoding.ASCII.GetString(nameBuffer);
                String[] names = nameResult.TrimEnd('\0', '\n').Split('\n');
                return names;

            }
        }

        [DllImport(ExternLibrary, CallingConvention = TfInvoke.TfCallingConvention)]
        private static extern void tfeListAllPhysicalDevices(IntPtr nameBuffer, IntPtr status);

        /// <summary>
        /// A Log Listener call back
        /// </summary>
        /// <param name="msg">The pointer to the native message.</param>
        [UnmanagedFunctionPointer(TfCallingConvention)]
        public delegate void TfLogListener(IntPtr msg);

        /// <summary>
        /// Register a Log Listener
        /// </summary>
        /// <param name="listener">The Log listener to be registered.</param>
        [DllImport(
            ExternLibrary,
            CallingConvention = TfInvoke.TfCallingConvention,
            EntryPoint = "tfeRegisterLogListener")]
        public static extern void RegisterLogListener(TfLogListener listener);


        [DllImport(
            ExternLibrary,
            CallingConvention = TfInvoke.TfCallingConvention)]
        private static extern void tfeAddLogSink(IntPtr sink);

        /// <summary>
        /// Add a log sink to receive log messages (tensorflow::TFAddLogSink)
        /// </summary>
        /// <param name="sink">The log sink to be added</param>
        public static void AddLogSink(ILogSink sink)
        {
            tfeAddLogSink(sink.LogSinkPtr);
        }

        [DllImport(
            ExternLibrary,
            CallingConvention = TfInvoke.TfCallingConvention)]
        private static extern void tfeRemoveLogSink(IntPtr sink);

        /// <summary>
        /// Remove a log sink from receiving log messages (tensorflow::TFRemoveLogSink)
        /// </summary>
        /// <param name="sink">The log sink to be removed</param>
        public static void RemoveLogSink(ILogSink sink)
        {
            tfeRemoveLogSink(sink.LogSinkPtr);
        }

        private static TfLogListener _defaultLogListener = TfDefaultLogListener;


        /// <summary>
        /// Handle the event when Log Message is received
        /// </summary>
        public static event EventHandler<String> LogMsgReceived;

        /// <summary>
        /// Handles log messages received from the native library.
        /// </summary>
        /// <param name="msgPtr">Pointer to the log message string.</param>
#if UNITY_WSA || UNITY_ANDROID || UNITY_STANDALONE
        [AOT.MonoPInvokeCallback(typeof(TfLogListener))]
#endif
        public static void TfDefaultLogListener(IntPtr msgPtr)
        {
            //String msg = System.Runtime.InteropServices.Marshal.PtrToStringAuto(msgPtr);
            String msg = System.Runtime.InteropServices.Marshal.PtrToStringAnsi(msgPtr);
            LogMsgReceived?.Invoke(null, msg);
        }


        private static ILogSink _defaultLogForwarderSink = null;

        /// <summary>
        /// Get the default log forwarder sink
        /// </summary>
        public static ILogSink DefaultLogForwarderSink
        {
            get
            {
                return _defaultLogForwarderSink;
            }

        }
    }


}
