namespace JEO3.Logging
{
    internal sealed class CachedMethodMetadata
    {
        public string ClassFullName { get; set; } = string.Empty;
        public string Namespace { get; set; } = string.Empty;
        public string AssemblyVersion { get; set; } = string.Empty;
        public string AssemblyFilePath { get; set; } = string.Empty;
    }
}
