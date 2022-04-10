using Grpc.Core;
using System;
using System.Buffers;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GrpcDotNetMmap.Internal
{
    internal class MmapRequestDeserializationContext : DeserializationContext
    {
        private Memory<byte> _memory = Memory<byte>.Empty;
        public MmapRequestDeserializationContext()
        {
        }

        public override int PayloadLength => _memory.Length;

        internal void SetMemory(in Memory<byte> memory)
        {
            _memory = memory;
        }

        public override ReadOnlySequence<byte> PayloadAsReadOnlySequence()
        {
            return new ReadOnlySequence<byte>(_memory);
        }
    }
}
