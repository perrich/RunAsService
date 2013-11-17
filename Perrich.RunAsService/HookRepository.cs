using System;
using System.Collections.Generic;
using log4net;
using Perrich.RunAsService.ExitHook;

namespace Perrich.RunAsService
{
    /// <summary>
    /// A hook repository
    /// </summary>
    public class HookRepository
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(HookRepository));

        private readonly List<IExitHook> _hooks = new List<IExitHook>();

        private readonly String _hookClassNames;
        private readonly XmlConfig.XmlConfig _settings;
        private readonly IRunAsService _service;

        public virtual IList<IExitHook> Hooks
        {
            get
            {
                return _hooks;
            }
        }

        /// <summary>
        /// Create a IExitHook list from a class names list using the provided configuration and service
        /// </summary>
        /// <param name="hookClassNames"></param>
        /// <param name="settings"></param>
        /// <param name="service"></param>
        public HookRepository(String hookClassNames, XmlConfig.XmlConfig settings, IRunAsService service)
        {
            _hookClassNames = hookClassNames;
            _settings = settings;
            _service = service;
            Reset();
        }

        private static IExitHook GetHook(String hookClassName)
        {
            IExitHook hook = null;

            // quick naming, allow to declare the class name without the default namespace
            if (!hookClassName.Contains("."))
            {
                hookClassName = "Perrich.RunAsService.ExitHook." + hookClassName;
            }


            try
            {
                var type = Type.GetType(hookClassName, true);
                if (type != null)
                {
                    hook = Activator.CreateInstance(type) as IExitHook;
                }
            }
            catch (Exception ex)
            {
                Log.Error(string.Format("cannot create instance of hookClassName: {0}", ex.Message), ex);
            }

            return hook;
        }

        /// <summary>
        /// Reset all hooks (reuse the configuration)
        /// </summary>
        public void Reset()
        {
            _hooks.Clear();

            var hookClassNames = _hookClassNames.Split(new[] { "," },
                     StringSplitOptions.RemoveEmptyEntries);

            foreach (var hookClassName in hookClassNames)
            {
                var hook = GetHook(hookClassName.Trim());
                if (hook != null)
                {
                    hook.Init(_settings, _service);
                    _hooks.Add(hook);
                }
            }
        }
    }
}
