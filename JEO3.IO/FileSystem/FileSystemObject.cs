using JEO3.IO.Directory;

namespace JEO3.IO.Foundation
{
    /// <summary>
    /// Base class for a file system object
    /// </summary>
    public abstract class FileSystemObject : IFileSystemObject, IFileInformation
    {
        #region Properties

        #region Path

        /// <summary>
        /// The fully qualified filepath
        /// </summary>
        public string FilePath
        {
            get;
            set
            {
                // Check Dirty Flag
                if (field != "")
                {
                    IsDirty = true;
                }

                // Set File Path
                field = value;

                // Check Dirty Flag
                if (IsDirty == true)
                {
                    // Refresh File Information
                    this.Refresh();
                    IsDirty = false;
                }
            }
        } = "";

        /// <summary>
        /// The short name of the file system object - No Extension
        /// </summary>
        /// <returns></returns>
        public string Name { get; protected set; } = "";

        #endregion

        #region Relational

        private DirectoryObject m_ParentDirectory;
        /// <summary>
        /// The parent directory of this directory
        /// </summary>
        /// <returns>The parent directory of the file system object</returns>
        public virtual DirectoryObject ParentDirectory
        {
            get
            {
                // Check Is Path Root
                bool boolIsRoot = System.IO.Path.GetPathRoot(this.FilePath) == FilePath;

                // Validation
                if (m_ParentDirectory == null && this.Exists == true && boolIsRoot == false)
                {
                    // Get Directory Path
                    DirectoryInfo directoryInfo = System.IO.Directory.GetParent(this.FilePath);

                    // Validation
                    if (directoryInfo != null && directoryInfo.Exists == true)
                    {
                        // Get Parent Directory
                        this.m_ParentDirectory = new DirectoryObject(directoryInfo.FullName);
                    }
                }

                return this.m_ParentDirectory;
            }
        }

        #endregion

        #region Security

        private FileSystemSecurity m_Permissions;
        /// <summary>
        /// The user permissions for the current executing user
        /// </summary>
        /// <returns></returns>
        public virtual FileSystemSecurity Permissions => this.m_Permissions;

        #endregion

        #region Size

        /// <summary>
        /// Information about the size of the file system object
        /// </summary>
        public FileSizeInformation Size { get; protected set; }

        #endregion

        #region Attributes

        /// <summary>
        /// The attributes the file has
        /// </summary>
        /// <returns></returns>
        public FileAttributes Attributes { get; set; }

        /// <summary>
        /// The date that the file was created
        /// </summary>
        public DateTime CreationDate { get; protected set; } = DateTime.MinValue;

        /// <summary>
        /// The date that the file was last accessed
        /// </summary>
        public DateTime LastAccessedDate { get; protected set; } = DateTime.MinValue;

        /// <summary>
        /// The date that the file was last written to
        /// </summary>
        public DateTime LastWriteDate { get; protected set; } = DateTime.MinValue;

        #endregion

        #region Validation

        /// <summary>
        /// Flag indicating whether or not the file exists in the file system
        /// </summary>
        public abstract bool Exists { get; }

        /// <summary>
        /// Whether or not this objects properties have been modified and its property information needs to be refreshed
        /// </summary>
        /// <returns></returns>
        protected bool IsDirty { get; private set; }

        /// <summary>
        /// Returns whether this file object is read only
        /// </summary>
        /// <returns></returns>
        public bool IsReadOnly => (this.Attributes & FileAttributes.ReadOnly) != 0;

        #endregion

        #endregion

        #region Initialization

        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="strPath">File path of the system file or directory</param>
        protected FileSystemObject(string strPath)
        {
            // Validation
            if (strPath == "") { return; }

            // Set File Path
            this.FilePath = strPath;

            // Load Properties
            this.LoadProperties();
        }

        /// <summary>
        /// Load the properties for this file object
        /// </summary>
        /// <returns></returns>
        private bool LoadProperties()
        {
            try
            {
                #region Attributes

                // Get File Information
                FileInfo info = new FileInfo(FilePath);

                // Get Short Name
                Name = info.Name;

                // Get Attributes
                this.Attributes = info.Attributes;

                // Get File Creation Date
                CreationDate = System.IO.File.GetCreationTime(this.FilePath);

                // Get File Last Accessed Date
                LastAccessedDate = System.IO.File.GetLastAccessTime(this.FilePath);

                // Get File Last Write Date
                LastWriteDate = System.IO.File.GetLastWriteTime(this.FilePath);

                #endregion

                #region Security

                // Get Security Rules
                this.m_Permissions = new FileSystemSecurity(this);

                #endregion

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);

                return false;
            }
        }

        #endregion

        #region Abstract Methods

        /// <summary>
        /// Refresh the properties of this file
        /// </summary>        
        /// <returns></returns>
        public abstract void Refresh();

        /// <summary>
        /// Delete this file
        /// </summary>
        /// <returns></returns>
        public abstract bool Delete();

        /// <summary>
        /// Rename this file
        /// </summary>
        /// <param name="strNewName">New filename</param>
        /// <returns></returns>
        public abstract bool Rename(string strNewName);

        /// <summary>
        /// Refresh the properties of this file
        /// </summary>        
        /// <returns></returns>
        public abstract bool Move(DirectoryObject destinationDirectoryObject);

        #endregion        
    }
}
