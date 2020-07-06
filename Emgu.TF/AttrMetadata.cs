//----------------------------------------------------------------------------
//  Copyright (C) 2004-2020 by EMGU Corporation. All rights reserved.       
//----------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Emgu.TF.Util;
using System.Runtime.InteropServices;

namespace Emgu.TF
{
    /// <summary>
    /// Describes the value of an attribute on an operation.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct AttrMetadata
    {
        /// <summary>
        /// True if the attribute value is a list, false otherwise.
        /// </summary>
        [MarshalAs(TfInvoke.BoolMarshalType)]
        public bool IsList;

        /// <summary>
        /// Length of the list if is_list is true. Undefined otherwise.
        /// </summary>
        public Int64 ListSize;

        /// <summary>
        /// Type of elements of the list if is_list != 0.
        /// Type of the single value stored in the attribute if is_list == 0.
        /// </summary>
        public AttrType Type;

        /// <summary>
        /// Total size the attribute value.
        /// The units of total_size depend on is_list and type.
        /// (1) If type == TF_ATTR_STRING and is_list == 0
        ///     then total_size is the byte size of the string
        ///     valued attribute.
        /// (2) If type == TF_ATTR_STRING and is_list == 1
        ///     then total_size is the cumulative byte size
        ///     of all the strings in the list.
        /// (3) If type == TF_ATTR_SHAPE and is_list == 0
        ///     then total_size is the number of dimensions
        ///     of the shape valued attribute, or -1
        ///     if its rank is unknown.
        /// (4) If type == TF_ATTR_SHAPE and is_list == 1
        ///     then total_size is the cumulative number
        ///     of dimensions of all shapes in the list.
        /// (5) Otherwise, total_size is undefined.
        /// </summary>
        public Int64 TotalSize;

    }
}
