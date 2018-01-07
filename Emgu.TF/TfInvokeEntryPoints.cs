//----------------------------------------------------------------------------
//  Copyright (C) 2004-2018 by EMGU Corporation. All rights reserved.       
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
#if UNITY_EDITOR_OSX
      public const string ExternLibrary = "Assets/Emgu.TF/Plugins/emgutf.bundle/Contents/MacOS/libtfextern.dylib";
#elif UNITY_STANDALONE_OSX
      public const string ExternLibrary = "@executable_path/../Plugins/emgutf.bundle/Contents/MacOS/libtfextern.dylib";
#elif (__IOS__ || UNITY_IPHONE) && (!UNITY_EDITOR_WIN)
      public const string ExternLibrary = "__Internal";
#elif (!__IOS__) && __UNIFIED__
      public const string ExternLibrary = "libtfextern.dylib";
#else
      public const string ExternLibrary = "tfextern";
#endif
	  	  
      /// <summary>
      /// The List of the opencv modules
      /// </summary>
	  public static List<String> TensorflowModuleList = new List<String>
	  {       
        ExternLibrary
      };

	  
   }
}
