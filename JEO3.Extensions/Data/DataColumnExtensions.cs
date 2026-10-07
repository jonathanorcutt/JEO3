using System.Data;

namespace JEO3.Extensions
{
    internal static class DataColumnExtensions
    {
        internal static List<DataColumn> ToList(this DataColumnCollection columns)
        {
            var columnsList = new List<DataColumn>();
            foreach (DataColumn column in columns)
            {
                columnsList.Add(column);
            }
            return columnsList;
        }
    }
}
