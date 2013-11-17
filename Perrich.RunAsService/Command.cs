using System;
using log4net;
using Perrich.RunAsService.Process;

namespace Perrich.RunAsService
{
    public class Command : ICommand
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(Command));

        private readonly IProcessManager _manager;
        private readonly string _executable;
        private readonly string _name;
        private readonly string _parameters;
        private readonly bool _killChildren;

        private IProcessWrapper _process;

        protected bool Disposed
        {
            get;
            private set;
        }

        /// <summary>
        /// The command name
        /// </summary>
        public string Name
        {
            get { return _name; }
        }

        /// <summary>
        /// The executable path
        /// </summary>
        public string Executable
        {
            get { return _executable; }
        }

        /// <summary>
        /// The executable parameters 
        /// </summary>
        public string Parameters
        {
            get { return _parameters; }
        }

        /// <summary>
        /// Do we need to kill process launched by this command ?
        /// </summary>
        public bool KillChildren
        {
            get { return _killChildren; }
        }
        
        /// <summary>
        /// Create a service's command.
        /// </summary>
        /// <param name="manager">Process manager</param>
        /// <param name="name">Descriptive name of the command</param>
        /// <param name="executable">Fullpath to the _executable/command that is run as service</param>
        /// <param name="parameters">List of commandline parameters</param>
        /// <param name="killChildren">All chidren should be killed when the command exited</param>
        public Command(IProcessManager manager, string name, string executable, string parameters, bool killChildren)
        {
            _manager = manager;
            _name = name;
            _executable = executable;
            _parameters = parameters;
            _killChildren = killChildren;

            if (Log.IsDebugEnabled)
            {
                Log.Debug(string.Format("Create new command {0}", this));
            }
        }

        #region IDisposable Members
        public void Dispose()
        {
            if (Disposed) return;

            Clean();
            Disposed = true;
        }
        #endregion

        public event EventHandler CommandExited;

        public void Start()
        {
            if (Disposed)
            {
                throw new ObjectDisposedException(GetType().Name);
            }

            if (_process != null)
            {
                Clean();
            }

            _process = _manager.GetProcess(_executable, _parameters);
            _process.Exited += ProcessExited;
            try
            {
                _process.Start();
                Log.Debug("Started !");
            }
            catch (Exception e)
            {
                if (_process != null)
                {
                    _process.Dispose();
                    _process = null;
                }
                Log.Error(string.Format("Unable to start command {0}: {1}", this, e.Message), e);
            }
        }

        private void ProcessExited(object sender, EventArgs e)
        {
            Log.Error(string.Format("Command {0} has unexpectedly exited (maybe killed) !!!", _name));

            EventHandler handler = CommandExited;
            if (handler != null)
            {
                handler(this, EventArgs.Empty);
            }
        }

        public void Stop()
        {
            if (Disposed)
            {
                throw new ObjectDisposedException(GetType().Name);
            }

            Log.Debug("Stopping...");

            if (_process != null)
            {
                Clean();
                Log.Debug("Stopped !");
            }
            else
            {
                Log.Error(string.Format("Command {0} is not started! No need to stop it.", _name));
            }
        }

        public override String ToString()
        {
            return string.Format("{0}: \"{1}\" {2}", _name, _executable, _parameters);
        }


        /// <summary>
        /// Kill the process and remove it from memory
        /// </summary>
        private void Clean()
        {
            if (_process == null) return;

            try
            {
                _process.Exited -= ProcessExited;
                if (_process.IsStarted)
                {
                    _manager.KillProcess(_process, _killChildren);
                }
                _process.Dispose();
                _process = null;

            }
            catch (Exception ex)
            {
                Log.Error(string.Format("Command {0} can't be killed: {1}", _name, ex.Message), ex);
            }
        }
    }
}