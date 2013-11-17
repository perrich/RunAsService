using System;
using log4net;
using Perrich.RunAsService.Process;
using Perrich.RunAsService.XmlConfig;

namespace Perrich.RunAsService
{
    /// <summary>
    /// Allow to build commands
    /// </summary>
    public class CommandBuilder
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof (CommandBuilder));

        private readonly IProcessManager _processManager;

        public CommandBuilder(IProcessManager processManager)
        {
            _processManager = processManager;
        }

        /// <summary>
        /// Build a command using the settings and a name (for instance "Microsoft Word" for word.exe)
        /// </summary>
        /// <param name="settings">The available settings</param>
        /// <param name="commandName">The command name</param>
        /// <returns></returns>
        public virtual ICommand BuildCommand(XmlConfig.XmlConfig settings, String commandName)
        {
            if (settings == null)
            {
                Log.Error("Settings must be well set in the config file before trying to start the command.");
                throw new InvalidOperationException(
                    "Settings must be well set in the config file before trying to start the command.");
            }

            Log.Debug("Reading command configuration...");

            string executable = settings.GetItem("executable").Value;

            if (String.IsNullOrEmpty(executable))
            {
                Log.Error("Executable is mandatory to start the command.");
                throw new XmlConfigException("Executable is mandatory to start the command.");
            }

            // Parameter default value is empty
            string parameters = settings.GetItem("parameters").Value;

            // killProcessTree default value is false
            var item = settings.GetItem("killProcessTree");
            bool killChildren = String.IsNullOrEmpty(item.Value)
                                    ? false
                                    : item.BoolValue;

            return new Command(_processManager, commandName, executable, parameters, killChildren);
        }
    }
}