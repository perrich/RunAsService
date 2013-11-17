using System;

namespace Perrich.RunAsService.Process
{
    public class ProcessWrapper : IProcessWrapper
    {
        private readonly System.Diagnostics.Process _process;

        public event EventHandler Exited;

        protected bool Disposed
        {
            get;
            private set;
        }

        public bool IsStarted
        {
            get
            {
                if (Disposed)
                {
                    throw new ObjectDisposedException(GetType().Name);
                }
                return !_process.HasExited && _process.Id != 0;
            }
        }

        public int Id
        {
            get
            {
                if (Disposed)
                {
                    throw new ObjectDisposedException(GetType().Name);
                }
                return _process.Id;
            }
        }

        public ProcessWrapper(System.Diagnostics.Process process)
        {
            _process = process;
            _process.Exited += ProcessExited;
        }

        public bool Start()
        {
            if (Disposed)
            {
                throw new ObjectDisposedException(GetType().Name);
            }
            return _process.Start();
        }

        public void Kill()
        {
            if (Disposed)
            {
                throw new ObjectDisposedException(GetType().Name);
            }
            _process.Kill();
        }

        public void Dispose()
        {
            _process.Exited -= ProcessExited;
            _process.Dispose();
        }

        protected virtual void Dispose(bool disposing)
        {
            // Multiple Dispose calls should be OK.
            if (!Disposed)
            {
                if (disposing)
                {
                    Dispose();
                }

                Disposed = true;
            }
        }

        private void ProcessExited(object sender, EventArgs e)
        {
            EventHandler handler = Exited;
            if (handler != null)
            {
                handler(sender, e);
            }
        }
    }
}
