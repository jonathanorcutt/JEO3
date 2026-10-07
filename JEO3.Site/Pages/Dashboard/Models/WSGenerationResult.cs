using JEO3.Generation.Models;
using JEO3.Site.Models;

namespace JEO3.Site.Dashboard
{
    public sealed class WSGenerationResult
    {
        public QueryGenerationResult QueryEditorResult { get; set; } = new();
        public QueryGenerationResult DiagramResultJarvis { get; set; } = new();
        public QueryGenerationResult DiagramResultDatabase { get; set; } = new();
        public DataBarViewModel Databars { get; set; }
    }
}
