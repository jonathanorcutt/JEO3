namespace JEO3.IO.Foundation
{
    /// <summary>
    /// This interface contains size properties for file system objects
    /// </summary>
    internal interface IFileSizeInformation
    {
        #region Properties

        /// <summary>
        /// The size of the file in Bytes
        /// </summary>
        decimal Bytes { get; }

        /// <summary>
        /// The size of the file in KiloBytes
        /// </summary>
        decimal KiloBytes { get; }

        /// <summary>
        /// The size of the file in MegaBytes
        /// </summary>
        decimal MegaBytes { get; }

        /// <summary>
        /// The size of the file in GigaBytes
        /// </summary>
        decimal GigaBytes { get; }

        #endregion
    }
}
