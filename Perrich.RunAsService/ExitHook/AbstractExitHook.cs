using System;

namespace Perrich.RunAsService.ExitHook
{
    /// <summary>
    /// Manage initialization when an unwanted exit is detected
    /// </summary>
    public abstract class AbstractExitHook : IExitHook
    {
        /// <summary>
        /// The Service which runs the hook
        /// </summary>
        protected IRunAsService Service { get; private set; }

        /// <summary>
        /// Is the hook initialized ?
        /// </summary>
        public bool Initialized { get; private set; }

        public bool Init(XmlConfig.XmlConfig settings, IRunAsService service)
        {
            if (settings == null)
            {
                throw new ArgumentNullException("settings");
            }

            if (service == null)
            {
                throw new ArgumentNullException("service");
            }

            Service = service;
            Initialized = Configure(settings);
            return Initialized;
        }

        public bool Launch()
        {
            if (Initialized)
            {
                return Execute();
            }
            return false;
        }

        /// <summary>
        /// Execute the hook action
        /// </summary>
        /// <returns></returns>
        protected abstract bool Execute();

        /// <summary>
        /// Configure the hook with available settings
        /// </summary>
        /// <param name="settings">the available settings (can't be null)</param>
        /// <returns>true if the hook is well configured, false is probably due to a missing/Wrong setting</returns>
        protected abstract bool Configure(XmlConfig.XmlConfig settings);
    }
}
