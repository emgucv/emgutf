//----------------------------------------------------------------------------
//  Copyright (C) 2004-2021 by EMGU Corporation. All rights reserved.       
//----------------------------------------------------------------------------

using System;
using System.Collections.Generic;

namespace Emgu.TF
{
    public partial class TfInvoke
    {
        /// <summary>
        /// The file name of the tfextern library
        /// </summary>
#if (__IOS__ || UNITY_IPHONE) && (!UNITY_EDITOR)
        public const string ExternLibrary = "__Internal";
#else
        public const string ExternLibrary = "tfextern";
#endif

        /// <summary>
        /// The List of the tensorflow modules
        /// </summary>
        public static List<String> TensorflowModuleList = new List<String>
        {
            ExternLibrary
        };
    }
}
