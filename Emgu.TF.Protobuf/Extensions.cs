#pragma warning disable 1591, 0612, 3021


using pb = global::Google.Protobuf;
using pbc = global::Google.Protobuf.Collections;
using pbr = global::Google.Protobuf.Reflection;
using scg = global::System.Collections.Generic;

using System;
using System.IO;

namespace Tensorflow
{
    public sealed partial class ConfigProto : pb::IMessage<ConfigProto>
    {
        public byte[] ToProtobuf()
        {
            using (MemoryStream ms = new MemoryStream())
            using (pb::CodedOutputStream stream = new pb::CodedOutputStream(ms))
            {
                WriteTo(stream);
                stream.Flush();
                return ms.ToArray();
            }
        }
    }

}


