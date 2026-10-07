namespace JEO3.IO.Foundation
{
    /// <summary>
    /// This class holds file system object size information
    /// </summary>
    public sealed class FileSizeInformation : IFileSizeInformation
    {

        #region Properties

        /// <summary>
        /// The size of the file in Bytes
        /// </summary>
        public decimal Bytes
        {
            get =>
              // Validation
              field <= 0 ? 0 : (field);
        } = 0;

        /// <summary>
        /// The size of the file in KiloBytes
        /// </summary>
        public decimal KiloBytes =>
            // Validation
            this.Bytes <= 0 ? 0 : this.Bytes / 1024;

        /// <summary>
        /// The size of the file in MegaBytes
        /// </summary>
        public decimal MegaBytes =>
            // Validation
            this.KiloBytes <= 0 ? 0 : this.KiloBytes / 1024;

        /// <summary>
        /// The size of the file in GigaBytes
        /// </summary>
        public decimal GigaBytes =>
            // Validation
            this.MegaBytes <= 0 ? 0 : this.MegaBytes / 1024;

        #endregion

        #region Initialization

        internal FileSizeInformation(decimal decBytes)
        {
            Bytes = decBytes;
        }

        #endregion
    }
}
