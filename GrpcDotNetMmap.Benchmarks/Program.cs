using BenchmarkDotNet.Running;


namespace GrpcDotNetMmap.Benchmarks
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            //var b = new UnaryBenchmark();
            //await b.Mmap();
            //b.Dispose();
            var summary = BenchmarkRunner.Run(typeof(Program).Assembly);
        }
    }
}