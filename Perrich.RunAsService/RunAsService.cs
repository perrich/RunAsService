using System;
using System.IO;
using System.Reflection;
using System.ServiceProcess;
using log4net;
using Perrich.RunAsService.Process;

namespace Perrich.RunAsService
{
    public partial class RunAsService : ServiceBase, IRunAsService
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(RunAsService));
        private XmlConfig.XmlConfig _settings;

        private ICommand _myCommand;

        private HookRepository _repository;
        private CommandBuilder _builder;

        private bool _disposed;

        public RunAsService()
        {
            InitializeComponent();

            if (!Init())
            {
                throw new InvalidOperationException("Initialization can't be done !");
            }
        }

        public RunAsService(XmlConfig.XmlConfig settings, HookRepository repository, CommandBuilder builder)
        {
            _repository = repository;
            _builder = builder;
            _settings = settings;

            InitProperties();
        }

        public string DisplayName { get; private set; }
        public string Description { get; private set; }

        private bool Init()
        {
            Log.Debug("Initialize from configuration file...");

            try
            {
                var currentDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                if (currentDir == null)
                    throw new IOException("Cannot retrieve the current assembly directory name.");
                _settings =
                    new XmlConfig.XmlConfig(
                        Path.Combine(currentDir,
                                     "configuration.xml"));

                InitProperties();

                _repository = new HookRepository(_settings.GetItem("exitHooks").Value, _settings, this);
                _builder = new CommandBuilder(new ProcessManager());

                return true;
            }
            catch (Exception ex)
            {
                Log.Error("Service can't be initialized", ex);
            }

            return false;
        }

        private void InitProperties()
        {
            ServiceName = _settings.GetItem("name").Value;
            DisplayName = _settings.GetItem("displayName").Value;
            Description = _settings.GetItem("description").Value;
            IsStopped = true;

            Log.Debug("Initialized as \"" + DisplayName + "\"");
        }

        private void CommandExited(object obj, EventArgs args)
        {
            _myCommand = null;
            foreach (var hook in _repository.Hooks)
            {
                hook.Launch();
            }
        }

        public void StartCommand()
        {
            IsStopped = false;

            if (_disposed)
            {
                Log.Error("Object is disposed.");
                throw new ObjectDisposedException(GetType().Name);
            }

            if (_myCommand != null)
            {
                Log.Error("Cannot start twice the command!");
                throw new InvalidOperationException("Cannot start twice the command!");
            }

            _myCommand = _builder.BuildCommand(_settings, ServiceName);
            _myCommand.CommandExited += CommandExited;
            _myCommand.Start();
        }

        public bool IsStopped { get; private set; }

        internal void InternalStart()
        {
            OnStart(null);
        }

        internal void InternalStop()
        {
            OnStop();
        }

        protected override void OnStart(string[] args)
        {
            Log.Debug("Starting...");
            StartCommand();
        }

        protected override void OnStop()
        {
            Log.Debug("Stopping...");
            if (_myCommand != null)
            {
                _myCommand.Stop();
                _myCommand.CommandExited -= CommandExited;
                _myCommand.Dispose();
                _myCommand = null;
            }

            // Hooks must be resetted
            if (_repository != null)
            {
                _repository.Reset();
            }
            IsStopped = true;
        }

        private void DisposeCommand() // Called in the designer...
        {
            if (!_disposed)
            {
                if (_myCommand != null)
                {
                    _myCommand.Dispose();
                    _myCommand = null;
                }
                _disposed = true;
            }
        }
    }
}