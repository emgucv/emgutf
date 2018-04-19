//----------------------------------------------------------------------------
//  Copyright (C) 2004-2018 by EMGU Corporation. All rights reserved.       
//----------------------------------------------------------------------------

/*
using System;
using System.Collections.Generic;
using System.Text;
using Emgu.TF.Lite;
using System.IO;

namespace Emgu.TF.Lite.Models
{
    
    public class SmartReply : DownloadableModels
    {
        private String _inputLayer;
        private String _outputLayer;

        public SmartReply(            
            String[] modelFiles = null,
            String downloadUrl = "https://github.com/emgucv/models/raw/master/smartreply_1.0_2017_11_01/",
            String inputLayer = "input", 
            String outputLayer = "output")
            : base(
                modelFiles ?? new string[] { "smartreply.tflite", "backoff_response.txt" },
                downloadUrl)
        {
            _inputLayer = inputLayer;
            _outputLayer = outputLayer;
        }

        public void Init(
            System.Net.DownloadProgressChangedEventHandler onDownloadProgressChanged = null,
            System.ComponentModel.AsyncCompletedEventHandler onDownloadFileCompleted = null)
        {
            int retry = 1;
            Download(
                retry,
                onDownloadProgressChanged,
                (object sender, System.ComponentModel.AsyncCompletedEventArgs e) =>
                {
                    
                    if (onDownloadFileCompleted != null)
                    {
                        onDownloadFileCompleted(sender, e);
                    }
                });
        }
        

        public String[] Labels
        {
            get
            {
                return File.ReadAllLines(GetLocalFileName(_modelFiles[1]));
            }
        }

        
    }
}
*/