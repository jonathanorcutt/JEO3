namespace JEO3.Providers.Extensions
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = true)]
    public sealed class JeoKey : Attribute
    {
        public string Name { get; set; } = string.Empty;
        public bool IsPrimaryKey { get; set; }
        public bool IsForeignKey { get; set; }
        public bool IsDbGenerated { get; set; } // e.g., IDENTITY or Default values
    }
}
