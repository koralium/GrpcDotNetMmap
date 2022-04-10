using BenchmarkDotNet.Attributes;
using GrpcDotNetNamedPipes;
using SimpleGrpcServer;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GrpcDotNetMmap.Benchmarks
{
    public class UnaryBenchmark : IDisposable
    {
        private static IDictionary<int, string> parameterValues = GenerateParamterValues();

        private static IDictionary<int, string> GenerateParamterValues()
        {
            Dictionary<int, string> output = new Dictionary<int, string>();
            output.Add(100, GenerateString(100));
            output.Add(1000, GenerateString(1000));
            output.Add(10000, GenerateString(10000));
            output.Add(100000, GenerateString(100000));
            return output;
        }

        private static string GenerateString(int nr)
        {
            StringBuilder output = new StringBuilder();

            for (int i = 0; i < nr; i++)
            {
                output.Append("a");
            }
            return output.ToString();
        }

        IDisposable channel;
        private Greeter.GreeterClient client;
        private Process process;
        public UnaryBenchmark()
        {
            
        }

        public void Dispose()
        {
            process.Kill(true);
            channel?.Dispose();
        }

        [GlobalSetup(Target = nameof(Mmap))]
        public void SetupMmap()
        {
            var currentDir = Directory.GetCurrentDirectory();

            if (Environment.OSVersion.Platform == PlatformID.Unix)
            {
                currentDir += "/bin/Debug/net6.0";
            }

            process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "dotnet",
                    Arguments = currentDir + "/SimpleGrpcServer.dll mmap",
                    UseShellExecute = false,
                    RedirectStandardOutput = false,
                    RedirectStandardError = true,
                    CreateNoWindow = true,
                }
            };

            process.Start();

            Thread.Sleep(1000);

            var mmapChannel = new MmapGrpcChannel("testmmap", false, 1000000);
            client = new Greeter.GreeterClient(mmapChannel);
            channel = mmapChannel;
        }

        [GlobalSetup(Target = nameof(NamedPipes))]
        public void Setup()
        {
            var currentDir = Directory.GetCurrentDirectory();

            if (Environment.OSVersion.Platform == PlatformID.Unix)
            {
                currentDir += "/bin/Debug/net6.0";
            }

            process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "dotnet",
                    Arguments = currentDir + "/SimpleGrpcServer.dll pipe",
                    UseShellExecute = false,
                    RedirectStandardOutput = false,
                    RedirectStandardError = true,
                    CreateNoWindow = true,
                }
            };

            process.Start();

            Thread.Sleep(1000);

            var pipeChannel = new NamedPipeChannel(".", "testpipe");
            client = new Greeter.GreeterClient(pipeChannel);
        }

        [Params(100, 1000, 10000, 100000)]
        public int Size { get; set; }


        [Benchmark()]
        public async Task Mmap()
        {

            
            await client.SayHelloAsync(new HelloRequest()
            {
                Name = parameterValues[Size]
            });
        }

        [Benchmark()]
        public async Task NamedPipes()
        {
            await client.SayHelloAsync(new HelloRequest()
            {
                Name = parameterValues[Size]
            });
        }
    }
}
        