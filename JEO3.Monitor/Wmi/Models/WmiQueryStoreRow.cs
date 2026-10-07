namespace JEO3.Monitor.Wmi
{
    public sealed class WmiQueryStoreRow
    {
        public string Caption { get; set; }
        public string Description { get; set; }
        public UInt64 Frequency_Object { get; set; }
        public UInt64 Frequency_PerfTime { get; set; }
        public UInt64 Frequency_Sys100NS { get; set; }
        public String Name { get; set; }
        public UInt64 QueryStoreCPUusage { get; set; }
        public UInt64 QueryStorelogicalreads { get; set; }
        public UInt64 QueryStorelogicalwrites { get; set; }
        public UInt64 QueryStorephysicalreads { get; set; }
        public UInt64 Timestamp_Object { get; set; }
        public UInt64 Timestamp_PerfTime { get; set; }
        public UInt64 Timestamp_Sys100NS { get; set; }
    }
}
