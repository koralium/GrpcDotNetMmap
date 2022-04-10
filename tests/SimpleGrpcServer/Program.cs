using GrpcDotNetMmap;
using GrpcDotNetNamedPipes;
using SimpleGrpcServer;
using SimpleGrpcServer.Services;

namespace SimpleGrpcServer
{

    public class Program
    {
        public static void Run(string arg)
        {
            if (arg == "mmap")
            {
                var server = new MmapGrpcServer("testmmap", true, 1000000);
                Greeter.BindService(server.ServiceBinder, new GreeterService());
                server.Run();
            }
            else if (arg == "pipe")
            {
                var server = new NamedPipeServer("testpipe");
                Greeter.BindService(server.ServiceBinder, new GreeterService());
                server.Start();
            }
        }

        public static void Main(params string[] args)
        {
            Run(args.Length > 0 ? args[0] : "mmap");
        }
    }
}