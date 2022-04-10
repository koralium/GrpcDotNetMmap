using System;
using System.Collections.Generic;
using System.IO.MemoryMappedFiles;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GrpcDotNetMmap.Internal
{
    internal unsafe class MmapStream : IDisposable
    {
        private readonly bool _isReader;
        private readonly MemoryMappedFile _mmap;
        protected readonly MemoryMappedViewAccessor _accessor;
        byte* ptr = null;
        private bool disposedValue;
        private readonly int _capacity;
        private readonly MutexSemaphore semaphore;

        public MmapStream(string name, bool isCreator, bool isReader, int capacity)
        {
            _capacity = capacity;
            _isReader = isReader;

            // Create or open the data
            if (isCreator)
            {
                _mmap = MMap.Create(name, capacity + 1);
            }
            else
            {
                _mmap = MMap.Open(name);
            }
            // Semaphore uses the last address in the mmap for signaling purposes
            semaphore = new MutexSemaphore(name, isCreator, () => Accessor.ReadInt32(_capacity), (i) => Accessor.Write(_capacity, i), !isReader);
            _accessor = _mmap.CreateViewAccessor();
            _accessor.SafeMemoryMappedViewHandle.AcquirePointer(ref ptr);
            Memory = new UnmanagedMemoryManager<byte>(ptr, capacity).Memory;
        }

        public void BeginRead()
        {
            semaphore.WaitForRead();
        }

        public void EndRead()
        {
            semaphore.DoneRead();
        }

        public void BeginWrite()
        {
        }

        public void SendMessage()
        {
            semaphore.Signal();
        }

        public Memory<byte> Memory { get; }

        public UnmanagedMemoryAccessor Accessor => _accessor;

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                _accessor.Dispose();
                _mmap.Dispose();
                semaphore.Dispose();
                disposedValue = true;
            }
        }

        ~MmapStream()
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