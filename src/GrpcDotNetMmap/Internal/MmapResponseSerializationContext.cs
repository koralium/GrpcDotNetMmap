using Grpc.Core;
using System;
using System.Buffers;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GrpcDotNetMmap.Internal
{
    internal class MmapResponseSerializationContext : SerializationContext
    {
        private readonly MmapStream _stream;
        private readonly Memory<byte> _payloadMemory;
        public MmapResponseSerializationContext(MmapStream stream)
        {
            _stream = stream;
            _payloadMemory = stream.Memory.Slice(4);
        }

        public override void Complete()
        {
        }

        public override IBufferWriter<byte> GetBufferWriter()
        {
            return new MmapBufferWriter(_payloadMemory);
        }

        public override void SetPayloadLength(int payloadLength)
        {
            _stream.Accessor.Write(0, payloadLength);
        }
    }
}
