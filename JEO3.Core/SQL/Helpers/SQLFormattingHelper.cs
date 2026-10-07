namespace JEO3.Core
{
    public static class SQLFormattingHelper
    {
        public static List<string> GetSqlColumnNameListFromText(string text)
        {
            if (text == null || text.Length == 0) { return []; }

            var columnList = text.Split('\n')
                .Select(v => v.Replace("\r", string.Empty).Trim())
                .Where(v => !string.IsNullOrEmpty(v))
                .Select(v => SQLReservedKeywords.ReservedKeywordsAll.Any(x => x.ToLower() == v.ToLower()) ? String.Format("{0}{1}{2}", "[", v, "]") : v)
                .Distinct().Order().ToList();

            return columnList;
        }
    }
}
