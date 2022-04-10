using System;
using System.Collections.Generic;
using System.IO.MemoryMappedFiles;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GrpcDotNetMmap.Internal
{
    internal unsafe class MmapGrpcTransport : IDisposable
    {
        protected readonly MmapStream _request_stream;
        protected readonly MmapStream _response_stream;
        private bool disposedValue;

        public MmapGrpcTransport(string name, bool isCreator, bool isServer, int capacity)
        {
            _request_stream = new MmapStream($"{name}_request", isCreator, isServer, capacity);
            _response_stream = new MmapStream($"{name}_response", isCreator, !isServer, capacity);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                _request_stream.Dispose();
                _response_stream.Dispose();
                disposedValue = true;
            }
        }

        ~MmapGrpcTransport()
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
