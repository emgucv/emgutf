//----------------------------------------------------------------------------
//  Copyright (C) 2004-2018 by EMGU Corporation. All rights reserved.       
//----------------------------------------------------------------------------

using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;

namespace Emgu.TF.Lite
{
    public interface IOpResolver
    {
        IntPtr OpResolverPtr { get; }
    }
}
