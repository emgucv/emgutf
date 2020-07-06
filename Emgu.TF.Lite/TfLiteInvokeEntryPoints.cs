//----------------------------------------------------------------------------
//  Copyright (C) 2004-2020 by EMGU Corporation. All rights reserved.       
//----------------------------------------------------------------------------

using System;
using System.Collections.Generic;

namespace Emgu.TF.Lite
{
    public partial class TfLiteInvoke
    {
        /// <summary>
        /// The file name of the tfliteextern library
        /// </summary>
#if (__IOS__ || UNITY_IPHONE) && (!UNITY_EDITOR)
        public const string ExternLibrary = "__Internal";
#else
        public const string ExternLibrary = "tfliteextern";
#endif

        /// <summary>
        /// The List of the tensorflow lite modules
        /// </summary>
        public static List<String> TensorflowModuleList = new List<String>
        {
            ExternLibrary
        };

    }
}
