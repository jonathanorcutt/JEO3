namespace JEO3.Monitor
{
    public interface IMonitorStateConsumer
    {
        event Action<MonitorState>? OnMonitorStateChanged;
        void NotifyMonitorStateChanged(MonitorState state);
    }
}
