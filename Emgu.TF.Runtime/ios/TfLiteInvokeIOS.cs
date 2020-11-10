//----------------------------------------------------------------------------
//  Copyright (C) 2004-2020 by EMGU Corporation. All rights reserved.       
//----------------------------------------------------------------------------


using System;
using Emgu.TF.Lite;

namespace Emgu.TF
{
   /// <summary>
   /// TfInvoke for iOS
   /// </summary>
   public static class TfLiteInvokeIOS
   {
      /// <summary>
      /// Return true if the class is loaded.
      /// </summary>
      public static bool CheckLibraryLoaded ()
      {
         return TfLiteInvoke.CheckLibraryLoaded ();
      }
   }
}
