namespace Perrich.RunAsService
{
    /// <summary>
    /// Interface for a service which wrap a command
    /// </summary>
    public interface IRunAsService
    {
        string Description { get; }
        string DisplayName { get; }
        void StartCommand();
        void Stop();
        bool IsStopped { get; }
    }
}
