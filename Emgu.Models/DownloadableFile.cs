//----------------------------------------------------------------------------
//  Copyright (C) 2004-2018 by EMGU Corporation. All rights reserved.       
//----------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Diagnostics;
#if UNITY_EDITOR || UNITY_IOS || UNITY_ANDROID || UNITY_STANDALONE
using UnityEngine;
#endif

/// <summary>
/// Utilities for downloading model files.
/// </summary>
namespace Emgu.Models
{
    public class DownloadableFile
    {
        public DownloadableFile(String url)
        {
            Url = url;
        }

        private String _localFile = null;
        public String Url;

        public String LocalFile
        {
            get
            {
                if (_localFile != null)
                    return _localFile;
                else if (Url == null)
                    return null;
                else
                {
                    Uri uri = new Uri(Url);

                    string fileName = System.IO.Path.GetFileName(uri.LocalPath);
                    return GetLocalFileName(fileName);

                }
            }
            set
            {
                _localFile = value;
            }
        }

#if __ANDROID__
        public static String PersistentDataPath = System.IO.Path.Combine(Android.OS.Environment.ExternalStorageDirectory.AbsolutePath, Android.OS.Environment.DirectoryDownloads);
#elif UNITY_EDITOR || UNITY_IOS || UNITY_ANDROID || UNITY_STANDALONE
        public static String PersistentDataPath = Application.persistentDataPath;
#endif

        public static String GetLocalFileName(String fileName)
        {
#if __ANDROID__ || UNITY_EDITOR || UNITY_IOS || UNITY_ANDROID || UNITY_STANDALONE
            return System.IO.Path.Combine(PersistentDataPath, fileName);
#else
            return fileName;
#endif
        }
    }

}
