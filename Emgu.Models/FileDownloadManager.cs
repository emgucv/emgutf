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

namespace Emgu.Models
{
    public class FileDownloadManager
    {
        public FileDownloadManager()
        {
        }

        public event System.Net.DownloadProgressChangedEventHandler OnDownloadProgressChanged;
        public event System.ComponentModel.AsyncCompletedEventHandler OnDownloadCompleted;


        private List<DownloadableFile> _files = new List<DownloadableFile>();
        
        public void Clear()
        {
            _files.Clear();
        }

        public void AddFile(String url)
        {
            _files.Add(new DownloadableFile(url));
        }

        public DownloadableFile[] Files
        {
            get
            {
                return _files.ToArray();
            }
        }

        public void Download(int retry = 1)
        {
            Download( _files.ToArray(), retry, this.OnDownloadProgressChanged, this.OnDownloadCompleted);
        }

        private static void Download(
            DownloadableFile[] files,
            int retry = 1,
            System.Net.DownloadProgressChangedEventHandler onDownloadProgressChanged = null,
            System.ComponentModel.AsyncCompletedEventHandler onDownloadFileCompleted = null)
        {
            DownloadHelper(files, retry, onDownloadProgressChanged, onDownloadFileCompleted);
        }

        private static void DownloadHelper(
            DownloadableFile[] downloadableFiles,
            int retry = 1, 
            System.Net.DownloadProgressChangedEventHandler onDownloadProgressChanged = null,
            System.ComponentModel.AsyncCompletedEventHandler onDownloadFileCompleted = null)
        {
            if (downloadableFiles == null || downloadableFiles.Length == 0)
            {
                if (onDownloadFileCompleted != null)
                    onDownloadFileCompleted(null, null);
            } else if (downloadableFiles.Length == 1)
            {
                DownloadHelper(downloadableFiles[0], retry, onDownloadProgressChanged, onDownloadFileCompleted);
            } else
            {
                DownloadableFile currentFile = downloadableFiles[0];
                DownloadableFile[] remainingFiles = new DownloadableFile[downloadableFiles.Length - 1];
                Array.Copy(downloadableFiles, 1, remainingFiles, 0, remainingFiles.Length);
                DownloadHelper(currentFile, retry, onDownloadProgressChanged,
                    (object sender, System.ComponentModel.AsyncCompletedEventArgs e) =>
                    {
                        DownloadHelper(remainingFiles, retry, onDownloadProgressChanged, onDownloadFileCompleted);
                    }
                    );
            }
        }

        private static void DownloadHelper(
            DownloadableFile downloadableFile,
            int retry = 1, 
            System.Net.DownloadProgressChangedEventHandler onDownloadProgressChanged = null,
            System.ComponentModel.AsyncCompletedEventHandler onDownloadFileCompleted = null
            )
        {
            if (downloadableFile.Url == null)
                return;

            //uncomment the following line to force re-download every time.
            //File.Delete(downloadableFile.LocalFile);
            if (!File.Exists(downloadableFile.LocalFile) || !(new FileInfo(downloadableFile.LocalFile).Length > 0))
            {
                try
                {
                    //Download the file
                    Trace.WriteLine("downloading file from:" + downloadableFile.Url + " to: " + downloadableFile.LocalFile);
                    System.Net.WebClient downloadClient = new System.Net.WebClient();
                    
                    if (onDownloadProgressChanged != null)
                        downloadClient.DownloadProgressChanged += onDownloadProgressChanged;
                    if (onDownloadFileCompleted != null)
                        downloadClient.DownloadFileCompleted += onDownloadFileCompleted;

                    downloadClient.DownloadFileAsync(new Uri(downloadableFile.Url), downloadableFile.LocalFile);
                    
                }
                catch (Exception e)
                {
                    if (File.Exists(downloadableFile.LocalFile))
                        //The downloaded file may be corrupted, should delete it
                        File.Delete(downloadableFile.LocalFile);

                    if (retry > 0)
                    {
                        DownloadHelper( downloadableFile,  retry - 1);
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
