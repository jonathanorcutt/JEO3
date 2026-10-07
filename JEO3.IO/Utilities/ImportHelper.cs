using System.Data;
using System.Text;
using ExcelDataReader;
using Microsoft.VisualBasic.FileIO;

namespace JEO3.IO
{
    internal class ImportHelper
    {
        public static DataTable FileToDataTable(Stream fileStream, string fileName)
        {
            if (new string[] { ".xls", ".xlsx" }.Any(v => fileName.ToLower().EndsWith(v, StringComparison.OrdinalIgnoreCase)))
            {
                return ExcelToDataTable(fileStream, fileName);
            }
            else if (fileName.ToLower().EndsWith(".csv", StringComparison.OrdinalIgnoreCase))
            {
                return CsvToDataTable(fileStream, fileName);
            }
            else if (fileName.ToLower().EndsWith(".tsv", StringComparison.OrdinalIgnoreCase))
            {
                return TsvToDataTable(fileStream, fileName);
            }
            throw new NotSupportedException("File Format Not Supported");
        }

        public static DataTable ExcelToDataTable(Stream fileStream, string fileName)
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            using (var reader = ExcelReaderFactory.CreateReader(fileStream))
            {
                return GetDataTableFromReader(reader);
            }
        }

        public static DataTable CsvToDataTable(Stream fileStream, string fileName)
        {
            using (var reader = ExcelReaderFactory.CreateCsvReader(fileStream))
            {
                return GetDataTableFromReader(reader);
            }
        }

        public static DataTable TsvToDataTable(Stream fileStream, string fileName)
        {
            var readerConfig = new ExcelReaderConfiguration()
            {
                AutodetectSeparators = new char[] { '\t' }
            };
            using (var reader = ExcelReaderFactory.CreateCsvReader(fileStream, readerConfig))
            {
                return GetDataTableFromReader(reader);
            }
        }
        public static DataTable FixedWidthToDataTable(Stream fileStream, int[] columnWidths)
        {
            var dataTable = new DataTable();

            using (var streamReader = new StreamReader(fileStream))
            using (var parser = new TextFieldParser(streamReader))
            {
                parser.TextFieldType = FieldType.FixedWidth;
                parser.SetFieldWidths(columnWidths);
                bool isHeader = true;
                while (!parser.EndOfData)
                {
                    string[] fields = parser.ReadFields();
                    if (isHeader)
                    {
                        foreach (var field in fields)
                        {
                            dataTable.Columns.Add(field.Trim());
                        }
                        isHeader = false;
                    }
                    else
                    {
                        for (int i = 0; i < fields.Length; i++)
                        {
                            fields[i] = fields[i]?.Trim();
                        }
                        dataTable.Rows.Add(fields);
                    }
                }
            }
            return dataTable;
        }
        public static DataTable GetDataTableFromReader(IExcelDataReader reader, bool useHeaderRow = true)
        {
            var result = reader.AsDataSet(new ExcelDataSetConfiguration()
            {
                ConfigureDataTable = (_) => new ExcelDataTableConfiguration()
                {
                    UseHeaderRow = useHeaderRow
                }
            });
            return result.Tables[0];
        }
    }
}