using Grpc.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GrpcDotNetMmap.Internal
{
    internal class MmapGrpcTransportClient : MmapGrpcTransport
    {
        private MmapRequestDeserializationContext _deserializationContext;
        public MmapGrpcTransportClient(string name, bool isCreator, int capacity) : base(name, isCreator, false, capacity)
        {
            _deserializationContext = new MmapRequestDeserializationContext();
        }

        public SerializationContext BeginRequest(string name)
        {
            _request_stream.BeginWrite();
            int written = Encoding.UTF8.GetBytes(name, _request_stream.Memory.Span.Slice(4, _request_stream.Memory.Length - 4));
            _request_stream.Accessor.Write(0, written);
            var payloadMemory = _request_stream.Memory.Slice(8 + written);
            return new MmapSerializationContext((size) => _request_stream.Accessor.Write(4 + written, size), payloadMemory);
        }

        public void EndRequest()
        {
            _request_stream.SendMessage();
        }

        public DeserializationContext WaitForResponse()
        {
            _response_stream.BeginRead();
            var size = _response_stream.Accessor.ReadInt32(0);
            _deserializationContext.SetMemory(_response_stream.Memory.Slice(4, size));
            return _deserializationContext;
        }

        public void DoneReadingResponse()
        {
            _response_stream.EndRead();
        }
    }
}
