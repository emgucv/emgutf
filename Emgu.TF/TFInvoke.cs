//----------------------------------------------------------------------------
//  Copyright (C) 2004-2019 by EMGU Corporation. All rights reserved.       
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

        /*
        private static Status _defaultStatus = null;

        
        public static Status DefaultStatus
        {
            get
            {
                if (_defaultStatus == null)
                {
                    _defaultStatus = new Status();
                }
                return _defaultStatus;
            }
        }*/

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

        /// <summary>
        /// Attempts to load tensorflow modules from the specific location
        /// </summary>
        /// <param name="loadDirectory">The directory where the unmanaged modules will be loaded. If it is null, the default location will be used.</param>
        /// <param name="unmanagedModules">The names of tensorflow modules. </param>
        /// <returns>True if all the modules has been loaded successfully</returns>
        /// <remarks>If <paramref name="loadDirectory"/> is null, the default location on windows is the dll's path appended by either "x64" or "x86", depends on the applications current mode.</remarks>
        public static bool LoadUnmanagedModules(String loadDirectory, params String[] unmanagedModules)
        {
#if NETFX_CORE
         if (loadDirectory != null)
         {
            throw new NotImplementedException("Loading modules from a specific directory is not implemented in Windows Store App");
         }

         String subfolder = String.Empty;
         if (Emgu.TF.Util.Platform.OperationSystem == Emgu.TF.Util.TypeEnum.OS.Windows) //|| Platform.OperationSystem == Emgu.Util.TypeEnum.OS.WindowsPhone)
         {
            if (IntPtr.Size == 8)
            {  //64bit process
#if UNITY_METRO
               subfolder = "x86_64";
#else
               subfolder = String.Empty;
#endif
            }
            else
            {
               subfolder = String.Empty;
            }
         }

         Windows.Storage.StorageFolder installFolder = Windows.ApplicationModel.Package.Current.InstalledLocation;
         
#if UNITY_METRO
         loadDirectory = Path.Combine(
            Path.GetDirectoryName(Path.GetDirectoryName(Path.GetDirectoryName(Path.GetDirectoryName( installFolder.Path))))
            , "Plugins", "Metro", subfolder);
#else
         loadDirectory = Path.Combine(installFolder.Path, subfolder);
#endif

         var t = System.Threading.Tasks.Task.Run(async () =>
         {
            List<string> files = new List<string>();
            Windows.Storage.StorageFolder loadFolder = installFolder;
            try
            {
               
               if (!String.IsNullOrEmpty(subfolder))
                  loadFolder = await installFolder.GetFolderAsync(subfolder);

               foreach (var file in await loadFolder.GetFilesAsync())
                  files.Add(file.Name);
            }
            catch (Exception e)
            {
               System.Diagnostics.Debug.WriteLine(String.Format("Unable to retrieve files in folder '{0}':{1}", loadFolder.Path, e.StackTrace));
            }

            return files;
         });
         t.Wait();

         List<String> loadableFiles = t.Result;
#else
            if (loadDirectory == null)
            {
                String subfolder = String.Empty;
#if UNITY_EDITOR_WIN
            subfolder = IntPtr.Size == 8 ? "x86_64" : "x86";
#elif UNITY_STANDALONE_WIN
#else
                if (Emgu.TF.Util.Platform.OperationSystem == Emgu.TF.Util.TypeEnum.OS.Windows)
                {
                    subfolder = IntPtr.Size == 8 ? "x64" : "x86";

                    if ("x86".Equals(subfolder))
                    {
                        throw new Exception("Emgu TF is only compatible with 64bit mode in Windows (not compatible with 32bit x86 mode)");
                    }
                }
#endif

                /*
                else if (Platform.OperationSystem == Emgu.TF.Util.TypeEnum.OS.MacOSX)
                {
                   subfolder = "..";
                }*/

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
                    /*
                                   loadDirectory = Path.GetDirectoryName(new UriBuilder(System.Reflection.Assembly.GetExecutingAssembly().CodeBase).Path);

                                   DirectoryInfo dir = new DirectoryInfo(loadDirectory);
                                   string subdir = String.Join(";", Array.ConvertAll(dir.GetDirectories(), d => d.ToString()));

                                   throw new Exception(String.Format(
                                      "The Emgu.CV.dll assembly path (typeof (CvInvoke).Assembly.Location) '{0}' is invalid." +
                                      Environment.NewLine
                                      + " Other possible path (System.Reflection.Assembly.GetExecutingAssembly().Location): '{1}';" +
                                      Environment.NewLine
                                      + " Other possible path (Path.GetDirectoryName(new UriBuilder(System.Reflection.Assembly.GetExecutingAssembly().CodeBase).Path): '{2}';" +
                                      Environment.NewLine
                                      + " Other possible path (System.Reflection.Assembly.GetExecutingAssembly().CodeBase): '{3};'" +
                                      Environment.NewLine
                                      + " Other possible path (typeof(CvInvoke).Assembly.CodeBase): '{4}'" +
                                      Environment.NewLine
                                      + " Other possible path (AppDomain.CurrentDomain.BaseDirectory): '{5}'" +
                                      Environment.NewLine
                                      + " subfolder name: '{6}'",
                                      asm.Location,
                                      Path.GetDirectoryName(new UriBuilder(System.Reflection.Assembly.GetExecutingAssembly().CodeBase).Path),
                                      loadDirectory + ": subdir '" + subdir +"'",
                                      System.Reflection.Assembly.GetExecutingAssembly().CodeBase,
                                      typeof(CvInvoke).Assembly.Location,
                                      AppDomain.CurrentDomain.BaseDirectory,
                                      subfolder
                                      ));
                     */
                }
                else
                {
                    loadDirectory = Path.GetDirectoryName(asm.Location);
                }
                /*
                FileInfo file = new FileInfo(asm.Location);
                //FileInfo file = new FileInfo(asm.CodeBase);
                DirectoryInfo directory = file.Directory;
                loadDirectory = directory.FullName;
                */

                if (!String.IsNullOrEmpty(subfolder))
                    loadDirectory = Path.Combine(loadDirectory, subfolder);

#if (UNITY_STANDALONE_WIN && !UNITY_EDITOR_WIN)
				FileInfo file = new FileInfo(asm.Location);
				DirectoryInfo directory = file.Directory;
            if (directory.Parent != null)
            {
               String unityAltFolder = Path.Combine(directory.Parent.FullName, "Plugins");
              
               if (Directory.Exists(unityAltFolder))
                  loadDirectory = unityAltFolder;
               else
               {
                  Debug.WriteLine("No suitable directory found to load unmanaged modules");
                  return false;
               }
            }
#elif __ANDROID__ || UNITY_ANDROID
#else
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
#if UNITY_EDITOR_WIN
              if (directory.Parent != null && directory.Parent.Parent != null)
                  {
                     String unityAltFolder =
                        Path.Combine(
                           Path.Combine(Path.Combine(Path.Combine(directory.Parent.Parent.FullName, "Assets"), "Emgu.TF"), "Plugins"),
                           subfolder);
                     
					 Debug.WriteLine("Trying unityAltFolder: " + unityAltFolder);
                     if (Directory.Exists(unityAltFolder))
                        loadDirectory = unityAltFolder;
                     else
                     {
                        Debug.WriteLine("No suitable directory found to load unmanaged modules");
                        return false;
                     }
                    
                  }
                  else
#elif (UNITY_EDITOR_OSX || UNITY_STANDALONE_OSX)
                     if (directory.Parent != null && directory.Parent.Parent != null)
                  {
                     String unityAltFolder =
                        Path.Combine(Path.Combine(Path.Combine(
                           Path.Combine(Path.Combine(directory.Parent.Parent.FullName, "Assets"), "Plugins"),
                           "emgucv.bundle"), "Contents"), "MacOS");
                     
                     if (Directory.Exists(unityAltFolder))
                     {
                        loadDirectory = unityAltFolder;
                     }
                     else
                     {
                        return false;
                     }
                     
                  }
                  else       
#endif
                        {
                            Debug.WriteLine("No suitable directory found to load unmanaged modules");
                            return false;
                        }
                    }
                    else
                        loadDirectory = altLoadDirectory;
                }
#endif
            }

            String oldDir = Environment.CurrentDirectory;
            if (!String.IsNullOrEmpty(loadDirectory) && Directory.Exists(loadDirectory))
                Environment.CurrentDirectory = loadDirectory;
#endif

            System.Diagnostics.Debug.WriteLine(String.Format("Loading tensorflow binary from {0}", loadDirectory));
            bool success = true;

            string prefix = string.Empty;

            foreach (String module in unmanagedModules)
            {
                string mName = module;

                String fullPath = Path.Combine(prefix, mName);

#if NETFX_CORE
            if (loadableFiles.Exists(sf => sf.Equals(fullPath)))
            {
               IntPtr handle = Toolbox.LoadLibrary(fullPath);
               success &= (!IntPtr.Zero.Equals(handle));
            }
            else
            {
               success = false;
            }
#else
                //Use absolute path for Windows Desktop
                fullPath = Path.Combine(loadDirectory, fullPath);

                bool fileExist = File.Exists(fullPath);
                if (!fileExist)
                    System.Diagnostics.Debug.WriteLine(String.Format("File {0} do not exist.", fullPath));
                bool fileExistAndLoaded = fileExist && !IntPtr.Zero.Equals(Toolbox.LoadLibrary(fullPath));
                if (fileExist && (!fileExistAndLoaded))
                    System.Diagnostics.Debug.WriteLine(String.Format("File {0} cannot be loaded.", fullPath));
                success &= fileExistAndLoaded;
#endif
            }

#if !NETFX_CORE
            Environment.CurrentDirectory = oldDir;
#endif
            return success;
        }

        /// <summary>
        /// Get the module format string.
        /// </summary>
        /// <returns>On Windows, "{0}".dll will be returned; On Linux, "lib{0}.so" will be returned; Otherwise {0} is returned.</returns>
        public static String GetModuleFormatString()
        {
#if UNITY_EDITOR_WIN
         return "{0}.dll";
#elif UNITY_EDITOR_OSX
         return "lib{0}.dylib";
#else
            String formatString = "{0}";
            if (Emgu.TF.Util.Platform.OperationSystem == Emgu.TF.Util.TypeEnum.OS.Windows
                || Emgu.TF.Util.Platform.OperationSystem == Emgu.TF.Util.TypeEnum.OS.WindowsPhone)
                formatString = "{0}.dll";
            else if (Emgu.TF.Util.Platform.OperationSystem == Emgu.TF.Util.TypeEnum.OS.Linux)
                formatString = "lib{0}.so";
            else if (Emgu.TF.Util.Platform.OperationSystem == Emgu.TF.Util.TypeEnum.OS.MacOSX)
                formatString = "lib{0}.dylib";
            return formatString;
#endif
        }

        /// <summary>
        /// Attempts to load tensorflow modules from the specific location
        /// </summary>
        /// <param name="modules">The names of tensorflow modules.</param>
        /// <returns>True if all the modules has been loaded successfully</returns>
        public static bool DefaultLoadUnmanagedModules(String[] modules)
        {
            bool libraryLoaded = true;
#if __ANDROID__ || (UNITY_ANDROID && !UNITY_EDITOR)

         System.Reflection.Assembly asm = System.Reflection.Assembly.GetExecutingAssembly();
         FileInfo file = new FileInfo(asm.Location);
         DirectoryInfo directory = file.Directory;

#if (UNITY_ANDROID && !UNITY_EDITOR)
         UnityEngine.AndroidJavaObject jo = new UnityEngine.AndroidJavaObject("java.lang.System");
#endif
         foreach (String module in modules)
         {
            //IntPtr handle = Emgu.TF.Util.Toolbox.LoadLibrary(module);
            //Debug.WriteLine(string.Format(handle == IntPtr.Zero ? "Failed to load {0}." : "Loaded {0}.", module));
            try
            {

               Console.WriteLine(string.Format("Trying to load {0}.", module));
#if __ANDROID__
               Java.Lang.JavaSystem.LoadLibrary(module);
#else //(UNITY_ANDROID && !UNITY_EDITOR)

               jo.CallStatic("loadLibrary", module); 
#endif
               Console.WriteLine(string.Format("Loaded {0}.", module));
            }
            catch (Exception e)
            {
               libraryLoaded = false;
               Console.WriteLine(String.Format("Failed to load {0}: {1}", module, e.Message));
            }
         }
#elif __IOS__ || UNITY_IOS || NETFX_CORE
#else
            if (Emgu.TF.Util.Platform.OperationSystem != Emgu.TF.Util.TypeEnum.OS.MacOSX)
            {
                String formatString = GetModuleFormatString();
                for (int i = 0; i < modules.Length; ++i)
                    modules[i] = String.Format(formatString, modules[i]);

                libraryLoaded &= LoadUnmanagedModules(null, modules);
            }
#endif
            return libraryLoaded;
        }

        /// <summary>
        /// Static Constructor to setup tensorflow environment
        /// </summary>
        static TfInvoke()
        {
            //Debug.WriteLine(String.Format("Emgu TF Running in {0} bit mode.", IntPtr.Size));
            List<String> modules = TfInvoke.TensorflowModuleList;
            modules.RemoveAll(String.IsNullOrEmpty);

            _libraryLoaded = DefaultLoadUnmanagedModules(modules.ToArray());
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

        [DllImport(ExternLibrary, CallingConvention = TfInvoke.TFCallingConvention )]
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
        [return:MarshalAs(TfInvoke.BoolMarshalType)]
        private static extern bool tfeIsGoogleCudaEnabled();
    }
}
