//----------------------------------------------------------------------------
//  Copyright (C) 2004-2020 by EMGU Corporation. All rights reserved.       
//----------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
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
        public static bool CheckLibraryLoaded()
        {
            return _libraryLoaded;
        }

        /// <summary>
        /// The Tensorflow native api calling convention
        /// </summary>
        public const CallingConvention TFCallingConvention = CallingConvention.Cdecl;

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

            if (loadDirectory == null)
            {
                String subfolder = String.Empty;
                if (System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(System.Runtime.InteropServices.OSPlatform.Windows)
                //|| System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(System.Runtime.InteropServices.OSPlatform.Linux)
			    )
                {
                    if (System.Runtime.InteropServices.RuntimeInformation.OSArchitecture == Architecture.X64)
                        subfolder = "x64";
                    else if (System.Runtime.InteropServices.RuntimeInformation.OSArchitecture == Architecture.X86)
                        subfolder = "x86";
                    else if (System.Runtime.InteropServices.RuntimeInformation.OSArchitecture == Architecture.Arm)
                        subfolder = "arm";
                    else if (System.Runtime.InteropServices.RuntimeInformation.OSArchitecture == Architecture.Arm64)
                        subfolder = "arm64";

                    if ("x86".Equals(subfolder) && System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(System.Runtime.InteropServices.OSPlatform.Windows))
                    {
                        throw new Exception("Emgu TF is only compatible with 64bit mode in Windows (not compatible with 32bit x86 mode)");
                    }
                }

                System.Reflection.Assembly asm = typeof(TfInvoke).Assembly; //System.Reflection.Assembly.GetExecutingAssembly();
                if ((String.IsNullOrEmpty(asm.Location) || !System.IO.File.Exists(asm.Location)) && AppDomain.CurrentDomain.BaseDirectory != null)
                {
                    //if may be running in a debugger visualizer under a unit test in this case
                    String visualStudioDir = AppDomain.CurrentDomain.BaseDirectory;
                    DirectoryInfo visualStudioDirInfo = new DirectoryInfo(visualStudioDir);
                    String debuggerVisualzerPath =
                        Path.Combine(Path.Combine(Path.Combine(
                            visualStudioDirInfo.Parent.FullName, "Packages"), "Debugger"), "Visualizers");

                    if (Directory.Exists(debuggerVisualzerPath))
                        loadDirectory = debuggerVisualzerPath;
                    else
                        loadDirectory = String.Empty;
                }
                else
                {
                    loadDirectory = Path.GetDirectoryName(asm.Location);
                }

                if (!String.IsNullOrEmpty(subfolder))
                    loadDirectory = Path.Combine(loadDirectory, subfolder);

                System.Reflection.Assembly monoAndroidAssembly = Emgu.TF.Util.Toolbox.FindAssembly("Mono.Android.dll");
                if (monoAndroidAssembly == null)
                {
                    //Not running on Android
                    if (!Directory.Exists(loadDirectory))
                    {
                        //try to find an alternative loadDirectory path
                        //The following code should handle finding the asp.NET BIN folder 
                        String altLoadDirectory = Path.GetDirectoryName(asm.CodeBase);
                        if (!String.IsNullOrEmpty(altLoadDirectory) && altLoadDirectory.StartsWith(@"file:\"))
                            altLoadDirectory = altLoadDirectory.Substring(6);

                        if (!String.IsNullOrEmpty(subfolder))
                            altLoadDirectory = Path.Combine(altLoadDirectory, subfolder);

                        if (!Directory.Exists(altLoadDirectory))
                        {
                            FileInfo file = new FileInfo(asm.Location);
                            DirectoryInfo directory = file.Directory;
                            
                            Trace.WriteLine("No suitable directory found to load unmanaged modules");
                            return false;
                        }
                        else
                            loadDirectory = altLoadDirectory;
                    }

                }
            }

            String oldDir = Environment.CurrentDirectory;
            if (!String.IsNullOrEmpty(loadDirectory) && Directory.Exists(loadDirectory))
            {
                Environment.CurrentDirectory = loadDirectory;
                if (System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(System.Runtime.InteropServices.OSPlatform.Windows))
                {
                    bool setDllDirectorySuccess = Emgu.TF.Util.Toolbox.SetDllDirectory(loadDirectory);
                }
            }

            System.Diagnostics.Trace.WriteLine(String.Format("Loading tensorflow binary from {0}", loadDirectory));

            bool success = true;
            string prefix = string.Empty;
            foreach (String module in unmanagedModules)
            {
                string mName = module;

                String fullPath = Path.Combine(prefix, mName);

                //Use absolute path for Windows Desktop
                fullPath = Path.Combine(loadDirectory, fullPath);

                bool fileExist = File.Exists(fullPath);
                if (!fileExist)
                    System.Diagnostics.Trace.WriteLine(String.Format("File {0} do not exist.", fullPath));
                bool fileExistAndLoaded = fileExist && !IntPtr.Zero.Equals(Toolbox.LoadLibrary(fullPath));
                if (fileExist && (!fileExistAndLoaded))
                    System.Diagnostics.Trace.WriteLine(String.Format("File {0} cannot be loaded.", fullPath));
                success &= fileExistAndLoaded;
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
            }
            else
            {
                System.Diagnostics.Trace.WriteLine(String.Format("Failed to load tensorflow binary from {0}", loadDirectory));
            }

            Environment.CurrentDirectory = oldDir;

            return success;
        }

        /// <summary>
        /// Attempts to load tensorflow modules from the specific location
        /// </summary>
        /// <param name="modules">The names of tensorflow modules.</param>
        /// <returns>True if all the modules has been loaded successfully</returns>
        public static bool DefaultLoadUnmanagedModules(String[] modules)
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
                                Trace.WriteLine(string.Format("Trying to load {0} ({1} bit).", module,
                                    IntPtr.Size * 8));
                                loadLibraryMethodInfo.Invoke(null, new object[] { module });
                                //Java.Lang.JavaSystem.LoadLibrary(module);
                                Trace.WriteLine(string.Format("Loaded {0}.", module));
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
                String formatString = GetModuleFormatString();
                for (int i = 0; i < modules.Length; ++i)
                    modules[i] = String.Format(formatString, modules[i]);

                libraryLoaded &= LoadUnmanagedModules(null, modules);
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
        }

        /// <summary>
        /// Get the tensor flow version
        /// </summary>
        public static String Version
        {
            get
            {
                IntPtr ptr = tfeGetVersion();
                return Marshal.PtrToStringAnsi(ptr);
            }
        }

        [DllImport(ExternLibrary, CallingConvention = TfInvoke.TFCallingConvention)]
        private static extern IntPtr tfeGetVersion();

        /// <summary>
        /// Get the size of the datatype in bytes.
        /// </summary>
        /// <param name="dt"></param>
        /// <returns></returns>
        [DllImport(ExternLibrary, CallingConvention = TfInvoke.TFCallingConvention, EntryPoint = "tfeDataTypeSize")]
        public static extern int DataTypeSize(DataType dt);

        /// <summary>
        /// Get the proto buffer that contains the list of all the supported operations.
        /// </summary>
        /// <returns></returns>
        public static Buffer GetAllOpList()
        {
            return new Buffer(tfeGetAllOpList(), true);
        }
        [DllImport(ExternLibrary, CallingConvention = TfInvoke.TFCallingConvention)]
        private static extern IntPtr tfeGetAllOpList();

        [DllImport(ExternLibrary, CallingConvention = TfInvoke.TFCallingConvention)]
        internal static extern int tfeStringEncodedSize(int len);

        [DllImport(ExternLibrary, CallingConvention = TfInvoke.TFCallingConvention)]
        internal static extern int tfeStringEncode(IntPtr src, int srcLen, IntPtr dst, int dstLen, IntPtr status);

        [DllImport(ExternLibrary, CallingConvention = TfInvoke.TFCallingConvention)]
        internal static extern int tfeStringDecode(IntPtr src, int srcLen, ref IntPtr dst, ref IntPtr dstLen, IntPtr status);

        [DllImport(ExternLibrary, CallingConvention = TfInvoke.TFCallingConvention)]
        internal static extern void tfeMemcpy(IntPtr dst, IntPtr src, int length);

        /// <summary>
        /// Returns true if GOOGLE_CUDA is defined.
        /// </summary>
        public static bool IsGoogleCudaEnabled
        {
            get { return tfeIsGoogleCudaEnabled(); }
        }
        [DllImport(ExternLibrary, CallingConvention = TfInvoke.TFCallingConvention)]
        [return: MarshalAs(TfInvoke.BoolMarshalType)]
        private static extern bool tfeIsGoogleCudaEnabled();

        /// <summary>
        /// Returns true if the operation is registered.
        /// </summary>
        public static bool OpIsRegistered(String operationName)
        {
            return tfeOpIsRegistered(operationName);
        }
        [DllImport(ExternLibrary, CallingConvention = TfInvoke.TFCallingConvention)]
        [return: MarshalAs(TfInvoke.BoolMarshalType)]
        private static extern bool tfeOpIsRegistered(
            [MarshalAs(TfInvoke.StringMarshalType)]
            String operationName);

        /// <summary>
        /// Returns true if the operation has a kernel
        /// </summary>
        public static bool OpHasKernel(String operationName)
        {
            return tfeOpHasKernel(operationName);
        }
        [DllImport(ExternLibrary, CallingConvention = TfInvoke.TFCallingConvention)]
        [return: MarshalAs(TfInvoke.BoolMarshalType)]
        private static extern bool tfeOpHasKernel(
            [MarshalAs(TfInvoke.StringMarshalType)]
            String operationName);


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

        [DllImport(ExternLibrary, CallingConvention = TfInvoke.TFCallingConvention)]
        private static extern void tfeListAllPhysicalDevices(IntPtr nameBuffer, IntPtr status);
    }
}
