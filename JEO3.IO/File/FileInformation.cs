namespace JEO3.IO.Information
{
    /// <summary>
    /// This class holds File information properties
    /// </summary>
    public static class FileInformation
    {
        #region Properties

        #region Information

        /// <summary>
        /// The type of write action to be performed
        /// </summary>
        public enum FileWriteType : int
        {
            /// <summary>
            /// The file write type is unknown
            /// </summary>
            Unknown = 0,

            /// <summary>
            /// Create a new file
            /// </summary>
            Create = 1,

            /// <summary>
            /// Append an existing file
            /// </summary>
            Append = 2,

            /// <summary>
            /// Overwrite an existing file
            /// </summary>
            Overwrite = 3
        }

        /// <summary>
        /// File string occurrence type
        /// </summary>
        public enum FileOccurenceType : int
        {
            /// <summary>
            /// The ocurrence type is unknown
            /// </summary>
            Unknown = 0,

            /// <summary>
            /// The first occurrence foundd in the file
            /// </summary>
            FirstOccurence = 1,

            /// <summary>
            /// The last occurrence foundd in the file
            /// </summary>
            LastOccurence = 2,

            /// <summary>
            /// All occurrences foundd in the file
            /// </summary>
            AllOccurences = 3
        }

        /// <summary>
        /// File size information type
        /// </summary>
        public enum FileSizeType : int
        {
            /// <summary>
            /// The file size type is unknown
            /// </summary>
            Unknown = 0,

            /// <summary>
            /// File size in Bytes
            /// </summary>
            Bytes = 1,

            /// <summary>
            /// File size in KiloBytes
            /// </summary>
            KiloBytes = 2,

            /// <summary>
            /// File size in MegaBytes
            /// </summary>
            MegaBytes = 3,

            /// <summary>
            /// File size in GigaBytes
            /// </summary>
            GigaBytes = 4,

            /// <summary>
            /// File size in TeraBytes
            /// </summary>
            TeraBytes = 5
        }

        /// <summary>
        /// File date property information type
        /// </summary>
        public enum FileDatePropertyType : int
        {
            /// <summary>
            /// The date property type is unknown
            /// </summary>
            Unknown = 0,

            /// <summary>
            /// The date of creation
            /// </summary>
            CreationDate = 1,

            /// <summary>
            /// The date last accessed
            /// </summary>
            LastAccessedDate = 2,

            /// <summary>
            /// The date last written to
            /// </summary>
            LastWriteDate = 3
        }

        /// <summary>
        /// Location to append content to
        /// </summary>
        public enum FileAppendLocationType : int
        {
            /// <summary>
            /// The append location type is unknown
            /// </summary>
            Unknown = 0,

            /// <summary>
            /// The top of the file
            /// </summary>
            Top = 1,

            /// <summary>
            /// The bottom of the file
            /// </summary>
            Bottom = 2
        }

        #endregion

        #region Error

        /// <summary>
        /// File error type
        /// </summary>
        public enum FileErrorType : int
        {
            /// <summary>
            /// Unknown error type encountered
            /// </summary>
            Unknown = 0,

            /// <summary>
            /// Exception thrown when dealing with file
            /// </summary>
            ExceptionThrown = 1,

            /// <summary>
            /// Directory does not exist or user does not have permissions to access it
            /// </summary>
            DirectoryExistsError = 2,

            /// <summary>
            /// File does not exist or user does not have permissions to access it
            /// </summary>
            FileExistsError = 3,

            /// <summary>
            /// File cannot be read or user does not have permissions to read it
            /// </summary>
            FileReadError = 4,

            /// <summary>
            /// File cannot be written to or user does not have permissions to write to it
            /// </summary>
            FileWriteError = 5,

            /// <summary>
            /// File cannot be copied or user does not have permissions to access it
            /// </summary>
            FileCopyError = 6,

            /// <summary>
            /// File cannot be deleted or user does not have permissions to delete it
            /// </summary>
            FileDeleteError = 7,

            /// <summary>
            /// File cannot be created or user does not have permissions to create it
            /// </summary>
            FileCreateError = 8,

            /// <summary>
            /// An error was encountered because of the filepath
            /// </summary>
            FilePathError = 9,

            /// <summary>
            /// An error was encountered because of the file extension
            /// </summary>
            FileExtensionError = 10,

            /// <summary>
            /// An error was encountered searching for files
            /// </summary>
            FileSearchError = 11,

            /// <summary>
            /// An error was encountered moving the file
            /// </summary>
            FileMoveError = 12,

            /// <summary>
            /// An error was encountered renaming the file
            /// </summary>
            FileRenameError = 13
        }

        #endregion

        #region Search

        /// <summary>
        /// Type of date search to be performed
        /// </summary>
        public enum FileSearchType : int
        {
            /// <summary>
            /// The type of date to search is unknown
            /// </summary>
            Unknown = 0,

            /// <summary>
            /// The date must be greater than the provided date
            /// </summary>
            GreaterThan = 1,

            /// <summary>
            /// The date must be less than the provided date
            /// </summary>
            LessThan = 2,

            /// <summary>
            /// The date must be between the provided dates
            /// </summary>
            Between = 3
        }

        #endregion

        #endregion
    }
}