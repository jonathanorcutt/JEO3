namespace JEO3.IO.File
{
    /// <summary>
    /// File object interface
    /// </summary>
    internal interface IFileObject
    {
        #region Properties

        /// <summary>
        /// The short name of the file system object including the file extension
        /// </summary>
        /// <returns></returns>
        string FullName { get; }

        /// <summary>
        /// The type of extension the file has
        /// </summary>
        string Extension { get; }

        /// <summary>
        /// The contents of the file
        /// </summary>
        FileContent Content { get; }

        #endregion
    }
}
