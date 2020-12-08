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

namespace Emgu.TF.Lite
{
    public static partial class TfLiteInvoke
    {
#if UNITY_ANDROID || UNITY_IOS || UNITY_EDITOR || UNITY_STANDALONE
        [AOT.MonoPInvokeCallback(typeof(TfliteErrorCallback))]
#endif
        private static int TfliteErrorHandler(
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
        [UnmanagedFunctionPointer(TFCallingConvention)]
        public delegate int TfliteErrorCallback(int status, IntPtr errMsg);

        /// <summary>
        /// Redirect tensorflow lite error.
        /// </summary>
        /// <param name="errorHandler">The error handler</param>
        [DllImport(ExternLibrary, CallingConvention = TFCallingConvention, EntryPoint = "tfeRedirectError")]
        public static extern void RedirectError(TfliteErrorCallback errorHandler);

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
                        throw new Exception("Emgu TF Lite is only compatible with 64bit mode in Windows (not compatible with 32bit x86 mode)");
                    }
                }
                System.Reflection.Assembly asm = typeof(TfLiteInvoke).Assembly; //System.Reflection.Assembly.GetExecutingAssembly();
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
                            if (!Directory.Exists(Path.Combine(baseDirectoryInfo.FullName, subfolder)))
                            {
                                subfolder = String.Empty;
                            }
                        }
                    }
                }
                else
                {
                    loadDirectory = Path.GetDirectoryName(asm.Location);
                    if ((loadDirectory != null) && (!Directory.Exists(Path.Combine(loadDirectory, subfolder))))
                    {
                        subfolder = String.Empty;
                    }
                }

                if (!String.IsNullOrEmpty(subfolder))
                {
                    var temp = Path.Combine(loadDirectory, subfolder);
                    if (Directory.Exists(temp))
                    {
                        loadDirectory = temp;
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
                        String altLoadDirectory = Path.GetDirectoryName(asm.CodeBase);
                        if (!String.IsNullOrEmpty(altLoadDirectory) && altLoadDirectory.StartsWith(@"file:\"))
                            altLoadDirectory = altLoadDirectory.Substring(6);

                        if (!String.IsNullOrEmpty(subfolder))
                            altLoadDirectory = Path.Combine(altLoadDirectory, subfolder);

                        if (!Directory.Exists(altLoadDirectory))
                        {
                            FileInfo file = new FileInfo(asm.Location);
                            DirectoryInfo directory = file.Directory;

                            System.Diagnostics.Trace.WriteLine("No suitable directory found to load unmanaged modules, please make sure a Emgu.TF.Lite.Runtime project / nuget package is referenced.");
                            return false;

                        }
                        else
                            loadDirectory = altLoadDirectory;
                    }

                }
            }

            bool addDllDirectorySuccess = false;
            if (!String.IsNullOrEmpty(loadDirectory) && Directory.Exists(loadDirectory))
            {
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    addDllDirectorySuccess = Emgu.TF.Util.Toolbox.AddDllDirectory(loadDirectory);
                    if (!addDllDirectorySuccess)
                    {
                        System.Diagnostics.Debug.WriteLine(String.Format("Failed to add dll directory: {0}", loadDirectory));
                    }
                } else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                {
                    /*
                    var native_search_dirs = AppContext.GetData("NATIVE_DLL_SEARCH_DIRECTORIES");
                    if (native_search_dirs == null)
                    {
                        System.Diagnostics.Debug.WriteLine("NATIVE_DLL_SEARCH_DIRECTORIES: NULL");
                    }
                    else
                    {
                        System.Diagnostics.Debug.WriteLine(String.Format("NATIVE_DLL_SEARCH_DIRECTORIES: {0}", native_search_dirs));
                    }
                    
                    String ldLibraryPath = Environment.GetEnvironmentVariable("LD_LIBRARY_PATH");
                    if (ldLibraryPath == null)
                    {
                        Environment.SetEnvironmentVariable("LD_LIBRARY_PATH", loadDirectory);
                        String verifyPath = Environment.GetEnvironmentVariable("LD_LIBRARY_PATH");
                    } else if (!ldLibraryPath.Contains(loadDirectory))
                    {
                        String newLdLibraryPath = String.Format("{0};{1}", loadDirectory, ldLibraryPath);
                        Environment.SetEnvironmentVariable("LD_LIBRARY_PATH", newLdLibraryPath);
                    }
                    addDllDirectorySuccess = true;*/
                }
                else if (Emgu.TF.Util.Toolbox.FindAssembly("Xamarin.iOS.dll") != null)
                {
                    //do nothing
                    System.Diagnostics.Debug.WriteLine("iOS required static linking, Setting loadDirectory is not supported");
                }
                else
                {
                    oldDir = Environment.CurrentDirectory;
                    Environment.CurrentDirectory = loadDirectory;
                }
            }

            if (addDllDirectorySuccess)
            {
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    System.Diagnostics.Debug.WriteLine(
                        String.Format(
                            "Loading TF Lite binary for default locations. Current directory: {0}; Additional load folder: {1}",
                            Environment.CurrentDirectory,
                            loadDirectory));                    
                }
                
            }
            else
            {
                System.Diagnostics.Debug.WriteLine(
                    String.Format(
                        "Loading TF Lite binary for default locations. Current directory: {0}", 
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
                    /*
                    loaded = fileExist && !IntPtr.Zero.Equals(Toolbox.LoadLibrary(module));
                    if (loaded)
                    {
                        System.Diagnostics.Trace.WriteLine(String.Format("File {0} loaded.", fullPath));
                    }
                    else
                    {
                        System.Diagnostics.Trace.WriteLine(String.Format("File {0} cannot be loaded.", fullPath));
                    }*/
                }

                success &= (fileExist && loaded);
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
        /// Attempts to load tensorflow modules from the specific location
        /// </summary>
        /// <param name="modules">The names of tensorflow modules.</param>
        /// <returns>True if all the modules has been loaded successfully</returns>
        public static bool DefaultLoadUnmanagedModules(String[] modules)
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
                                Console.WriteLine(string.Format("Trying to load {0} ({1} bit).", module,
                                    IntPtr.Size * 8));
                                loadLibraryMethodInfo.Invoke(null, new object[] { module });
                                //Java.Lang.JavaSystem.LoadLibrary(module);
                                Console.WriteLine(string.Format("Loaded {0}.", module));
                            }
                            catch (Exception e)
                            {
                                libraryLoaded = false;
                                Console.WriteLine(String.Format("Failed to load {0}: {1}", module, e.Message));
                            }
                        }
                        return libraryLoaded;
                    }
                }
            }

            if (!System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(System.Runtime.InteropServices.OSPlatform.OSX))
            {
                //String formatString = GetModuleFormatString();
                //for (int i = 0; i < modules.Length; ++i)
                //    modules[i] = String.Format(formatString, modules[i]);

                libraryLoaded &= LoadUnmanagedModules(null, modules);
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
                Trace.WriteLine("Failed to load native binary. Please make sure a proper Emgu.TF.Lite.runtime.{platform} nuget package is added, or make sure the native binary can be found in the folder of executable.");
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
        public static readonly TfliteErrorCallback TfliteErrorHandlerThrowException = (TfliteErrorCallback)TfliteErrorHandler;

        [DllImport(ExternLibrary, CallingConvention = TFCallingConvention)]
        internal static extern void tfeMemcpy(IntPtr dst, IntPtr src, int length);

        /// <summary>
        /// Get the tensorflow lite version.
        /// </summary>
        public static String Version
        {
            get
            {
                return Marshal.PtrToStringAnsi(tfeGetLiteVersion());
            }
        }

        [DllImport(ExternLibrary, CallingConvention = TFCallingConvention)]
        internal static extern IntPtr tfeGetLiteVersion();
    }
}
