//----------------------------------------------------------------------------
//  Copyright (C) 2004-2022 by EMGU Corporation. All rights reserved.       
//----------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using Emgu.TF.Util;

namespace Emgu.TF
{

    public interface ILogSink
    {
        IntPtr LogSinkPtr { get; }
    }

}
