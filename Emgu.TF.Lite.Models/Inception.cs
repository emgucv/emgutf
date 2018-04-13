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
                    /*
                    byte[] model = File.ReadAllBytes(GetLocalFileName(_modelFiles[0]));

                    Buffer modelBuffer = Buffer.FromString(model);

                    using (ImportGraphDefOptions options = new ImportGraphDefOptions())
                        ImportGraphDef(modelBuffer, options, status);
                    */
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

        /*
        public float[] Recognize(Tensor image)
        {
            Session inceptionSession = new Session(this);
            Tensor[] finalTensor = inceptionSession.Run(new Output[] { this[_inputLayer] }, new Tensor[] { image },
                new Output[] { this[_outputLayer] });
            float[] probability = finalTensor[0].GetData(false) as float[];
            return probability;
        }*/
    }
}
