namespace JEO3.Generation
{
    public sealed class TableAliasGenerator
    {
        #region Properties

        private readonly Dictionary<string, int> _counts = [];

        #endregion

        #region Functions

        internal string Next(string table)
        {
            var alias = new string(table.Where(char.IsUpper).ToArray());
            if (string.IsNullOrWhiteSpace(alias))
                alias = table.Length >= 3 ? table[..3].ToUpperInvariant() : table.ToUpperInvariant();

            if (_counts.TryGetValue(alias, out var c))
            {
                _counts[alias] = ++c;
                return alias + c;
            }

            _counts[alias] = 1;
            return alias;
        }

        internal void Reset()
        {
            _counts.Clear();
        }

        #endregion
    }
}