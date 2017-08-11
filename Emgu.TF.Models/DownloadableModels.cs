//----------------------------------------------------------------------------
//  Copyright (C) 2004-2017 by EMGU Corporation. All rights reserved.       
//----------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Text;
using Emgu.TF;
using System.IO;
using System.Diagnostics;
#if UNITY_EDITOR || UNITY_IOS || UNITY_ANDROID || UNITY_STANDALONE
using UnityEngine;
#endif

namespace Emgu.TF.Models
{
    public class DownloadableModels : Graph
    {
        public DownloadableModels(String[] modelFiles, String downloadUrl)
        {
            _modelFiles = modelFiles;
            _downloadUrl = downloadUrl;
        }

        public String[] _modelFiles;
        public String _downloadUrl;

        public void Download(int retry = 1)
        {
            foreach (var fileName in _modelFiles)
            {
                Download(fileName, retry);
            }
        }

#if __ANDROID__
        public static String PersistentDataPath = System.IO.Path.Combine(Android.OS.Environment.ExternalStorageDirectory.AbsolutePath, Android.OS.Environment.DirectoryDownloads);
#elif UNITY_EDITOR || UNITY_IOS || UNITY_ANDROID || UNITY_STANDALONE
        public static String PersistentDataPath = Application.persistentDataPath;
#endif

        public static String GetLocalFileName(String fileName)
        {
#if __ANDROID__
            return System.IO.Path.Combine(PersistentDataPath, fileName);
#elif UNITY_EDITOR || UNITY_IOS || UNITY_ANDROID || UNITY_STANDALONE
            return System.IO.Path.Combine(PersistentDataPath, fileName);
#else
            return fileName;
#endif
        }

        private void Download(String fileName, int retry = 1)
        {
            if (_downloadUrl == null)
                return;

            String localFile = GetLocalFileName(fileName);

            if (!File.Exists(localFile) || !(new FileInfo(localFile).Length > 0))
            {
                try
                {
                    //Download the tensorflow file
                    String multiboxUrl = _downloadUrl + fileName;
                    Trace.WriteLine("downloading file from:" + multiboxUrl + " to: " + localFile);
                    System.Net.WebClient downloadClient = new System.Net.WebClient();
                    downloadClient.DownloadFile(multiboxUrl, localFile);

                }
                catch (Exception e)
                {
                    if (retry > 0)
                    {
                        Download(retry - 1);
                    }
                    else
                    {
                        Console.WriteLine(e);
                        throw;
                    }
                }
            }
        }
    }
}
