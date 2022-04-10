Test project to evaluate performance of using MMap instead of named pipes for gRPC when doing IPC.

This solution only supports unary gRPC requests.

## Benchmark

|     Method |   Size |      Mean |     Error |    StdDev |    Median |
|----------- |------- |----------:|----------:|----------:|----------:|
|       Mmap |    100 |  36.53 us |  0.461 us |  0.360 us |  36.57 us |
| NamedPipes |    100 | 201.87 us |  9.232 us | 27.076 us | 204.14 us |
|       Mmap |   1000 |  50.01 us |  0.836 us |  0.782 us |  49.55 us |
| NamedPipes |   1000 | 211.56 us |  9.155 us | 26.995 us | 219.49 us |
|       Mmap |  10000 |  52.78 us |  0.708 us |  1.478 us |  52.67 us |
| NamedPipes |  10000 | 248.78 us |  8.715 us | 25.423 us | 256.82 us |
|       Mmap | 100000 | 372.65 us |  7.153 us | 17.946 us | 371.41 us |
| NamedPipes | 100000 | 795.22 us | 29.218 us | 86.151 us | 806.57 us |