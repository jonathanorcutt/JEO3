using JEO3.IO.File;
using JEO3.IO.Information;

namespace JEO3.IO.Directory
{
    /// <summary>
    /// This class provides file system search information for use in enumerating system files
    /// </summary>
    public sealed class SearchInformation
    {
        #region Properties

        #region Delegates

        /// <summary>
        /// Delegate for callback invocation during long running file searches
        /// </summary>
        /// <param name="fileObjects"></param>
        /// <param name="directoriesSearched"></param>
        public delegate void SearchCallback(FileObjectList fileObjects, DirectoryObjectList directoriesSearched = null);
        //public delegate void SearchCallback(FileObjectList fileObjects, DirectoryObjectList directoriesSearched = null, SearchEventArgs eventArgs = null);

        #endregion

        #region Search

        /// <summary>
        ///  Whether or not to search files recursively (search all subfolders)
        /// </summary>
        public bool Recurse { get; set; }

        #endregion

        #region Path

        /// <summary>
        ///  Whether or not the search string is case-sensitive
        /// </summary>
        public bool MatchCase { get; set; }

        /// <summary>
        /// Search pattern to use to find files containing the search pattern
        /// </summary>
        public string SearchPattern { get; set; }

        #endregion

        #region DateTime

        /// <summary>
        /// The type of file date search to be conducted
        /// </summary>
        internal FileInformation.FileSearchType DateSearchType { get; private set; }

        /// <summary>
        /// The minimum datetime of the file
        /// </summary>
        public DateTime MinimumDate { get; set; }

        /// <summary>
        /// The minimum datetime of the file
        /// </summary>
        public DateTime MaximumDate { get; set; }

        /// <summary>
        /// Whether or not to use file date parameters when searching for files
        /// </summary>
        internal bool UseDateInformation { get; private set; }

        #endregion

        #region Size

        /// <summary>
        /// The type of file size search to be conducted
        /// </summary>
        internal FileInformation.FileSearchType SizeSearchType { get; private set; }

        /// <summary>
        /// The minimum size file size
        /// </summary>
        public decimal MinimumSize { get; set; }

        /// <summary>
        /// The maximum size file size
        /// </summary>
        public decimal MaximumSize { get; set; }

        /// <summary>
        /// Whether or not to use file size parameters when searching for files
        /// </summary>
        internal bool UseSizeInformation { get; private set; }

        #endregion

        #region Enums

        /// <summary>
        /// The date information type to search
        /// </summary>
        public FileInformation.FileDatePropertyType DateType { get; set; }

        /// <summary>
        /// The file size type to search
        /// </summary>
        public FileInformation.FileSizeType SizeType { get; set; }

        #endregion

        #endregion

        #region Initialization

        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="recurse"></param>
        public SearchInformation(bool recurse)
        {
            this.Recurse = recurse;

            // Configure Search Information Values
            this.LoadSearchInformation();
        }

        /// <summary>
        /// Constructor predicated upon a search pattern
        /// </summary>
        /// <param name="recurse">Whether or not to retrieve files recursively</param>
        /// <param name="strSearchPattern">Search pattern to use to find file system objects</param>
        public SearchInformation(bool recurse, string strSearchPattern)
        {
            this.Recurse = recurse;
            this.SearchPattern = strSearchPattern;

            // Configure Search Information Values
            this.LoadSearchInformation();
        }

        /// <summary>
        /// Constructor predicated upon a search pattern with case sensitivity
        /// </summary>
        /// <param name="recurse">Whether or not to retrieve files recursively</param>
        /// <param name="strSearchPattern">Search pattern to use to find file system objects</param>
        /// <param name="boolMatchCase">Whether or not the search pattern is case sensitive</param>
        public SearchInformation(bool recurse, string strSearchPattern, bool boolMatchCase)
        {
            this.Recurse = recurse;
            this.SearchPattern = strSearchPattern;
            this.MatchCase = boolMatchCase;

            // Configure Search Information Values
            this.LoadSearchInformation();
        }

        /// <summary>
        /// Constructor predicated upon file size
        /// </summary>
        /// <param name="recurse">Whether or not to retrieve files recursively</param>
        /// <param name="strSearchPattern">Search pattern to use to find file system objects</param>
        /// <param name="boolMatchCase">Whether or not the search pattern is case sensitive</param>
        /// <param name="sizeType">The size type to query file system objects for</param>
        /// <param name="decMinimumSize">The minimum size of the file system object</param>
        /// <param name="decMaximumSize">The maximum size of the file system object</param>
        public SearchInformation(bool recurse, string strSearchPattern, bool boolMatchCase,
            FileInformation.FileSizeType sizeType, decimal decMinimumSize, decimal decMaximumSize)
        {
            this.Recurse = recurse;
            this.SearchPattern = strSearchPattern;
            this.MatchCase = boolMatchCase;

            // Configure Search Information Values
            this.LoadSearchInformation();
        }

        /// <summary>
        /// Constructor predicated upon file date information
        /// </summary>
        /// <param name="recurse">Whether or not to retrieve files recursively</param>
        /// <param name="strSearchPattern">Search pattern to use to find file system objects</param>
        /// <param name="boolMatchCase">Whether or not the search pattern is case sensitive</param>
        /// <param name="dateType">The date type to query file system objects for</param>
        /// <param name="dtMinimumDate">The minimum date of the file system object</param>
        /// <param name="dtMaximumDate">The maximum date of the file system object</param>
        public SearchInformation(bool recurse, string strSearchPattern, bool boolMatchCase,
            FileInformation.FileDatePropertyType dateType, DateTime dtMinimumDate, DateTime dtMaximumDate)
        {
            this.Recurse = recurse;
            this.SearchPattern = strSearchPattern;
            this.MatchCase = boolMatchCase;
            this.DateType = dateType;
            this.MinimumDate = dtMinimumDate;
            this.MaximumDate = dtMaximumDate;

            // Configure Search Information Values
            this.LoadSearchInformation();
        }

        /// <summary>
        /// Constructor with constraints for recursion, search parameters, date information, and size information
        /// </summary>
        /// <param name="recurse">Whether or not to retrieve files recursively</param>
        /// <param name="strSearchPattern">Search pattern to use to find file system objects</param>
        /// <param name="boolMatchCase">Whether or not the search pattern is case sensitive</param>
        /// <param name="dateType">The date type to query file system objects for</param>
        /// <param name="dtMinimumDate">The minimum date of the file system object</param>
        /// <param name="dtMaximumDate">The maximum date of the file system object</param>
        /// <param name="sizeType">The size type to query file system objects for</param>
        /// <param name="decMinimumSize">The minimum size of the file system object</param>
        /// <param name="decMaximumSize">The maximum size of the file system object</param>
        public SearchInformation(bool recurse, string strSearchPattern, bool boolMatchCase,
            FileInformation.FileDatePropertyType dateType, DateTime dtMinimumDate, DateTime dtMaximumDate,
            FileInformation.FileSizeType sizeType = FileInformation.FileSizeType.Bytes, decimal decMinimumSize = 0, decimal decMaximumSize = Decimal.MaxValue)
        {
            this.Recurse = recurse;
            this.SearchPattern = strSearchPattern;
            this.MatchCase = boolMatchCase;
            this.DateType = dateType;
            this.MinimumDate = dtMinimumDate;
            this.MaximumDate = dtMaximumDate;
            this.SizeType = sizeType;
            this.MinimumSize = decMinimumSize;
            this.MaximumSize = decMaximumSize;

            // Configure Search Information Values
            this.LoadSearchInformation();
        }

        /// <summary>
        /// Constructor predicated upon file size information
        /// </summary>
        /// <param name="recurse">Whether or not to retrieve files recursively</param>
        /// <param name="sizeType">The size type to query file system objects for</param>
        /// <param name="decMinimumSize">The minimum size of the file system object</param>
        /// <param name="decMaximumSize">The maximum size of the file system object</param>
        public SearchInformation(bool recurse, FileInformation.FileSizeType sizeType, decimal decMinimumSize = 0, decimal decMaximumSize = Decimal.MaxValue)
        {
            this.Recurse = recurse;
            this.SizeType = sizeType;
            this.MinimumSize = decMinimumSize;
            this.MaximumSize = decMaximumSize;

            // Configure Search Information Values
            this.LoadSearchInformation();
        }

        /// <summary>
        /// Constructor predicated upon file date information
        /// </summary>
        /// <param name="recurse">Whether or not to retrieve files recursively</param>
        /// <param name="dateType">The date type to query file system objects for</param>
        /// <param name="dtMinimumDate">The minimum date of the file system object</param>
        /// <param name="dtMaximumDate">The maximum date of the file system object</param>
        public SearchInformation(bool recurse, FileInformation.FileDatePropertyType dateType, DateTime dtMinimumDate, DateTime dtMaximumDate)
        {
            this.Recurse = recurse;
            this.DateType = dateType;
            this.MinimumDate = dtMinimumDate;
            this.MaximumDate = dtMaximumDate;

            // Configure Search Information Values
            this.LoadSearchInformation();
        }

        #endregion

        #region Functions

        /// <summary>
        /// Load search configuration information
        /// </summary>
        private void LoadSearchInformation()
        {
            // Determine Search Pattern
            this.SearchPattern = (this.SearchPattern == null || this.SearchPattern == "") ? "*" : this.SearchPattern;
            this.SearchPattern = (this.SearchPattern[0].ToString() != "*") ? "*" + this.SearchPattern + "*" : this.SearchPattern;

            // Determine Size Information
            this.UseSizeInformation = this.MinimumSize >= 0 || this.MaximumSize < decimal.MaxValue;
            this.SizeSearchType = (this.UseSizeInformation == false) ? FileInformation.FileSearchType.Unknown
                : (this.MaximumSize >= this.MinimumSize) ? FileInformation.FileSearchType.Between
                : (this.MaximumSize < decimal.MaxValue) ? FileInformation.FileSearchType.LessThan : FileInformation.FileSearchType.GreaterThan;

            // Determine Date Information
            this.UseDateInformation = this.MinimumDate > DateTime.MinValue || this.MaximumDate < DateTime.MaxValue;
            this.DateSearchType = (this.UseDateInformation == false) ? FileInformation.FileSearchType.Unknown
                : (this.MaximumDate >= this.MinimumDate) ? FileInformation.FileSearchType.Between
                : (this.MaximumDate < DateTime.MaxValue) ? FileInformation.FileSearchType.LessThan : FileInformation.FileSearchType.GreaterThan;
        }

        #endregion

        #region Filter

        /// <summary>
        /// Internal method to filter files sby search criteria
        /// </summary>
        /// <param name="fileObjectList"></param>
        /// <returns></returns>
        [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
        internal List<FileObject> GetSearchFilesFiltered(FileObjectList fileObjectList)
        {
            List<FileObject> listFiles = [.. fileObjectList.ToArray()];

            #region Filter By Date

            // Validation
            if (this.UseDateInformation == true)
            {
                if (this.DateType == FileInformation.FileDatePropertyType.CreationDate)
                {
                    if (this.DateSearchType == FileInformation.FileSearchType.GreaterThan)
                    {
                        listFiles = listFiles.Where(file => file.CreationDate > this.MinimumDate).ToList();
                    }
                    else if (this.DateSearchType == FileInformation.FileSearchType.LessThan)
                    {
                        listFiles = listFiles.Where(file => file.CreationDate < this.MaximumDate).ToList();
                    }
                    else if (this.DateSearchType == FileInformation.FileSearchType.Between)
                    {
                        listFiles = listFiles
                            .Where(file => file.CreationDate > this.MinimumDate && file.CreationDate < this.MaximumDate).ToList();
                    }
                }
                else if (this.DateType == FileInformation.FileDatePropertyType.LastAccessedDate)
                {
                    if (this.DateSearchType == FileInformation.FileSearchType.GreaterThan)
                    {
                        listFiles = listFiles.Where(file => file.LastAccessedDate > this.MinimumDate).ToList();
                    }
                    else if (this.DateSearchType == FileInformation.FileSearchType.LessThan)
                    {
                        listFiles = listFiles.Where(file => file.LastAccessedDate < this.MaximumDate).ToList();
                    }
                    else if (this.DateSearchType == FileInformation.FileSearchType.Between)
                    {
                        listFiles = listFiles
                            .Where(file => file.LastAccessedDate > this.MinimumDate && file.LastAccessedDate < this.MaximumDate).ToList();
                    }
                }
                else if (this.DateType == FileInformation.FileDatePropertyType.LastWriteDate)
                {
                    if (this.DateSearchType == FileInformation.FileSearchType.GreaterThan)
                    {
                        listFiles = listFiles.Where(file => file.LastWriteDate > this.MinimumDate).ToList();
                    }
                    else if (this.DateSearchType == FileInformation.FileSearchType.LessThan)
                    {
                        listFiles = listFiles.Where(file => file.LastWriteDate < this.MaximumDate).ToList();
                    }
                    else if (this.DateSearchType == FileInformation.FileSearchType.Between)
                    {
                        listFiles = listFiles
                            .Where(file => file.LastWriteDate > this.MinimumDate && file.LastWriteDate < this.MaximumDate).ToList();
                    }
                }
            }

            #endregion

            #region Filter By Size

            // Validation
            if (this.UseSizeInformation == true)
            {
                if (this.SizeType == FileInformation.FileSizeType.Bytes)
                {
                    if (this.DateSearchType == FileInformation.FileSearchType.GreaterThan)
                    {
                        listFiles = listFiles.Where(file => file.Size.Bytes > this.MinimumSize).ToList();
                    }
                    else if (this.DateSearchType == FileInformation.FileSearchType.LessThan)
                    {
                        listFiles = listFiles.Where(file => file.Size.Bytes < this.MaximumSize).ToList();
                    }
                    else if (this.DateSearchType == FileInformation.FileSearchType.Between)
                    {
                        listFiles = listFiles
                            .Where(file => file.Size.Bytes > this.MinimumSize && file.Size.Bytes < this.MaximumSize).ToList();
                    }
                }
                else if (this.SizeType == FileInformation.FileSizeType.KiloBytes)
                {
                    if (this.DateSearchType == FileInformation.FileSearchType.GreaterThan)
                    {
                        listFiles = listFiles.Where(file => file.Size.KiloBytes > this.MinimumSize).ToList();
                    }
                    else if (this.DateSearchType == FileInformation.FileSearchType.LessThan)
                    {
                        listFiles = listFiles.Where(file => file.Size.KiloBytes < this.MaximumSize).ToList();
                    }
                    else if (this.DateSearchType == FileInformation.FileSearchType.Between)
                    {
                        listFiles = listFiles
                            .Where(file => file.Size.KiloBytes > this.MinimumSize && file.Size.KiloBytes < this.MaximumSize).ToList();
                    }
                }
                else if (this.SizeType == FileInformation.FileSizeType.MegaBytes)
                {
                    if (this.DateSearchType == FileInformation.FileSearchType.GreaterThan)
                    {
                        listFiles = listFiles.Where(file => file.Size.MegaBytes > this.MinimumSize).ToList();
                    }
                    else if (this.DateSearchType == FileInformation.FileSearchType.LessThan)
                    {
                        listFiles = listFiles.Where(file => file.Size.MegaBytes < this.MaximumSize).ToList();
                    }
                    else if (this.DateSearchType == FileInformation.FileSearchType.Between)
                    {
                        listFiles = listFiles
                            .Where(file => file.Size.MegaBytes > this.MinimumSize && file.Size.MegaBytes < this.MaximumSize).ToList();
                    }
                }
                else if (this.SizeType == FileInformation.FileSizeType.GigaBytes)
                {
                    if (this.DateSearchType == FileInformation.FileSearchType.GreaterThan)
                    {
                        listFiles = listFiles.Where(file => file.Size.GigaBytes > this.MinimumSize).ToList();
                    }
                    else if (this.DateSearchType == FileInformation.FileSearchType.LessThan)
                    {
                        listFiles = listFiles.Where(file => file.Size.GigaBytes < this.MaximumSize).ToList();
                    }
                    else if (this.DateSearchType == FileInformation.FileSearchType.Between)
                    {
                        listFiles = listFiles
                            .Where(file => file.Size.GigaBytes > this.MinimumSize && file.Size.GigaBytes < this.MaximumSize).ToList();
                    }
                }
            }

            #endregion

            return listFiles;
        }

        #endregion
    }
}
