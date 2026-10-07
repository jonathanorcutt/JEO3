namespace JEO3.Core.Extensions
{
    public static class SqlExtensions
    {
        public static string Sql(this string identifier)
        {
            try
            {
                ArgumentException.ThrowIfNullOrWhiteSpace(identifier);

                bool needsBrackets =
                    SQLReservedKeywords.ReservedKeywordsAll.Any(v => v.ToLower() == identifier.ToLower()) ||
                    char.IsDigit(identifier[0]) ||
                    identifier.Any(c => !char.IsLetterOrDigit(c) && c != '_');

                return !needsBrackets ? identifier : $"[{identifier.Replace("]", "]]")}]";
            }
            catch (Exception ex)
            {
                return identifier;
            }
        }
    }
}
