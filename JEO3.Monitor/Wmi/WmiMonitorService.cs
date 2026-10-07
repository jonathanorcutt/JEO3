using System.Management;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;


namespace JEO3.Monitor.Wmi
{
    public sealed class WmiMonitorStore
    {
        private readonly object _lock = new();

        private WmiMonitorSnapshot _current = new();

        public WmiMonitorSnapshot Current
        {
            get
            {
                lock (_lock)
                    return _current;
            }
        }

        public event Action? Changed;

        public void Update(WmiMonitorSnapshot snapshot)
        {
            lock (_lock)
                _current = snapshot;

            Changed?.Invoke();
        }
    }

    public sealed class WmiMonitorSnapshot
    {
        public DateTime SampledAtUtc { get; init; }

        public Dictionary<string, List<object?>> Exceptions { get; init; } = [];
        public Dictionary<string, List<object?>> Memory { get; init; } = [];
        public Dictionary<string, List<object?>> Locks { get; init; } = [];
        public Dictionary<string, List<object?>> Databases { get; init; } = [];
        public Dictionary<string, List<object?>> QueryStore { get; init; } = [];
        public Dictionary<string, List<object?>> WebService { get; init; } = [];

        public Dictionary<string, List<object?>> Processor { get; init; } = [];
        public Dictionary<string, List<object?>> ProcessorInformation { get; init; } = [];
        public Dictionary<string, List<object?>> Process { get; init; } = [];
        public Dictionary<string, List<object?>> ProcessV2 { get; init; } = [];
        public Dictionary<string, List<object?>> Thread { get; init; } = [];
        public Dictionary<string, List<object?>> System { get; init; } = [];

        public Dictionary<string, List<object?>> LogicalDisk { get; init; } = [];
        public Dictionary<string, List<object?>> PhysicalDisk { get; init; } = [];
        public Dictionary<string, List<object?>> PagingFile { get; init; } = [];
        public Dictionary<string, List<object?>> Cache { get; init; } = [];

        public Dictionary<string, List<object?>> NetworkInterface { get; init; } = [];
        public Dictionary<string, List<object?>> TCPv4 { get; init; } = [];
        public Dictionary<string, List<object?>> TCPv6 { get; init; } = [];
        public Dictionary<string, List<object?>> UDPv4 { get; init; } = [];
        public Dictionary<string, List<object?>> UDPv6 { get; init; } = [];

        public Dictionary<string, List<object?>> Server { get; init; } = [];
        public Dictionary<string, List<object?>> ServerWorkQueues { get; init; } = [];
        public Dictionary<string, List<object?>> Redirector { get; init; } = [];

        public Dictionary<string, List<object?>> SMBServerShares { get; init; } = [];
        public Dictionary<string, List<object?>> SMBServerSessions { get; init; } = [];
        public Dictionary<string, List<object?>> SMBClientShares { get; init; } = [];
        public Dictionary<string, List<object?>> SMBClientSessions { get; init; } = [];

        public Dictionary<string, List<object?>> HTTPService { get; init; } = [];
        public Dictionary<string, List<object?>> HTTPServiceRequestQueues { get; init; } = [];

        public Dictionary<string, List<object?>> NETCLRJIT { get; init; } = [];
        public Dictionary<string, List<object?>> NETCLRLoading { get; init; } = [];
        public Dictionary<string, List<object?>> NETCLRLocksAndThreads { get; init; } = [];
        public Dictionary<string, List<object?>> NETCLRRemoting { get; init; } = [];
        public Dictionary<string, List<object?>> NETCLRSecurity { get; init; } = [];
        public Dictionary<string, List<object?>> NETCLRInterop { get; init; } = [];

        public Dictionary<string, List<object?>> ASPNET { get; init; } = [];
        public Dictionary<string, List<object?>> ASPNETApplications { get; init; } = [];

        public Dictionary<string, List<object?>> TerminalServices { get; init; } = [];
        public Dictionary<string, List<object?>> TerminalServicesSession { get; init; } = [];

        public Dictionary<string, List<object?>> DNSClient { get; init; } = [];
        public Dictionary<string, List<object?>> DNS { get; init; } = [];

        public Dictionary<string, List<object?>> IPv4 { get; init; } = [];
        public Dictionary<string, List<object?>> IPv6 { get; init; } = [];

        public Dictionary<string, List<object?>> WFPv4 { get; init; } = [];
        public Dictionary<string, List<object?>> WFPv6 { get; init; } = [];

        public Dictionary<string, List<object?>> Service { get; init; } = [];
        public Dictionary<string, List<object?>> Objects { get; init; } = [];

        public Dictionary<string, List<object?>> JobObject { get; init; } = [];
        public Dictionary<string, List<object?>> Registry { get; init; } = [];

        public Dictionary<string, List<object?>> HyperVHypervisorLogicalProcessor { get; init; } = [];
        public Dictionary<string, List<object?>> HyperVHypervisorVirtualProcessor { get; init; } = [];
        public Dictionary<string, List<object?>> HyperVDynamicMemoryVM { get; init; } = [];
        public Dictionary<string, List<object?>> HyperVVirtualMachineSummary { get; init; } = [];
        public Dictionary<string, List<object?>> HyperVVirtualSwitch { get; init; } = [];
        public Dictionary<string, List<object?>> HyperVVirtualNetworkAdapter { get; init; } = [];

        public Dictionary<string, List<object?>> PrintQueue { get; init; } = [];
        public Dictionary<string, List<object?>> WindowsSearchIndexer { get; init; } = [];

        public Dictionary<string, List<object?>> DFSReplication { get; init; } = [];
        public Dictionary<string, List<object?>> DFSNamespace { get; init; } = [];

        public Dictionary<string, List<object?>> ClusterNode { get; init; } = [];
        public Dictionary<string, List<object?>> ClusterNetwork { get; init; } = [];
        public Dictionary<string, List<object?>> ClusterNetworkInterface { get; init; } = [];
    }


    public sealed class WmiMonitorService(
        WmiMonitorStore store,
        ILogger<WmiMonitorService> logger) : BackgroundService
    {
        private const string Namespace = @"root\cimv2";

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    store.Update(new WmiMonitorSnapshot
                    {
                        SampledAtUtc = DateTime.UtcNow,
                        Exceptions = Query("Win32_PerfRawData_NETFramework_NETCLRExceptions"),
                        Memory = Query("Win32_PerfRawData_NETFramework_NETCLRMemory"),
                        Locks = Query("Win32_PerfRawData_MSSQLSERVER_SQLServerLocks"),
                        Databases = Query("Win32_PerfRawData_MSSQLSERVER_SQLServerDatabases"),
                        QueryStore = Query("Win32_PerfRawData_MSSQLSERVER_SQLServerQueryStore"),
                        WebService = Query("Win32_PerfRawData_W3SVC_WebService")
                    });
                    //store.Update(new WmiMonitorSnapshot
                    //{
                    //    SampledAtUtc = DateTime.UtcNow,

                    //    Exceptions = Query("Win32_PerfRawData_NETFramework_NETCLRExceptions"),
                    //    Memory = Query("Win32_PerfRawData_PerfOS_Memory"),
                    //    Locks = Query("Win32_PerfRawData_MSSQLSERVER_SQLServerLocks"),
                    //    Databases = Query("Win32_PerfRawData_MSSQLSERVER_SQLServerDatabases"),
                    //    QueryStore = Query("Win32_PerfRawData_MSSQLSERVER_SQLServerQueryStore"),
                    //    WebService = Query("Win32_PerfRawData_W3SVC_WebService"),

                    //    Processor = Query("Win32_PerfRawData_PerfOS_Processor"),
                    //    ProcessorInformation = Query("Win32_PerfRawData_PerfOS_ProcessorInformation"),
                    //    Process = Query("Win32_PerfRawData_PerfProc_Process"),
                    //    ProcessV2 = Query("Win32_PerfRawData_PerfProc_ProcessV2"),
                    //    Thread = Query("Win32_PerfRawData_PerfProc_Thread"),
                    //    System = Query("Win32_PerfRawData_PerfOS_System"),

                    //    LogicalDisk = Query("Win32_PerfRawData_PerfDisk_LogicalDisk"),
                    //    PhysicalDisk = Query("Win32_PerfRawData_PerfDisk_PhysicalDisk"),
                    //    PagingFile = Query("Win32_PerfRawData_PerfOS_PagingFile"),
                    //    Cache = Query("Win32_PerfRawData_PerfOS_CacheStore"),

                    //    NetworkInterface = Query("Win32_PerfRawData_Tcpip_NetworkInterface"),
                    //    TCPv4 = Query("Win32_PerfRawData_Tcpip_TCPv4"),
                    //    TCPv6 = Query("Win32_PerfRawData_Tcpip_TCPv6"),
                    //    UDPv4 = Query("Win32_PerfRawData_Tcpip_UDPv4"),
                    //    UDPv6 = Query("Win32_PerfRawData_Tcpip_UDPv6"),

                    //    Server = Query("Win32_PerfRawData_PerfNet_Server"),
                    //    ServerWorkQueues = Query("Win32_PerfRawData_PerfNet_ServerWorkQueues"),
                    //    Redirector = Query("Win32_PerfRawData_PerfNet_Redirector"),

                    //    SMBServerShares = Query("Win32_PerfRawData_PerfSMB_ServerShares"),
                    //    SMBServerSessions = Query("Win32_PerfRawData_PerfSMB_ServerSessions"),
                    //    SMBClientShares = Query("Win32_PerfRawData_PerfSMB_ClientShares"),
                    //    SMBClientSessions = Query("Win32_PerfRawData_PerfSMB_ClientSessions"),

                    //    HTTPService = Query("Win32_PerfRawData_HTTPService_HTTPService"),
                    //    HTTPServiceRequestQueues = Query("Win32_PerfRawData_HTTPService_HTTPServiceRequestQueues"),

                    //    NETCLRJIT = Query("Win32_PerfRawData_NETFramework_NETCLRJit"),
                    //    NETCLRLoading = Query("Win32_PerfRawData_NETFramework_NETCLRLoading"),
                    //    NETCLRLocksAndThreads = Query("Win32_PerfRawData_NETFramework_NETCLRLocksAndThreads"),
                    //    NETCLRRemoting = Query("Win32_PerfRawData_NETFramework_NETCLRRemoting"),
                    //    NETCLRSecurity = Query("Win32_PerfRawData_NETFramework_NETCLRSecurity"),
                    //    NETCLRInterop = Query("Win32_PerfRawData_NETFramework_NETCLRInterop"),

                    //    ASPNET = Query("Win32_PerfRawData_ASPNET_ASPNET"),
                    //    ASPNETApplications = Query("Win32_PerfRawData_ASPNET_ASPNETApplications"),

                    //    TerminalServices = Query("Win32_PerfRawData_TermService_TerminalServices"),
                    //    TerminalServicesSession = Query("Win32_PerfRawData_TermService_TerminalServicesSession"),

                    //    DNSClient = Query("Win32_PerfRawData_DNSClient_DNSClient"),
                    //    DNS = Query("Win32_PerfRawData_DNS_DNS"),

                    //    IPv4 = Query("Win32_PerfRawData_Tcpip_IPv4"),
                    //    IPv6 = Query("Win32_PerfRawData_Tcpip_IPv6"),

                    //    WFPv4 = Query("Win32_PerfRawData_WFP_WFPv4"),
                    //    WFPv6 = Query("Win32_PerfRawData_WFP_WFPv6"),

                    //    Service = Query("Win32_PerfRawData_PerfOS_Service"),
                    //    Objects = Query("Win32_PerfRawData_PerfOS_Objects"),

                    //    JobObject = Query("Win32_PerfRawData_PerfProc_JobObject"),
                    //    Registry = Query("Win32_PerfRawData_PerfOS_Registry"),

                    //    HyperVHypervisorLogicalProcessor = Query("Win32_PerfRawData_HvStats_HyperVHypervisorLogicalProcessor"),
                    //    HyperVHypervisorVirtualProcessor = Query("Win32_PerfRawData_HvStats_HyperVHypervisorVirtualProcessor"),
                    //    HyperVDynamicMemoryVM = Query("Win32_PerfRawData_HvStats_HyperVDynamicMemoryVM"),
                    //    HyperVVirtualMachineSummary = Query("Win32_PerfRawData_HvStats_HyperVVirtualMachineSummary"),
                    //    HyperVVirtualSwitch = Query("Win32_PerfRawData_HvStats_HyperVVirtualSwitch"),
                    //    HyperVVirtualNetworkAdapter = Query("Win32_PerfRawData_HvStats_HyperVVirtualNetworkAdapter"),

                    //    PrintQueue = Query("Win32_PerfRawData_Spooler_PrintQueue"),
                    //    WindowsSearchIndexer = Query("Win32_PerfRawData_PerfOS_WindowsSearchIndexer"),

                    //    DFSReplication = Query("Win32_PerfRawData_DFSR_DFSR"),
                    //    DFSNamespace = Query("Win32_PerfRawData_DFSNamespace_DFSNamespace"),

                    //    ClusterNode = Query("Win32_PerfRawData_FailoverClustering_ClusterNode"),
                    //    ClusterNetwork = Query("Win32_PerfRawData_FailoverClustering_ClusterNetwork"),
                    //    ClusterNetworkInterface = Query("Win32_PerfRawData_FailoverClustering_ClusterNetworkInterface")
                    //});
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "WMI monitor polling failed.");
                }

                await Task.Delay(TimeSpan.FromSeconds(2), stoppingToken);
            }
        }

        private static Dictionary<string, List<object?>> Query(string className)
        {
            using var searcher = new ManagementObjectSearcher(
                Namespace,
                $"SELECT * FROM {className}");

            using var results = searcher.Get();

            var columns = new Dictionary<string, List<object?>>();

            foreach (ManagementObject item in results)
            {
                foreach (PropertyData property in item.Properties)
                {
                    if (!columns.TryGetValue(property.Name, out var values))
                    {
                        values = [];
                        columns[property.Name] = values;
                    }

                    values.Add(property.Value);
                }
            }

            return columns;
        }
    }
}