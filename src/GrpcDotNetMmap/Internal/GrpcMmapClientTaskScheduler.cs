using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Collections.Concurrent;
using GrpcDotNetMmap.Internal;

namespace GrpcDotNetMmap.Internal
{
    internal class GrpcMmapClientTaskScheduler : TaskScheduler, IDisposable
    {
        private SemaphoreSlim _semaphore;
        ConcurrentQueue<Task> cq = new ConcurrentQueue<Task>();
        private CancellationTokenSource _cancellationTokenSource;
        private MmapGrpcTransportClient transport;
        private Thread workingThread;
        private bool disposedValue;

        public GrpcMmapClientTaskScheduler(string name, bool isMemoryCreator, int capacity)
        {
            _semaphore = new SemaphoreSlim(0);
            _cancellationTokenSource = new CancellationTokenSource();
            workingThread = new Thread(() => WorkerLoop(name, isMemoryCreator, capacity))
            { IsBackground = false };
            workingThread.Start();
        }

        private void WorkerLoop(string name, bool isMemoryCreator, int capacity)
        {
            // Create the transport on the worker thread, to have all mutexes on that thread only
            try
            {
                transport = new MmapGrpcTransportClient(name, isMemoryCreator, capacity);
                while (!_cancellationTokenSource.IsCancellationRequested)
                {
                    _semaphore.Wait(_cancellationTokenSource.Token);

                    if (cq.TryDequeue(out var task))
                    {
                        TryExecuteTask(task);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
            
        }

        protected override IEnumerable<Task>? GetScheduledTasks()
        {
            throw new NotImplementedException();
        }

        public Task<TResponse> QueueGrpc<TResponse>(Func<MmapGrpcTransportClient, TResponse> func, CancellationToken cancellationToken)
        {
            return Task.Factory.StartNew(() =>
            {
                return func(transport);
            }, cancellationToken, TaskCreationOptions.None, this);
        }

        protected override void QueueTask(Task task)
        {
            cq.Enqueue(task);
            _semaphore.Release();
        }

        protected override bool TryExecuteTaskInline(Task task, bool taskWasPreviouslyQueued)
        {
            return false;
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                _cancellationTokenSource.Cancel();
                transport.Dispose();
                disposedValue = true;
            }
        }

        ~GrpcMmapClientTaskScheduler()
        {
            Dispose(disposing: false);
        }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
