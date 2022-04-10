using System.Buffers;

namespace GrpcDotNetMmap.Internal
{
    internal class MmapBufferWriter : IBufferWriter<byte>
    {
        private Memory<byte> memory;
        private int _position;
        
        public MmapBufferWriter(Memory<byte> memory)
        {
            this.memory = memory;
        }

        public void Advance(int count)
        {
            _position += count;
        }

        public Memory<byte> GetMemory(int sizeHint = 0)
        {

            return memory.Slice(_position);
        }

        public Span<byte> GetSpan(int sizeHint = 0)
        {
            return memory.Span.Slice(_position);
        }
    }
}
