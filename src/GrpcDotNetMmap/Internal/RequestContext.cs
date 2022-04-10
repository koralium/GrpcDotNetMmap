using Grpc.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GrpcDotNetMmap.Internal
{
    internal class RequestContext
    {
        private readonly MmapGrpcTransportServer _transport;
        public string FullName { get; }

        public ServerCallContext CallContext { get; }

        public DeserializationContext DeserializationContext => _transport.DeserializationContext;

        public RequestContext(in string fullName, in MmapGrpcTransportServer transport)
        {
            FullName = fullName;
            _transport = transport;
        }

        public void DoneReadingRequest()
        {
            _transport.DoneReadingRequest();
        }

        public SerializationContext BeginResponse()
        {
            return _transport.BeginWriteResponse();
        }

        public void SendResponse()
        {
            _transport.EndWriteResponse();
        }

        public void Error(Exception e)
        {
            //TODO: Implement
        }
    }
}
