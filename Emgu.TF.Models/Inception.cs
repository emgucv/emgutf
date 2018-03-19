//----------------------------------------------------------------------------
//  Copyright (C) 2004-2018 by EMGU Corporation. All rights reserved.       
//----------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Text;
using Emgu.TF;
using System.IO;

namespace Emgu.TF.Models
{
    public class Inception : DownloadableModels
    {
        private String _inputLayer;
        private String _outputLayer;

        public Inception(Status status = null, String[] modelFiles = null, String downloadUrl = "https://s3.amazonaws.com/emgu-public/inception/", String inputLayer = "input", String outputLayer = "output")
            : base(
                modelFiles ?? new string[] { "tensorflow_inception_graph.pb", "imagenet_comp_graph_label_strings.txt" },
                downloadUrl)
        {
            _inputLayer = inputLayer;
            _outputLayer = outputLayer;

            Download();

            byte[] model = File.ReadAllBytes(GetLocalFileName(_modelFiles[0]));

            Buffer modelBuffer = Buffer.FromString(model);

            using (ImportGraphDefOptions options = new ImportGraphDefOptions())
                ImportGraphDef(modelBuffer, options, status);
        }

        public String[] Labels
        {
            get
            {
                return File.ReadAllLines(GetLocalFileName(_modelFiles[1]));
            }
        }

        public float[] Recognize(Tensor image)
        {
            Session inceptionSession = new Session(this);
            Tensor[] finalTensor = inceptionSession.Run(new Output[] { this[_inputLayer] }, new Tensor[] { image },
                new Output[] { this[_outputLayer] });
            float[] probability = finalTensor[0].GetData(false) as float[];
            return probability;
        }
    }
}
