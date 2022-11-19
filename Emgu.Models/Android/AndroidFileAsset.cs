//----------------------------------------------------------------------------
//  Copyright (C) 2004-2022 by EMGU Corporation. All rights reserved.       
//----------------------------------------------------------------------------

#if __ANDROID__

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Android.App;
using Android.Content;
using Android.Content.Res;
using Android.OS;
using Android.Runtime;
using Android.Views;

namespace Emgu.TF.Util
{
    /// <summary>
    /// Helper class for writing Android asset into a file
    /// </summary>
    public abstract class AndroidFileAsset : DisposableObject
    {
        /// <summary>
        /// The file info
        /// </summary>
        protected FileInfo _file;

        /// <summary>
        /// The file overwrite policy
        /// </summary>
        protected OverwritePolicy _overwritePolicy;


        /// <summary>
        /// The file overwrite policyt
        /// </summary>
        public enum OverwritePolicy
        {
            /// <summary>
            /// Always overwrite the file
            /// </summary>
            AlwaysOverwrite,
            /*
            /// <summary>
            /// Copy if the current file is newer than the existing one
            /// </summary>
            CopyIfNewer,*/
            /// <summary>
            /// Will never overwrite. Throw exception if the file already exist
            /// </summary>
            NeverOverwrite
        }

        /// <summary>
        /// Copy the Android assets to the app's FilesDir
        /// </summary>
        /// <param name="context">The android context</param>
        /// <param name="assertName">The name of the assert</param>
        /// <param name="dstDir">The subfolder in the app's FilesDir</param>
        /// <param name="policy">overwrite method</param>
        /// <returns>The resulting FileInfo</returns>
        public static FileInfo WritePermanentFileAsset(Context context, String assertName, String dstDir, OverwritePolicy policy)
        {
            String fullPath = Path.Combine(context.FilesDir.AbsolutePath, dstDir, assertName);

            //Create the directory if it is not already exist.
            Directory.CreateDirectory(Path.GetDirectoryName(fullPath));

            using (Stream iStream = context.Assets.Open(assertName))
                WriteStream(iStream, fullPath, policy);
            return new FileInfo(fullPath);
        }

        /// <summary>
        /// Write the io stream to disk
        /// </summary>
        /// <param name="iStream">The io stream</param>
        /// <param name="fileFullPath">The full file path to write to</param>
        /// <param name="policy">The overwrite policy</param>
        /// <exception cref="IOException">Will throw exception if policy is not to overwrite and file already exist.</exception>
        public static void WriteStream(System.IO.Stream iStream, String fileFullPath, OverwritePolicy policy)
        {
            if (policy == OverwritePolicy.NeverOverwrite && File.Exists(fileFullPath))
            {
                throw new IOException(String.Format("A file with the name {0} already exist.", fileFullPath));
            }
            using (Stream os = File.OpenWrite(fileFullPath))
            {
                byte[] buffer = new byte[8 * 1024];
                int len;
                while ((len = iStream.Read(buffer, 0, buffer.Length)) > 0)
                    os.Write(buffer, 0, len);
            }
        }

        /// <summary>
        /// The directory to write the android file assets to.
        /// </summary>
        public String DirectoryName
        {
            get
            {
                return _file.DirectoryName;
            }
        }

        /// <summary>
        /// The full file path of the asset 
        /// </summary>
        public String FileFullPath
        {
            get
            {
                return _file.FullName;
            }
        }

        /// <summary>
        /// Release all the resources associated with this android file asset.
        /// </summary>
        protected override void DisposeObject()
        {
        }
    }

    /// <summary>
    /// Copy the Android assets to the cache folder
    /// </summary>
    public class AndroidCacheFileAsset : AndroidFileAsset
    {

        /// <summary>
        /// Creat a file cache for android asset.
        /// </summary>
        /// <param name="context">The Android context</param>
        /// <param name="assertName">The android asset name</param>
        /// <param name="cacheFolderPostfix">The extra prefix to the cache folder</param>
        /// <param name="policy">The overwrite policy</param>
        public AndroidCacheFileAsset(Context context, String assertName, String cacheFolderPostfix = "tmp", OverwritePolicy policy = OverwritePolicy.NeverOverwrite)
        {
            String fileName = Path.GetFileName(assertName);
            fileName = Path.Combine(context.GetDir(cacheFolderPostfix, FileCreationMode.Private).AbsolutePath, fileName);
            _file = new FileInfo(fileName);
            _overwritePolicy = policy;

            using (System.IO.Stream iStream = context.Assets.Open(assertName))
                WriteStream(iStream, FileFullPath, _overwritePolicy);

        }

        /// <summary>
        /// Release all the resources associated with the Android File Asset.
        /// </summary>
        protected override void DisposeObject()
        {
            if (_file.Exists)
                _file.Delete();

            base.DisposeObject();
        }
    }
}

#endif