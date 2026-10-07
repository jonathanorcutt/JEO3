using JEO3.Generation.Models;
using JEO3.Schema;


namespace JEO3.Generation
{
    public interface IQueryGenerator
    {
        QueryGenerationResult Generate(ITable root);
        QueryGenerationResult Generate(ITable root, QueryGenerationOptions options);
    }
}
