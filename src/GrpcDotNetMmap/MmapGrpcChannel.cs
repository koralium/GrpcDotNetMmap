using Grpc.Core;
using GrpcDotNetMmap.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GrpcDotNetMmap
{
    public class MmapGrpcChannel : CallInvoker, IDisposable
    {
        private readonly GrpcMmapClientTaskScheduler _grpcMmapClientTaskScheduler;
        private bool disposedValue;

        public MmapGrpcChannel(string name, bool isMemoryCreator, int capacity)
        {
            _grpcMmapClientTaskScheduler = new GrpcMmapClientTaskScheduler(name, isMemoryCreator, capacity);
        }

        public override TResponse BlockingUnaryCall<TRequest, TResponse>(Method<TRequest, TResponse> method, string? host, CallOptions options, TRequest request)
        {
            // Start request
            return _grpcMmapClientTaskScheduler.QueueGrpc<TResponse>((transport) =>
            {
                var serializationContext = transport.BeginRequest(method.FullName);
                method.RequestMarshaller.ContextualSerializer(request, serializationContext);
                transport.EndRequest();

                // Start response
                var deserializeContext = transport.WaitForResponse();
                var resp = method.ResponseMarshaller.ContextualDeserializer(deserializeContext);
                transport.DoneReadingResponse();
                return resp;
            }, default).Result;
        }

        public override AsyncUnaryCall<TResponse> AsyncUnaryCall<TRequest, TResponse>(Method<TRequest, TResponse> method, string? host, CallOptions options, TRequest request)
        {
            CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
            return new AsyncUnaryCall<TResponse>(
                _grpcMmapClientTaskScheduler.QueueGrpc<TResponse>((transport) =>
                {
                    var serializationContext = transport.BeginRequest(method.FullName);
                    method.RequestMarshaller.ContextualSerializer(request, serializationContext);
                    transport.EndRequest();

                    // Start response
                    var deserializeContext = transport.WaitForResponse();
                    var resp = method.ResponseMarshaller.ContextualDeserializer(deserializeContext);
                    transport.DoneReadingResponse();
                    return resp;
                }, cancellationTokenSource.Token),
                Task.FromResult<Metadata>(default),
                () => new Status(),
                () => new Metadata(),
                () => cancellationTokenSource.Cancel()
                );
        }

        public override AsyncClientStreamingCall<TRequest, TResponse> AsyncClientStreamingCall<TRequest, TResponse>(Method<TRequest, TResponse> method, string? host, CallOptions options)
        {
            throw new NotImplementedException();
        }

        public override AsyncDuplexStreamingCall<TRequest, TResponse> AsyncDuplexStreamingCall<TRequest, TResponse>(Method<TRequest, TResponse> method, string? host, CallOptions options)
        {
            throw new NotImplementedException();
        }

        public override AsyncServerStreamingCall<TResponse> AsyncServerStreamingCall<TRequest, TResponse>(Method<TRequest, TResponse> method, string? host, CallOptions options, TRequest request)
        {
            throw new NotImplementedException();
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                _grpcMmapClientTaskScheduler.Dispose();
                disposedValue = true;
            }
        }

        ~MmapGrpcChannel()
        {
            Dispose(disposing: false);
        }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
