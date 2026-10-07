using JEO3.IO.Directory;

namespace JEO3.IO.Foundation
{
    /// <summary>
    /// This interface the core file system object properties and methods
    /// </summary>
    internal interface IFileSystemObject
    {
        #region Properties

        #region Name / Path

        /// <summary>
        /// The short name of the file system object without the file extension
        /// </summary>
        /// <returns></returns>
        string Name { get; }

        /// <summary>
        /// The fully qualified path of the file system object
        /// </summary>
        string FilePath { get; set; }

        #endregion

        #region Relational

        /// <summary>
        /// The parent directory of this directory
        /// </summary>
        /// <returns>The parent directory of the file system object</returns>
        DirectoryObject ParentDirectory { get; }

        #endregion

        #region Validation

        /// <summary>
        /// Flag indicating whether or not the file exists in the file system
        /// </summary>
        bool Exists { get; }

        #endregion

        #endregion

        #region Functions

        #region Action

        /// <summary>
        /// Refresh the properties of this file
        /// </summary>
        /// <returns></returns>
        void Refresh();

        #endregion

        #endregion
    }
}
