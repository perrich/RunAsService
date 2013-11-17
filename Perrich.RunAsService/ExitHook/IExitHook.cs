namespace Perrich.RunAsService.ExitHook
{
    /// <summary>
    /// Hook executed after an unexpected exit
    /// </summary>
    public interface IExitHook
    {
        /// <summary>
        /// Init the hook running on the provided service with available settings
        /// </summary>
        /// <param name="settings"></param>
        /// <param name="service"></param>
        /// <returns>true if the hook is well initialized</returns>
        bool Init(XmlConfig.XmlConfig settings, IRunAsService service);

        /// <summary>
        /// Launch the hook action
        /// </summary>
        /// <returns></returns>
        bool Launch();
    }
}
