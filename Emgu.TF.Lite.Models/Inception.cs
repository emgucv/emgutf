//----------------------------------------------------------------------------
//  Copyright (C) 2004-2018 by EMGU Corporation. All rights reserved.       
//----------------------------------------------------------------------------

/*
using System;
using System.Collections.Generic;
using System.Text;
using Emgu.TF;
using System.IO;

namespace Emgu.TF.Lite.Models
{
    
    public class Inception : DownloadableModels
    {
        private String _inputLayer;
        private String _outputLayer;

        public Inception(            
            String[] modelFiles = null,
            String downloadUrl = "https://github.com/emgucv/models/raw/master/inception_v3_slim/",
            String inputLayer = "input", 
            String outputLayer = "output")
            : base(
                modelFiles ?? new string[] { "inceptionv3_slim_2016.tflite", "imagenet_slim_labels.txt" },
                downloadUrl)
        {
            _inputLayer = inputLayer;
            _outputLayer = outputLayer;
        }

        public void Init()
        {
            int retry = 1;
            Download(retry);
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