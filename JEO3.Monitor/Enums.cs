namespace JEO3.Monitor
{
    public enum MonitorPollingStatus
    {
        Stopped,
        Polling,
        Error,
        InsufficientPermissions
    }

    public enum WmiDataType
    {
        Exceptions,
        Memory,
        Locks,
        Databases,
        QueryStore,
        WebService
    }
}
