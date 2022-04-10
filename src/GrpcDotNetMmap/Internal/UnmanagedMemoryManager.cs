using System;
using System.Buffers;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace GrpcDotNetMmap.Internal
{
    public unsafe class UnmanagedMemoryManager<T> : MemoryManager<T>
    {
        readonly int _elementCount;
        readonly byte* _ptr;

        public UnmanagedMemoryManager(byte* ptr, int count)
        {
            _ptr = ptr;
            _elementCount = count;

            //_ptr = Marshal.AllocHGlobal(elementCount * Unsafe.SizeOf<T>());
        }

        protected override void Dispose(bool disposing)
        {
            //Marshal.FreeHGlobal(_ptr);
        }

        public override Span<T> GetSpan()
        {
            return new Span<T>((void*)_ptr, _elementCount);
        }

        public override MemoryHandle Pin(int elementIndex)
        {
            throw new Exception();
        }

        public override void Unpin()
        {
            throw new Exception();
        }
    }
}
