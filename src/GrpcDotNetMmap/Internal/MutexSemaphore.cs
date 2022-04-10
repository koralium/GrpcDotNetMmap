using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GrpcDotNetMmap.Internal
{
    internal class MutexSemaphore : IDisposable
    {
        private readonly string _name;
        private readonly Func<int> getCount;
        private readonly Action<int> setCount;
        private readonly Mutex _data_mutex;
        private readonly Mutex _signal_mutex;
        private bool disposedValue;

        public MutexSemaphore(string name, bool isMutexCreator, Func<int> getCount, Action<int> setCount, bool isSignaler)
        {
            _name = name;
            this.getCount = getCount;
            this.setCount = setCount;
            if (isMutexCreator)
            {
                _data_mutex = new Mutex(false, $"{name}_data", out var signalCreated);
                _signal_mutex = new Mutex(isSignaler, $"{name}_signal", out var ackCreated);
            }
            else
            {
                _data_mutex = Mutex.OpenExisting($"{name}_data");
                _signal_mutex = Mutex.OpenExisting($"{name}_signal");

                if (isSignaler)
                {
                    _signal_mutex.WaitOne();
                }
            }
        }

        // Called when waiting for a signal
        public void WaitForRead()
        {
            try
            {
                _signal_mutex.WaitOne();
                _signal_mutex.ReleaseMutex();
            }
            catch
            {
                //TODO: Add logging here that connection was lost
            }
            
            // Wait for signal
            _data_mutex.WaitOne();

            while (getCount() == 0)
            {
               
                _data_mutex.ReleaseMutex();
                _data_mutex.WaitOne();
            }
            
        }

        public void DoneRead()
        {
            setCount(0);
            _data_mutex.ReleaseMutex();
        }

        public void Signal()
        {
            _data_mutex.WaitOne();
            setCount(1);
            _data_mutex.ReleaseMutex();

            // Send the signal
            _signal_mutex.ReleaseMutex();

            _data_mutex.WaitOne();
            while (getCount() != 0)
            {
                _data_mutex.ReleaseMutex();
                _data_mutex.WaitOne();
            }
            _data_mutex.ReleaseMutex();

            _signal_mutex.WaitOne();
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                _data_mutex.Dispose();
                _signal_mutex.Dispose();
                disposedValue = true;
            }
        }
        ~MutexSemaphore()
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
