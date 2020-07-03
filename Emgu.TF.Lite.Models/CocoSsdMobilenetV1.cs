//----------------------------------------------------------------------------
//  Copyright (C) 2004-2020 by EMGU Corporation. All rights reserved.       
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

        public override 
#if UNITY_EDITOR || UNITY_IOS || UNITY_ANDROID || UNITY_STANDALONE
            IEnumerator
#else
            async Task
#endif
            Init(
                String[] modelFiles = null, 
                String downloadUrl = null,
                String localModelFolder = "CocoSsdMobilenetV1")
        {
            downloadUrl = ( downloadUrl == null ? "https://github.com/emgucv/models/raw/master/coco_ssd_mobilenet_v1_1.0_quant_2018_06_29/" : downloadUrl );
            modelFiles = (modelFiles == null ? new string[] { "detect.tflite", "labelmap.txt" } : modelFiles);

#if UNITY_EDITOR || UNITY_IOS || UNITY_ANDROID || UNITY_STANDALONE
            var e = base.Init(modelFiles, downloadUrl, localModelFolder);
            while(e.MoveNext())
                yield return e.Current;
#else
            await base.Init(modelFiles, downloadUrl, localModelFolder);
#endif

        }

    }
}
