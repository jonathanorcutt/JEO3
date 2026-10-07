namespace JEO3.Monitor.Wmi
{
    public sealed class WmiExceptionRow
    {
        public string Caption { get; set; }
        public string Description { get; set; }
        public UInt64 Frequency_Object { get; set; }
        public UInt64 Frequency_PerfTime { get; set; }
        public UInt64 Frequency_Sys100NS { get; set; }
        public String Name { get; set; }
        public UInt32 NumberofExcepsThrown { get; set; }
        public UInt32 NumberofExcepsThrownPersec { get; set; }
        public UInt32 NumberofFiltersPersec { get; set; }
        public UInt32 NumberofFinallysPersec { get; set; }
        public UInt32 ThrowToCatchDepthPersec { get; set; }
        public UInt64 Timestamp_Object { get; set; }
        public UInt64 Timestamp_PerfTime { get; set; }
        public UInt64 Timestamp_Sys100NS { get; set; }

    }
}
