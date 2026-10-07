namespace JEO3.Providers.Extensions
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public sealed class JeoTable : Attribute
    {
        public string Name { get; }
        public string Schema { get; set; } = "dbo";
        public JeoTable(string name)
        {
            Name = name;
        }
    }
}
