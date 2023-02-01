//----------------------------------------------------------------------------
//  Copyright (C) 2004-2023 by EMGU Corporation. All rights reserved.       
//----------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Runtime.InteropServices;
using Emgu.TF.Util;

namespace Emgu.TF.Lite
{
    public static partial class TfLiteInvoke
    {
#if UNITY_ANDROID || UNITY_IOS || UNITY_EDITOR || UNITY_STANDALONE
        [AOT.MonoPInvokeCallback(typeof(TfLiteErrorCallback))]
#endif
        private static int TfLiteErrorHandler(
            int status,
            IntPtr errMsg)
        {
            String msg = Marshal.PtrToStringAnsi(errMsg);
            throw new Exception(msg);
        }

        /// <summary>
        /// Define the functional interface for the error callback
        /// </summary>
        /// <param name="status">The status code</param>
        /// <param name="errMsg">The pointer to the error message</param>
        /// <returns></returns>
        [UnmanagedFunctionPointer(TfLiteCallingConvention)]
        public delegate int TfLiteErrorCallback(int status, IntPtr errMsg);

        /// <summary>
        /// Redirect tensorflow lite error.
        /// </summary>
        /// <param name="errorHandler">The error handler</param>
        [DllImport(ExternLibrary, CallingConvention = TfLiteCallingConvention, EntryPoint = "tfeRedirectError")]
        public static extern void RedirectError(TfLiteErrorCallback errorHandler);

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
        /// The Tensorflow lite native api calling convention
        /// </summary>
        public const CallingConvention TfLiteCallingConvention = CallingConvention.Cdecl;

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

#if !(UNITY_ANDROID || UNITY_IOS || UNITY_EDITOR || UNITY_STANDALONE)
        /// <summary>
        /// Attempts to load tensorflow modules from the specific location
        /// </summary>
        /// <param name="loadDirectory">The directory where the unmanaged modules will be loaded. If it is null, the default location will be used.</param>
        /// <param name="unmanagedModules">The names of tensorflow modules. </param>
        /// <returns>True if all the modules has been loaded successfully</returns>
        /// <remarks>If <paramref name="loadDirectory"/> is null, the default location on windows is the dll's path appended by either "x64" or "x86", depends on the applications current mode.</remarks>
        public static bool LoadUnmanagedModules(String loadDirectory, params String[] unmanagedModules)
        {
#if UNITY_WSA || UNITY_STANDALONE || UNITY_EDITOR || UNITY_ANDROID || UNITY_IOS
            if (loadDirectory != null)
            {
                throw new NotImplementedException("Loading modules from a specific directory is not implemented in Windows Store App");
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
                            subfolderOptions.Add(Path.Combine("runtimes", "win-x86", "native"));
                            subfolderOptions.Add("x86");
                            break;
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

                System.Reflection.Assembly
                    asm = typeof(TfLiteInvoke).Assembly; //System.Reflection.Assembly.GetExecutingAssembly();
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
                            debuggerVisualizerPath = Path.Combine(baseDirectoryInfo.Parent.FullName, "Packages",
                                "Debugger", "Visualizers");

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
                            Debug.WriteLine(String.Format("asm.Location is invalid: '{0}'", asm.Location));
                        }
                        else
                        {
                            FileInfo file = new FileInfo(asm.Location);
                            DirectoryInfo directory = file.Directory;
                            if ((directory != null) && (!String.IsNullOrEmpty(subfolder)) &&
                                Directory.Exists(Path.Combine(directory.FullName, subfolder)))
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
                        System.Diagnostics.Debug.WriteLine(String.Format("Failed to set dll directory: {0}",
                            loadDirectory));
                    }
                }
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                {

                }
                else if (Emgu.TF.Util.Toolbox.FindAssembly("Xamarin.iOS.dll") != null)
                {
                    //do nothing
                    System.Diagnostics.Debug.WriteLine(
                        "iOS required static linking, setting load directory is not supported");
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
                    System.Diagnostics.Debug.WriteLine(
                        String.Format(
                            "Loading TF Lite binary from default locations. Current directory: {0}; Additional load folder: {1}",
                            Environment.CurrentDirectory,
                            loadDirectory));
                }

            }
            else
            {
                System.Diagnostics.Debug.WriteLine(
                    String.Format(
                        "Loading TF Lite binary from default locations. Current directory: {0}",
                        Environment.CurrentDirectory));
            }

            System.Diagnostics.Trace.WriteLine(String.Format("Loading tensorflow lite binary from {0}", loadDirectory));
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

                /*
                bool loaded;
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    loaded = fileExist && !IntPtr.Zero.Equals(Toolbox.LoadLibrary(fullPath));
                    if (loaded)
                    {
                        System.Diagnostics.Trace.WriteLine(String.Format("File {0} loaded.", fullPath));
                    }
                    else
                    {
                        System.Diagnostics.Trace.WriteLine(String.Format("File {0} cannot be loaded.", fullPath));
                    }
                }
                else
                {
                    loaded = true;
                }

                success &= (fileExist && loaded); */
                bool optionalComponent = false;

                bool loaded = false;

                if (fileExist)
                {
                    //Try to load using the full path
                    System.Diagnostics.Trace.WriteLine(String.Format("Found full path {0} for {1}. Trying to load it.",
                        fullPath, mName));
                    loaded = !IntPtr.Zero.Equals(Emgu.TF.Util.Toolbox.LoadLibrary(fullPath));
                    if (loaded)
                        System.Diagnostics.Trace.WriteLine(String.Format("{0} loaded.", mName));
                    else
                        System.Diagnostics.Trace.WriteLine(String.Format("Failed to load {0} from {1}.", mName,
                            fullPath));
                }

                if (!loaded)
                {
                    //Try to load without the full path
                    System.Diagnostics.Trace.WriteLine(String.Format("Trying to load {0} using default path.", mName));
                    loaded = !IntPtr.Zero.Equals(Emgu.TF.Util.Toolbox.LoadLibrary(mName));
                    if (loaded)
                        System.Diagnostics.Trace.WriteLine(String.Format("{0} loaded using default path", mName));
                    else
                        System.Diagnostics.Trace.WriteLine(
                            String.Format("Failed to load {0} using default path", mName));
                }

                if (!loaded)
                    System.Diagnostics.Trace.WriteLine(String.Format("!!! Failed to load {0}.", mName));

                if (!optionalComponent)
                    success &= loaded;

            }

            if (!oldDir.Equals(String.Empty))
            {
                Environment.CurrentDirectory = oldDir;
            }

            return success;
#endif
        }

        /// <summary>
        /// Get the module format string.
        /// </summary>
        /// <returns>On Windows, "{0}".dll will be returned; On Linux, "lib{0}.so" will be returned; Otherwise {0} is returned.</returns>
        public static String GetModuleFormatString()
        {
            String formatString = "{0}";
            if (System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(System.Runtime.InteropServices.OSPlatform
                .Windows))
                formatString = "{0}.dll";
            else if (System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(System.Runtime.InteropServices
                .OSPlatform.Linux))
                formatString = "lib{0}.so";
            else if (System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(System.Runtime.InteropServices
                .OSPlatform.OSX))
                formatString = "lib{0}.dylib";
            return formatString;
        }

#endif

        /// <summary>
        /// Attempts to load tensorflow modules from the specific location
        /// </summary>
        /// <param name="modules">The names of tensorflow modules.</param>
        /// <param name="loadDirectory">The path to load the opencv modules. If null, will use the default path.</param>
        /// <returns>True if all the modules has been loaded successfully</returns>
        public static bool DefaultLoadUnmanagedModules(String[] modules, String loadDirectory = null)
        {
            bool libraryLoaded = true;
#if !(UNITY_ANDROID || UNITY_IOS || UNITY_EDITOR || UNITY_STANDALONE)
            System.Reflection.Assembly monoAndroidAssembly = Emgu.TF.Util.Toolbox.FindAssembly("Mono.Android.dll");
            if (monoAndroidAssembly != null)
            {
                //Running on Xamarin Android
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
                                System.Diagnostics.Trace.WriteLine(string.Format("Trying to load {0} ({1} bit).",
                                    fullModulePath,
                                    IntPtr.Size * 8));
                                loadLibraryMethodInfo.Invoke(null, new object[] { fullModulePath });
                                //Java.Lang.JavaSystem.LoadLibrary(module);
                                System.Diagnostics.Trace.WriteLine(string.Format("Loaded {0}.", fullModulePath));
                            }
                            catch (Exception e)
                            {
                                libraryLoaded = false;
                                System.Diagnostics.Trace.WriteLine(String.Format("Failed to load {0}: {1}", module,
                                    e.Message));
                            }
                        }

                        return libraryLoaded;
                    }
                }
            }
            else if (!System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(System.Runtime.InteropServices
                .OSPlatform.OSX))
            {
                //String formatString = GetModuleFormatString();
                //for (int i = 0; i < modules.Length; ++i)
                //    modules[i] = String.Format(formatString, modules[i]);

                libraryLoaded &= LoadUnmanagedModules(loadDirectory, modules);
            }
#endif
            return libraryLoaded;
        }

        /// <summary>
        /// Static Constructor to setup tensorflow environment
        /// </summary>
        static TfLiteInvoke()
        {
            System.Diagnostics.Trace.WriteLine(String.Format("Emgu TF Lite Running in {0} bit mode.", IntPtr.Size * 8));
            List<String> modules = TfLiteInvoke.TensorflowModuleList;
            modules.RemoveAll(String.IsNullOrEmpty);

            _libraryLoaded = DefaultLoadUnmanagedModules(modules.ToArray());

            if ((!_libraryLoaded) && RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                Trace.WriteLine(
                    "Failed to load native binary. Please make sure a proper Emgu.TF.Lite.runtime.{platform} nuget package is added, or make sure the native binary can be found in the folder of executable.");
            }

            try
            {
                if (Emgu.TF.Util.Toolbox.FindAssembly("Xamarin.iOS.dll") == null)
                {
                    //Not running on iOS
                    //Use the custom error handler
                    RedirectError(TfliteErrorHandlerThrowException);
                }
            }
            catch (DllNotFoundException e)
            {
                String errMsg =
                    "Unable to load native binary. Please make sure a proper Emgu.TF.Lite.runtime.{platform} nuget package is added, or make sure the native binary can be found in the folder of the executable.";
                Trace.WriteLine(errMsg);
                throw new DllNotFoundException(errMsg, e);
            }
        }

        /// <summary>
        /// The default error handler for tensorflow lite
        /// </summary>
        public static readonly TfLiteErrorCallback TfliteErrorHandlerThrowException =
            (TfLiteErrorCallback) TfLiteErrorHandler;

        [DllImport(ExternLibrary, CallingConvention = TfLiteCallingConvention)]
        internal static extern void tfeMemcpy(IntPtr dst, IntPtr src, int length);

        /// <summary>
        /// Get the tensorflow lite version.
        /// </summary>
        public static String Version
        {
            get { return Marshal.PtrToStringAnsi(tfeGetLiteVersion()); }
        }

        [DllImport(ExternLibrary, CallingConvention = TfLiteCallingConvention)]
        internal static extern IntPtr tfeGetLiteVersion();


    }
}
