//----------------------------------------------------------------------------
//  Copyright (C) 2004-2021 by EMGU Corporation. All rights reserved.       
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
    /// Coco SSD Mobilenet V3
    /// </summary>
    public class CocoSsdMobilenetV3 : CocoSsdMobilenet
    {
        /// <summary>
        /// Create Coco SSD Mobilenet V3
        /// </summary>
        public CocoSsdMobilenetV3()
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

            String defaultLocalSubfolder = "CocoSsdMobilenetV3";
            if (modelFile == null)
            {
                modelFile = new DownloadableFile(
                    "https://github.com/emgucv/models/raw/master/coco_ssd_mobilenet_v3_small_2020_01_14/model.tflite",
                    defaultLocalSubfolder,
                    "0F52F7A4884DD6426D38F4AFC06DA75105EFE77F7C83E470254C267FE34CC43C"
                );
            }

            if (labelFile == null)
            {
                labelFile = new DownloadableFile(
                    "https://github.com/emgucv/models/raw/master/coco_ssd_mobilenet_v3_small_2020_01_14/labelmap.txt",
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
