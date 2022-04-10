using Grpc.Core;
using System;
using System.Buffers;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GrpcDotNetMmap.Internal
{
    internal class MmapSerializationContext : SerializationContext
    {
        private IBufferWriter<byte> _writer;
        private readonly Action<int> sizeWriter;
        public MmapSerializationContext(Action<int> sizeWriter, Memory<byte> payloadMemory)//MmapGrpcResponseMessageBuilder messageBuilder)
        {
            this.sizeWriter = sizeWriter;
            _writer = new MmapBufferWriter(payloadMemory);
        }

        public override void Complete()
        {
        }

        public override IBufferWriter<byte> GetBufferWriter()
        {
            return _writer;
        }

        public override void SetPayloadLength(int payloadLength)
        {
            sizeWriter(payloadLength);
        }
    }
}
