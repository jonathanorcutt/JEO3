namespace JEO3.IO.Foundation
{
    /// <summary>
    /// This interface contains the must implement properties for file system security
    /// </summary>
    internal interface IFileSystemSecurity
    {
        #region Properties

        /// <summary>
        /// Whether or not the current executing user has read privledges
        /// </summary>
        /// <returns></returns>
        bool CanRead { get; }

        /// <summary>
        /// Whether or not the current executing user has write privledges
        /// </summary>
        /// <returns></returns>
        bool CanWrite { get; }

        /// <summary>
        /// Whether or not the current executing user has delete privledges
        /// </summary>
        /// <returns></returns>
        bool CanDelete { get; }

        /// <summary>
        /// Whether or not the current executing user has modify privledges
        /// </summary>
        /// <returns></returns>
        bool CanModify { get; }

        /// <summary>
        /// Whether or not the current executing user has execute privledges
        /// </summary>
        /// <returns></returns>
        bool CanExecute { get; }

        #endregion

        #region Functions

        /// <summary>
        /// Refresh the security priveldges for the file system object
        /// </summary>
        void Refresh();

        #endregion
    }
}
