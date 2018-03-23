//----------------------------------------------------------------------------
//  Copyright (C) 2004-2018 by EMGU Corporation. All rights reserved.       
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

        public void Download(int retry = 1,
            System.Net.DownloadProgressChangedEventHandler onDownloadProgressChanged = null,
            System.ComponentModel.AsyncCompletedEventHandler onDownloadFileCompleted = null)
        {
            DownloadHelper(_modelFiles, _downloadUrl, retry, onDownloadProgressChanged, onDownloadFileCompleted);
        }

        private static void DownloadHelper(
            String[] fileNames,
            String downloadUrl,
            int retry = 1, 
            System.Net.DownloadProgressChangedEventHandler onDownloadProgressChanged = null,
            System.ComponentModel.AsyncCompletedEventHandler onDownloadFileCompleted = null)
        {
            if (fileNames == null || fileNames.Length == 0)
            {
                if (onDownloadFileCompleted != null)
                    onDownloadFileCompleted(null, null);
            } else if (fileNames.Length == 1)
            {
                DownloadHelper(fileNames[0], downloadUrl, retry, onDownloadProgressChanged, onDownloadFileCompleted);
            } else
            {
                String currentFile = fileNames[0];
                String[] remainingFiles = new String[fileNames.Length - 1];
                Array.Copy(fileNames, 1, remainingFiles, 0, remainingFiles.Length);
                DownloadHelper(currentFile, downloadUrl, retry, onDownloadProgressChanged,
                    (object sender, System.ComponentModel.AsyncCompletedEventArgs e) =>
                    {
                        DownloadHelper(remainingFiles, downloadUrl, retry, onDownloadProgressChanged, onDownloadFileCompleted);
                    }
                    );
            }
            /*
            foreach (var fileName in _modelFiles)
            {
                Download(fileName, retry, onDownloadProgressChanged, onDownloadFileCompleted);
            }*/
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


        private static void DownloadHelper(
            String fileName, 
            String downloadUrl,
            int retry = 1, 
            System.Net.DownloadProgressChangedEventHandler onDownloadProgressChanged = null,
            System.ComponentModel.AsyncCompletedEventHandler onDownloadFileCompleted = null
            )
        {
            if (downloadUrl == null)
                return;

            String localFile = GetLocalFileName(fileName);

            if (!File.Exists(localFile) || !(new FileInfo(localFile).Length > 0))
            {
                try
                {
                    //Download the tensorflow file
                    String multiboxUrl = downloadUrl + fileName;
                    Trace.WriteLine("downloading file from:" + multiboxUrl + " to: " + localFile);
                    System.Net.WebClient downloadClient = new System.Net.WebClient();
                    
                    if (onDownloadProgressChanged != null)
                        downloadClient.DownloadProgressChanged += onDownloadProgressChanged;
                    if (onDownloadFileCompleted != null)
                        downloadClient.DownloadFileCompleted += onDownloadFileCompleted;

                    downloadClient.DownloadFileAsync(new Uri( multiboxUrl ), localFile);
                    
                }
                catch (Exception e)
                {
                    if (File.Exists(localFile))
                        //The downloaded file may be corrupted, should delete it
                        File.Delete(localFile);

                    if (retry > 0)
                    {
                        DownloadHelper(fileName, downloadUrl, retry - 1);
                    }
                    else
                    {

                        Debug.WriteLine(e);
                        throw;
                    }
                }
            } else
            {
                if (onDownloadFileCompleted != null)
                     onDownloadFileCompleted(null, null);
            }
        }
    }
}
