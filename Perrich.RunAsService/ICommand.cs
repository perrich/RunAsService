using System;
using log4net;
using Perrich.RunAsService.Process;

namespace Perrich.RunAsService
{
    /// <summary>
    /// Describe the properties of command that needs to be run as a service.
    /// </summary>
    public interface ICommand : IDisposable
    {
        /// <summary>
        /// The command name
        /// </summary>
        string Name { get; }

        /// <summary>
        /// The executable path
        /// </summary>
        string Executable { get; }

        /// <summary>
        /// The executable parameters 
        /// </summary>
        string Parameters { get; }

        /// <summary>
        /// Do we need to kill process launched by this command ?
        /// </summary>
        bool KillChildren { get; }
 
        /// <summary>
        /// Called when the command exited
        /// </summary>
        event EventHandler CommandExited;

        /// <summary>
        /// Starts the command without a window.
        /// </summary>
        void Start();

        /// <summary>
        /// Stops the command and destroy's the associated process.
        /// If the process exists its kill method is called to end the process.
        /// </summary>
        void Stop();

        /// <summary>
        /// Constructs a concatenation of the properties of the command.
        /// </summary>
        /// <returns>The string concatenation of the command's properties</returns>
        String ToString();
    }
}