//----------------------------------------------------------------------------
//  Copyright (C) 2004-2018 by EMGU Corporation. All rights reserved.       
//----------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Text;
using Emgu.TF;
using System.IO;

namespace Emgu.TF.Lite.Models
{
    public class Mobilenet : DownloadableModels
    {
        private String _inputLayer;
        private String _outputLayer;

        public Mobilenet(            
            String[] modelFiles = null,
            String downloadUrl = "https://github.com/emgucv/models/raw/master/mobilenet_v1_1.0_224_float_2017_11_08/",
            String inputLayer = "input", 
            String outputLayer = "output")
            : base(
                modelFiles ?? new string[] { "mobilenet_v1_1.0_224.tflite", "labels.txt" },
                downloadUrl)
        {
            _inputLayer = inputLayer;
            _outputLayer = outputLayer;
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
