namespace Perrich.RunAsService.ExitHook
{
    /// <summary>
    /// Stop the service when an unwanted exit is detected
    /// </summary>
    public class StopHook : AbstractExitHook
    {
        protected override bool Configure(XmlConfig.XmlConfig settings) { return true; }

        protected override bool Execute()
        {
            if (Service.IsStopped)
                return false;

            Service.Stop();
            return true;
        }
    }
}
