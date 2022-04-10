using Grpc.Core;
using GrpcDotNetMmap.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GrpcDotNetMmap
{
    public class MmapGrpcServer : IDisposable
    {
        private readonly Dictionary<string, Func<RequestContext, Task>> _methodHandlers =
                new Dictionary<string, Func<RequestContext, Task>>();
        private readonly MmapGrpcTransportServer _serverTransport;
        private bool disposedValue;

        public ServiceBinderBase ServiceBinder { get; }

        public MmapGrpcServer(string name, bool isMemoryCreator, int capacity)
        {
            _serverTransport = new MmapGrpcTransportServer(name, isMemoryCreator, capacity);
            ServiceBinder = new ServiceBinderImpl(this);
        }

        public void Run()
        {
            while (true)
            {
                var request = _serverTransport.WaitForRequest();
             
                if (_methodHandlers.TryGetValue(request.FullName, out var handler))
                {
                    handler(request);
                }
            }
        }

        private class ServiceBinderImpl : ServiceBinderBase
        {
            private readonly MmapGrpcServer _server;

            public ServiceBinderImpl(MmapGrpcServer server)
            {
                _server = server;
            }

            public override void AddMethod<TRequest, TResponse>(Method<TRequest, TResponse> method,
                UnaryServerMethod<TRequest, TResponse> handler)
            {
                _server._methodHandlers.Add(method.FullName, async ctx =>
                {
                    try
                    {
                        var request = method.RequestMarshaller.ContextualDeserializer(ctx.DeserializationContext);
                        

                        var response = await handler(request, ctx.CallContext).ConfigureAwait(false);

                        var resp = ctx.BeginResponse();
                        method.ResponseMarshaller.ContextualSerializer(response, resp);
                        ctx.DoneReadingRequest();
                        ctx.SendResponse();
                    }
                    catch (Exception ex)
                    {
                        ctx.Error(ex);
                    }
                });
            }
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                _serverTransport.Dispose();
                disposedValue = true;
            }
        }

        ~MmapGrpcServer()
        {
            Dispose(disposing: false);
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
