//----------------------------------------------------------------------------
//  Copyright (C) 2004-2022 by EMGU Corporation. All rights reserved.       
//----------------------------------------------------------------------------

using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using Emgu.TF.Lite;
using Emgu.Models;
using System.IO;
using System.ComponentModel;
using System.Diagnostics;
using System.Net;
using System.Threading.Tasks;

namespace Emgu.TF.Lite.Models
{
    /// <summary>
    /// Coco SSD Mobilenet V1
    /// </summary>
    public class CocoSsdMobilenetV1 : CocoSsdMobilenet
    {
        /// <summary>
        /// Create Coco SSD Mobilenet V1
        /// </summary>
        public CocoSsdMobilenetV1()
        : base()
        {
        }

        /// <summary>
        /// Initiate the graph by checking if the model file exist locally, if not download the graph from internet.
        /// </summary>
        /// <param name="modelFile">The tflite flatbuffer model files</param>
        /// <param name="labelFile">Text file that contains the labels</param>
        /// <param name="optDelegate">The optional delegate that can be used during model initialization</param>
        public override 
#if UNITY_EDITOR || UNITY_IOS || UNITY_ANDROID || UNITY_STANDALONE
            IEnumerator
#else
            async Task
#endif
            Init(
                DownloadableFile modelFile = null,
                DownloadableFile labelFile = null,
                IDelegate optDelegate = null)
        {
            String defaultLocalSubfolder = "CocoSsdMobilenetV1";
            if (modelFile == null)
            {
                modelFile = new DownloadableFile(
                    "https://github.com/emgucv/models/raw/master/coco_ssd_mobilenet_v1_1.0_quant_2018_06_29/detect.tflite",
                    defaultLocalSubfolder,
                    "E4B118E5E4531945DE2E659742C7C590F7536F8D0ED26D135ABCFE83B4779D13"
                );
            }

            if (labelFile == null)
            {
                labelFile = new DownloadableFile(
                    "https://github.com/emgucv/models/raw/master/coco_ssd_mobilenet_v1_1.0_quant_2018_06_29/labelmap.txt",
                    defaultLocalSubfolder,
                    "C7E79C855F73CBBA9F33D649D60E1676EB0A974021A41696D1AC0D4B7F7E0211"
                );
            }
            

#if UNITY_EDITOR || UNITY_IOS || UNITY_ANDROID || UNITY_STANDALONE
            return base.Init(modelFile, labelFile, optDelegate);
#else
            await base.Init(modelFile, labelFile, optDelegate);
#endif

        }

    }
}
