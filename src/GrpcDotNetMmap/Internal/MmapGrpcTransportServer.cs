using Grpc.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GrpcDotNetMmap.Internal
{
    internal class MmapGrpcTransportServer : MmapGrpcTransport
    {
        private MmapResponseSerializationContext _responseSerializationContext;
        private MmapRequestDeserializationContext _requestDeserializationContext;
        public MmapGrpcTransportServer(string name, bool isCreator, int capacity) : base(name, isCreator, true, capacity)
        {
            _responseSerializationContext = new MmapResponseSerializationContext(_response_stream);
            _requestDeserializationContext = new MmapRequestDeserializationContext();
        }

        public RequestContext WaitForRequest()
        {
            _request_stream.BeginRead();
            var nameSize = _request_stream.Accessor.ReadInt32(0);
            var requestName = Encoding.UTF8.GetString(_request_stream.Memory.Span.Slice(4, nameSize));
            var payloadSize = _request_stream.Accessor.ReadInt32(4 + nameSize);
            _requestDeserializationContext.SetMemory(_request_stream.Memory.Slice(nameSize + 8, payloadSize));
            return new RequestContext(requestName, this);
        }

        internal DeserializationContext DeserializationContext => _requestDeserializationContext;

        public void DoneReadingRequest()
        {
            _request_stream.EndRead();
        }

        public MmapResponseSerializationContext BeginWriteResponse()
        {
            _response_stream.BeginWrite();
            return _responseSerializationContext;
        }

        public void EndWriteResponse()
        {
            _response_stream.SendMessage();
        }
    }
}
