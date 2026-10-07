namespace JEO3.IO.Foundation
{
    /// <summary>
    /// This interface contains attribute properties for file system objects
    /// </summary>
    internal interface IFileInformation
    {
        #region Properties

        /// <summary>
        /// The attributes the file has
        /// </summary>
        /// <returns></returns>
        FileAttributes Attributes { get; set; }

        /// <summary>
        /// The date that the file was created
        /// </summary>
        DateTime CreationDate { get; }

        /// <summary>
        /// The date that the file was last accessed
        /// </summary>
        DateTime LastAccessedDate { get; }

        /// <summary>
        /// The date that the file was last written to
        /// </summary>
        DateTime LastWriteDate { get; }

        #endregion
    }
}
