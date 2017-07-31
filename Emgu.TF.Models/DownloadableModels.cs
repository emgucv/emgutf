//----------------------------------------------------------------------------
//  Copyright (C) 2004-2017 by EMGU Corporation. All rights reserved.       
//----------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Text;
using Emgu.TF;
using System.IO;
using System.Diagnostics;

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

        private void Download(String fileName, int retry = 1)
        {
            if (_downloadUrl == null)
                return;
#if __ANDROID__
            String multiboxFile = System.IO.Path.Combine(Android.OS.Environment.ExternalStorageDirectory.AbsolutePath,
                Android.OS.Environment.DirectoryDownloads, fileName);
#else
            String multiboxFile = fileName;
#endif
            if (!File.Exists(multiboxFile) || !(new FileInfo(multiboxFile).Length > 0))
            {
                try
                {
                    //Download the tensorflow file
                    String multiboxUrl = _downloadUrl + fileName;
                    Trace.WriteLine("downloading file from:" + multiboxUrl + " to: " + multiboxFile);
                    System.Net.WebClient downloadClient = new System.Net.WebClient();
                    downloadClient.DownloadFile(multiboxUrl, multiboxFile);

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
