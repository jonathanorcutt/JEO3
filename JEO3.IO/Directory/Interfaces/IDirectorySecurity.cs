using JEO3.IO.Foundation;

namespace JEO3.IO.Directory
{
    /// <summary>
    /// This class contains file directory security information for a DirectoryObject instance
    /// </summary>
    internal interface IDirectorySecurity : IFileSystemSecurity
    {
        #region Properties

        #region Directory

        /// <summary>
        /// Whether or not the current executing user has access privledges
        /// </summary>
        /// <returns></returns>
        bool CanAccess { get; }

        /// <summary>
        /// Whether or not the current executing user can create files in this directory
        /// </summary>
        /// <returns></returns>
        bool CanCreateFile { get; }

        /// <summary>
        /// Whether or not the current executing user can create directories in this directory
        /// </summary>
        /// <returns></returns>
        bool CanCreateDirectory { get; }

        #endregion

        #endregion
    }
}
