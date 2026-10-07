using System.Security;
using System.Xml;
using System.Xml.Serialization;
using Microsoft.VisualBasic.FileIO;

namespace JEO3.IO.File
{
    /// <summary>
    /// File content extensions
    /// </summary>
    public static class FileContentExtensions
    {
        #region Get Content As String

        /// <summary>
        /// Retrieve file content as a string
        /// </summary>
        /// <param name="fileContent">File content to retrieve</param>
        /// <returns></returns>
        [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
        internal static string GetContentAsString(this FileContent fileContent)
        {
            try
            {
                // Read All File Text
                string strContent = System.IO.File.ReadAllText(fileContent.File.FilePath);

                return strContent;
            }
            catch (FileNotFoundException ex)
            {
                // To Be Implemented: Throw Custom Exception...
                Console.WriteLine(ex.ToString());
                return "";
            }
            catch (IOException ex)
            {
                // To Be Implemented: Throw Custom Exception...
                Console.WriteLine(ex.ToString());
                return "";
            }
            catch (UnauthorizedAccessException ex)
            {
                // To Be Implemented: Throw Custom Exception...
                Console.WriteLine(ex.ToString());
                return "";
            }
            catch (SecurityException ex)
            {
                // To Be Implemented: Throw Custom Exception...
                Console.WriteLine(ex.ToString());
                return "";
            }
            catch (Exception ex)
            {
                // To Be Implemented: Throw Custom Exception...
                Console.WriteLine(ex.ToString());
                return "";
            }
        }

        #endregion

        #region Get Content As Line List

        /// <summary>
        /// Retrieve content from a file as a line list
        /// </summary>
        /// <param name="fileContent">File content to retrieve</param>
        /// <returns></returns>
        [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
        internal static List<string> GetContentAsLineList(this FileContent fileContent)
        {
            try
            {
                // Read All File Lines - UTF8 Encoding
                List<string> listLines = System.IO.File.ReadAllLines(fileContent.File.FilePath, System.Text.Encoding.UTF8).ToList();

                return listLines;
            }
            catch (FileNotFoundException ex)
            {
                // To Be Implemented: Throw Custom Exception...
                Console.WriteLine(ex.ToString());
                return null;
            }
            catch (IOException ex)
            {
                // To Be Implemented: Throw Custom Exception...
                Console.WriteLine(ex.ToString());
                return null;
            }
            catch (UnauthorizedAccessException ex)
            {
                // To Be Implemented: Throw Custom Exception...
                Console.WriteLine(ex.ToString());
                return null;
            }
            catch (SecurityException ex)
            {
                // To Be Implemented: Throw Custom Exception...
                Console.WriteLine(ex.ToString());
                return null;
            }
            catch (Exception ex)
            {
                // To Be Implemented: Throw Custom Exception...
                Console.WriteLine(ex.ToString());
                return null;
            }
        }

        #endregion

        #region Get Content As Memory Stream

        /// <summary>
        /// Retrieve content from a file as a MemoryStream
        /// </summary>
        /// <param name="fileContent">File content to retrieve</param>
        /// <returns></returns>
        [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
        internal static MemoryStream GetContentAsMemoryStream(this FileContent fileContent)
        {
            try
            {
                // Read All File Lines
                byte[] listBytes = System.IO.File.ReadAllBytes(fileContent.File.FilePath);

                // Create MemoryStream 
                MemoryStream stream = new MemoryStream(listBytes);

                return stream;
            }
            catch (FileNotFoundException ex)
            {
                // To Be Implemented: Throw Custom Exception...
                Console.WriteLine(ex.ToString());
                return null;
            }
            catch (IOException ex)
            {
                // To Be Implemented: Throw Custom Exception...
                Console.WriteLine(ex.ToString());
                return null;
            }
            catch (UnauthorizedAccessException ex)
            {
                // To Be Implemented: Throw Custom Exception...
                Console.WriteLine(ex.ToString());
                return null;
            }
            catch (SecurityException ex)
            {
                // To Be Implemented: Throw Custom Exception...
                Console.WriteLine(ex.ToString());
                return null;
            }
            catch (Exception ex)
            {
                // To Be Implemented: Throw Custom Exception...
                Console.WriteLine(ex.ToString());
                return null;
            }
        }

        #endregion

        #region Get Delimited File Content

        /// <summary>
        /// Retrieve a list of lines from a file in a delimited format
        /// </summary>
        /// <param name="fileContent">File content</param>
        /// <param name="strDelimiter">Delimiter string</param>
        /// <param name="boolSkipFirstLineHeaders">Optional: Skip the first line because it represents the file header</param>
        /// <returns></returns>
        public static List<List<string>> GetDelimitedLineList(this FileContent fileContent, string strDelimiter, ref string strError, bool boolSkipFirstLineHeaders = false)
        {
            try
            {
                List<List<string>> listFileLines = [];

                // Create Field Parser
                TextFieldParser parser = new TextFieldParser(fileContent.MemoryStream)
                {
                    // Set Delimiters
                    TextFieldType = FieldType.Delimited
                };
                parser.SetDelimiters(strDelimiter);

                bool boolIsFirstRow = true;

                // Loop Data
                while (parser.EndOfData == false)
                {
                    // Read Fields
                    List<string> fileLine = parser.ReadFields().ToList();

                    // Check Skip First Line
                    if (boolSkipFirstLineHeaders == true && boolIsFirstRow == true)
                    {
                        boolIsFirstRow = false;
                        continue;
                    }

                    // Add Line To List
                    listFileLines.Add(fileLine);
                }

                // Close Parser
                parser.Close();

                return listFileLines;
            }
            catch (Exception ex)
            {
                // To Be Implemented: Throw Custom Exception...
                Console.WriteLine(ex.ToString());
                return null;
            }
        }

        /// <summary>
        /// Retrieve a list of lines from a file in a fixed width format
        /// </summary>
        /// <param name="fileContent">File content</param>
        /// <param name="listFieldWidths">A list of field widths</param>
        /// <param name="boolSkipFirstLineHeaders">Optional: Skip the first line because it represents the file header</param>
        /// <returns></returns>
        public static List<List<string>> GetFixedWidthLineList(this FileContent fileContent, int[] listFieldWidths, ref string strError, bool boolSkipFirstLineHeaders = false)
        {
            List<List<string>> listFileLines = [];

            try
            {
                // Create Field Parser
                TextFieldParser parser = new TextFieldParser(fileContent.MemoryStream)
                {
                    // Set Fixed Width
                    TextFieldType = FieldType.FixedWidth
                };
                parser.SetFieldWidths(listFieldWidths);

                bool boolIsFirstRow = true;

                // Loop Data
                while (parser.EndOfData == false)
                {
                    List<string> fileLine = parser.ReadFields().ToList();

                    // Check Skip First Line
                    if (boolSkipFirstLineHeaders == true && boolIsFirstRow == true)
                    {
                        boolIsFirstRow = false;
                        continue;
                    }

                    // Add Line To List
                    listFileLines.Add(fileLine);
                }

                // Close Parser
                parser.Close();

                return listFileLines;
            }
            catch (Exception ex)
            {
                // To Be Implemented: Throw Custom Exception...
                Console.WriteLine(ex.ToString());
                return null;
            }
        }

        #endregion

        #region Serialization

        /// <summary>
        /// Deserializes the specified serialized data.
        /// </summary>
        /// <param name="fileContent">The serialized data.</param>
        /// <returns></returns>
        public static T Deserialize<T>(this FileContent fileContent)
        {
            // Create New Serializer
            XmlSerializer serializer = new XmlSerializer(typeof(T));

            // Create New Reader
            XmlTextReader reader = new XmlTextReader(new StringReader(fileContent.Value));

            return (T)serializer.Deserialize(reader);
        }

        #endregion
    }
}
