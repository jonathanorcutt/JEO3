namespace JEO3.Monitor.Wmi
{
    public sealed class WmiMemoryRow
    {
        public UInt64 AllocatedBytesPersec { get; set; }
        public string Caption { get; set; }
        public string Description { get; set; }
        public UInt64 FinalizationSurvivors { get; set; }
        public UInt64 Frequency_Object { get; set; }
        public UInt64 Frequency_PerfTime { get; set; }
        public UInt64 Frequency_Sys100NS { get; set; }
        public UInt64 Gen0heapsize { get; set; }
        public UInt64 Gen0PromotedBytesPerSec { get; set; }
        public UInt64 Gen1heapsize { get; set; }
        public UInt64 Gen1PromotedBytesPerSec { get; set; }
        public UInt64 Gen2heapsize { get; set; }
        public UInt64 LargeObjectHeapsize { get; set; }
        public String Name { get; set; }
        public UInt64 NumberBytesinallHeaps { get; set; }
        public UInt64 NumberGCHandles { get; set; }
        public UInt64 NumberGen0Collections { get; set; }
        public UInt64 NumberGen1Collections { get; set; }
        public UInt64 NumberGen2Collections { get; set; }
        public UInt64 NumberInducedGC { get; set; }
        public UInt64 NumberofPinnedObjects { get; set; }
        public UInt64 NumberofSinkBlocksinuse { get; set; }
        public UInt64 NumberTotalcommittedBytes { get; set; }
        public UInt64 NumberTotalreservedBytes { get; set; }
        public UInt32 PercentTimeinGC { get; set; }
        public UInt32 PercentTimeinGC_Base { get; set; }
        public UInt64 ProcessID { get; set; }
        public UInt64 PromotedFinalizationMemoryfromGen0 { get; set; }
        public UInt64 PromotedMemoryfromGen0 { get; set; }
        public UInt64 PromotedMemoryfromGen1 { get; set; }
        public UInt64 Timestamp_Object { get; set; }
        public UInt64 Timestamp_PerfTime { get; set; }
        public UInt64 Timestamp_Sys100NS { get; set; }
    }
}
