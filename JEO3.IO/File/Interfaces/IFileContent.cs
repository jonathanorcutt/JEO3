namespace JEO3.IO.File
{
    /// <summary>
    /// FileObject interface
    /// </summary>
    internal interface IFileContent
    {
        #region Properties

        #region File

        /// <summary>
        /// The file object this content resides in
        /// </summary>
        /// <returns>File object that content resides in</returns>
        [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
        FileObject File { get; }

        /// <summary>
        /// The memory stream representing the value of the file content
        /// </summary>
        /// <returns></returns>
        MemoryStream MemoryStream { get; }

        #endregion

        #region Content

        /// <summary>
        /// The value of the file content
        /// </summary>
        /// <returns></returns>
        string Value { get; set; }

        /// <summary>
        /// The list of lines that make up the file content
        /// </summary>
        /// <returns></returns>
        List<string> Lines { get; }

        #endregion

        #endregion

        #region Functions

        /// <summary>
        /// Load the file's content
        /// </summary>
        /// <returns></returns>
        bool Load();

        /// <summary>
        /// Save the content value back to the file
        /// </summary>
        /// <returns></returns>
        bool Save();

        /// <summary>
        /// Refresh the file's content
        /// </summary>
        /// <returns></returns>
        void Refresh();

        #endregion
    }
}
