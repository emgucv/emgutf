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
        /// <param name="modelFiles">An array where the first file is the tensorflow lite model and the second file is the object class labels. </param>
        /// <param name="downloadUrl">The url where the file can be downloaded</param>
        /// <param name="localModelFolder">The local folder to store the model</param>
        /// <param name="optDelegate">The optional delegate that can be used during model initialization</param>
        public override 
#if UNITY_EDITOR || UNITY_IOS || UNITY_ANDROID || UNITY_STANDALONE
            IEnumerator
#else
            async Task
#endif
            Init(
                String[] modelFiles = null, 
                String downloadUrl = null,
                String localModelFolder = "CocoSsdMobilenetV1",
                IDelegate optDelegate = null)
        {
            downloadUrl = ( downloadUrl == null ? "https://github.com/emgucv/models/raw/master/coco_ssd_mobilenet_v1_1.0_quant_2018_06_29/" : downloadUrl );
            modelFiles = (modelFiles == null ? new string[] { "detect.tflite", "labelmap.txt" } : modelFiles);

#if UNITY_EDITOR || UNITY_IOS || UNITY_ANDROID || UNITY_STANDALONE
            var e = base.Init(modelFiles, downloadUrl, localModelFolder, optDelegate);
            while(e.MoveNext())
                yield return e.Current;
#else
            await base.Init(modelFiles, downloadUrl, localModelFolder, optDelegate);
#endif

        }

    }
}
