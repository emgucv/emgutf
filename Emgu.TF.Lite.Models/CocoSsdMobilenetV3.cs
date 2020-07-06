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
    public class CocoSsdMobilenetV3 : CocoSsdMobilenet
    {
        public CocoSsdMobilenetV3()
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
                String localModelFolder = "CocoSsdMobilenetV3")
        {

            downloadUrl = (downloadUrl == null
                ? "https://github.com/emgucv/models/raw/master/coco_ssd_mobilenet_v3_small_2010_01_14/"
                : downloadUrl);
            modelFiles = (modelFiles == null ? new string[] {"model.tflite", "labelmap.txt"} : modelFiles);
#if UNITY_EDITOR || UNITY_IOS || UNITY_ANDROID || UNITY_STANDALONE
            var e = base.Init(modelFiles, downloadUrl, localModelFolder);
            while (e.MoveNext())
                yield return e.Current;
#else
            await base.Init(modelFiles, downloadUrl, localModelFolder);
#endif
        }
    }
}
