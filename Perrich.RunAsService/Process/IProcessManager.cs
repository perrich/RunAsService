using System;

namespace Perrich.RunAsService.Process
{
    /// <summary>
    /// Manage process
    /// </summary>
    public interface IProcessManager
    {
        /// <summary>
        /// Create a process using parameters
        /// </summary>
        /// <param name="executable">the executable path</param>
        /// <param name="parameters">the parameters</param>
        /// <returns></returns>
        IProcessWrapper GetProcess(String executable, String parameters);

        /// <summary>
        /// Kill the process
        /// </summary>
        /// <param name="process">the process to kill</param>
        /// <param name="killChildren">is process children must be killed ?</param>
        void KillProcess(IProcessWrapper process, bool killChildren);
    }
}
