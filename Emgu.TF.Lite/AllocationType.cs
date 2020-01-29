//----------------------------------------------------------------------------
//  Copyright (C) 2004-2020 by EMGU Corporation. All rights reserved.       
//----------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using Emgu.TF.Util;

namespace Emgu.TF.Lite
{
    /// <summary>
    /// Memory allocation strategies.
    /// </summary>
    public enum AllocationType
    {
        /// <summary>
        /// None
        /// </summary>
        MemNone = 0,
        /// <summary>
        ///  Read-only memory-mapped data (or data externally allocated).
        /// </summary>
        MmapRo,
        /// <summary>
        /// Arena allocated data
        /// </summary>
        ArenaRw,
        /// <summary>
        /// Arena allocated persistent data
        /// </summary>
        ArenaRwPersistent,
        /// <summary>
        /// Tensors that are allocated during evaluation
        /// </summary>
        Dynamic,
    }

}
