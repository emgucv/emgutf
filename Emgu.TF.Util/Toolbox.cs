//----------------------------------------------------------------------------
//  Copyright (C) 2004-2020 by EMGU Corporation. All rights reserved.       
//----------------------------------------------------------------------------
using System;
using System.Text;

using System.IO;
using System.Runtime.InteropServices;
using System.Diagnostics;
using System.Collections.Generic;

namespace Emgu.TF.Util
{
    /// <summary>
    /// utilities functions for Emgu
    /// </summary>
    public static class Toolbox
    {
        /// <summary>
        /// Maps the specified executable module into the address space of the calling process.
        /// </summary>
        /// <param name="dllname">The name of the dll</param>
        /// <returns>The handle to the library</returns>
        public static IntPtr LoadLibrary(String dllname)
        {
#if UNITY_EDITOR_WIN
            const int loadLibrarySearchDllLoadDir = 0x00000100;
            const int loadLibrarySearchDefaultDirs = 0x00001000;
            return LoadLibraryEx(dllname, IntPtr.Zero, loadLibrarySearchDllLoadDir | loadLibrarySearchDefaultDirs);
#else
            if (System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(System.Runtime.InteropServices.OSPlatform.Windows))
            {
                //if (Platform.ClrType == TypeEnum.ClrType.NetFxCore)
                {
                    const int loadLibrarySearchDllLoadDir = 0x00000100;
                    const int loadLibrarySearchDefaultDirs = 0x00001000;
                    //const int loadLibrarySearchUserDirs = 0x00000400;
                    IntPtr handler = LoadLibraryEx(dllname, IntPtr.Zero, loadLibrarySearchDllLoadDir | loadLibrarySearchDefaultDirs);
                    //IntPtr handler = LoadLibraryEx(dllname, IntPtr.Zero, loadLibrarySearchUserDirs);
                    if (handler == IntPtr.Zero)
                    {
                        int error = Marshal.GetLastWin32Error();

                        System.ComponentModel.Win32Exception ex = new System.ComponentModel.Win32Exception(error);
                        System.Diagnostics.Trace.WriteLine(String.Format("LoadLibraryEx {0} failed with error code {1}: {2}", dllname, (uint)error, ex.Message));
                        if (error == 5)
                        {
                            System.Diagnostics.Trace.WriteLine(String.Format("Please check if the current user has execute permission for file: {0} ", dllname));
                        }
                    }
                    return handler;
                } //else
                  //return WinAPILoadLibrary(dllname);
            }
            else
            {
                
                IntPtr handler = Dlopen(dllname, 2); // 2 == RTLD_NOW
                if (handler == IntPtr.Zero)
                {
                    System.Diagnostics.Trace.WriteLine(String.Format("Failed to use dlopen to load {0}", dllname));
                }

                return handler;
            }
#endif
        }

        [DllImport("Kernel32.dll", SetLastError = true)]
        private static extern IntPtr LoadLibraryEx(
            [MarshalAs(UnmanagedType.LPStr)]
            String fileName,
            IntPtr hFile,
            int dwFlags);

        [DllImport("dl", EntryPoint = "dlopen")]
        private static extern IntPtr Dlopen(
            [MarshalAs(UnmanagedType.LPStr)]
            String dllname, int mode);

        /*
        /// <summary>
        /// Decrements the reference count of the loaded dynamic-link library (DLL). When the reference count reaches zero, the module is unmapped from the address space of the calling process and the handle is no longer valid
        /// </summary>
        /// <param name="handle">The handle to the library</param>
        /// <returns>If the function succeeds, the return value is true. If the function fails, the return value is false.</returns>
        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool FreeLibrary(IntPtr handle);
        */

        /// <summary>
        /// Adds a directory to the search path used to locate DLLs for the application
        /// </summary>
        /// <param name="path">The directory to be searched for DLLs</param>
        /// <returns>True if success</returns>
        [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool SetDllDirectory(String path);
        

        /// <summary>
        /// Find the loaded assembly with the specific assembly name
        /// </summary>
        /// <param name="assembleName"></param>
        /// <returns></returns>
        public static System.Reflection.Assembly FindAssembly(String assembleName)
        {
            try
            {
                System.Reflection.Assembly[] asms = AppDomain.CurrentDomain.GetAssemblies();
                foreach (System.Reflection.Assembly asm in asms)
                {
                    if (asm.ManifestModule.Name.Equals(assembleName))
                        return asm;
                }
            }
            catch
            {

            }
            return null;
        }
    }
}
