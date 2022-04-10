using System.IO.MemoryMappedFiles;

namespace GrpcDotNetMmap.Internal
{
    public class MMap
    {
        public static MemoryMappedFile Create(string name, long capacity)
        {

            MemoryMappedFile? mmap = null;
            if (Environment.OSVersion.Platform == PlatformID.Unix)
            {
                var tempDirectory = Path.GetTempPath();
                mmap = MemoryMappedFile.CreateFromFile($"{tempDirectory}/{name}", FileMode.OpenOrCreate, null, capacity);
            }
            else if (Environment.OSVersion.Platform == PlatformID.Win32NT)
            {
                mmap = MemoryMappedFile.CreateOrOpen(name, capacity);
            }
            else
            {
                throw new NotSupportedException();
            }
            return mmap;
        }

        public static MemoryMappedFile Open(string name)
        {
            MemoryMappedFile? mmap = null;
            if (Environment.OSVersion.Platform == PlatformID.Unix)
            {
                var tempDirectory = Path.GetTempPath();
                mmap = MemoryMappedFile.CreateFromFile($"{tempDirectory}/{name}", FileMode.Open);
            }
            else if (Environment.OSVersion.Platform == PlatformID.Win32NT)
            {
                mmap = MemoryMappedFile.OpenExisting(name);
            }
            else
            {
                throw new NotSupportedException();
            }
            return mmap;
        }
    }
}