using System;
using log4net;

namespace Perrich.RunAsService.ExitHook
{
    /// <summary>
    /// Restart the command when an unwanted exit is detected.
    /// It could be limited with a number of times, in that case stop the service after the last attempt.
    /// </summary>
    public class RestartHook : AbstractExitHook
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(RestartHook));

        public static readonly int InfiniteTimes = Int32.MinValue;

        // Current remaining restart times
        private int _remainingTimes = InfiniteTimes;

        public int RemainingTimes
        {
            get
            {
                return _remainingTimes;
            }
        }

        protected override bool Configure(XmlConfig.XmlConfig settings)
        {
            String timesStr = settings.GetItem("restart/times").Value;

            if (!String.IsNullOrEmpty(timesStr))
            {
                if (!Int32.TryParse(timesStr, out _remainingTimes))
                {
                    Log.Warn("Cannot limit the number of restart: the times property is not well defined.");
                    return false;
                }
            }

            return true;
        }

        protected override bool Execute()
        {
            if (_remainingTimes == 0)
            {
                if (Service.IsStopped)
                {
                    Log.Debug("Not restarted: max allowed times already done and the service is stopped.");
                    return false;
                }

                // Cannot retry and service not stopped: stop it
                Log.Debug("Not restarted: max allowed times already done, stopping the service...");
                Service.Stop();
                return true;
            }

            if (_remainingTimes != InfiniteTimes)
            {
                _remainingTimes--; // only decrease remaining times if different than infinite
            }
            Service.StartCommand();

            if (Log.IsDebugEnabled)
            {
                Log.Debug(string.Format("Restart the command" + (_remainingTimes != InfiniteTimes ? " (allowed times: {0})" : String.Empty), _remainingTimes));
            }
            return true;
        }
    }
}
