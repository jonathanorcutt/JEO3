namespace JEO3.Site.Components
{
    public sealed class MonacoThemeData
    {
        #region Properties

        public string Base { get; set; } = "vs-dark";
        public bool Inherit { get; set; } = true;
        public List<MonacoTokenRule> Rules { get; set; } = [];
        public Dictionary<string, string> Colors { get; set; } = [];

        #endregion
    }
}
