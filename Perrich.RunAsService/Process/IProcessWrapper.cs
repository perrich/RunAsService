using System;

namespace Perrich.RunAsService.Process
{
    /// <summary>
    /// Wrap a System Process (easier way to manage and check)
    /// </summary>
    public interface IProcessWrapper: IDisposable
    {
        /// <summary>
        /// Callend when the process has exited
        /// </summary>
        event EventHandler Exited;

        /// <summary>
        /// Is the process started ?
        /// </summary>
        bool IsStarted { get; }

        /// <summary>
        /// The current process id
        /// </summary>
        int Id { get; }

        /// <summary>
        /// Start the process
        /// </summary>
        /// <returns>true if the process is well started</returns>
        bool Start();

        /// <summary>
        /// Kill the process
        /// </summary>
        void Kill();
    }
}
