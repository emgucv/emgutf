//----------------------------------------------------------------------------
//  Copyright (C) 2004-2024 by EMGU Corporation. All rights reserved.       
//----------------------------------------------------------------------------

#if __IOS__

using System;
using ObjCRuntime;

[assembly: LinkWith (
   "libtfliteextern.a", 
   LinkTarget.ArmV7 | LinkTarget.Simulator | LinkTarget.Arm64 | LinkTarget.Simulator64, 
   ForceLoad = true,
   SmartLink = true, 
   Frameworks="Foundation Accelerate CoreFoundation CoreGraphics AssetsLibrary AVFoundation CoreImage CoreMedia CoreVideo QuartzCore ImageIO Metal", 
   LinkerFlags = "-stdlib=libc++ -ObjC -lc++", 
   IsCxx=true)]

#endif