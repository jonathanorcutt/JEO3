using JEO3.IO.File;
using JEO3.IO.Foundation;
using JEO3.IO.Information;

namespace JEO3.IO.Directory
{
    /// <summary>
    /// This class represents a filesystem directory 
    /// </summary>
    public sealed class DirectoryObject : FileSystemObject, IFileSystemObject
    {
        #region Properties

        #region All Children

        /// <summary>
        /// List of all directories within this directory. This property loads recusrively, so the first instantiation may be expensive
        /// </summary>
        public DirectoryObjectList AllDirectories
        {
            get
            {
                // Validation
                if (field == null)
                {
                    // Get SubDirectories
                    field = this.GetSubDirectories("*", true);
                }

                return field;
            }
        }

        /// <summary>
        /// List of all files within this directory. This property loads recusrively, so the first instantiation may be expensive
        /// </summary>
        public FileObjectList AllFiles
        {
            get
            {
                // Validation
                if (field == null)
                {
                    // Get SubDirectories
                    field = this.GetFiles("*", true);
                }

                return field;
            }
        }

        #endregion

        #region Sub Directories

        /// <summary>
        /// List of sub directories within this directory
        /// </summary>
        public DirectoryObjectList SubDirectories
        {
            get
            {
                // Validation
                if (field == null)
                {
                    // Get SubDirectories
                    field = this.GetSubDirectories("*", false);
                }

                return field;
            }
        }

        #endregion

        #region Files

        /// <summary>
        /// List of filenames within this directory
        /// </summary>
        public FileObjectList Files
        {
            get
            {
                // Validation
                if (field == null)
                {
                    // Get Files
                    field = this.GetFiles("*", false);
                }

                return field;
            }
        }

        #endregion

        #region Validation

        /// <summary>
        /// Overrides the base method and refreshes the exists property
        /// </summary>
        public sealed override bool Exists =>
            // Refresh Exists Property
            this.CheckExists();

        #endregion

        #region Permissions

        /// <summary>
        /// Directory permissions
        /// </summary>
        public new DirectorySecurity Permissions { get; private set; }

        #endregion

        #endregion

        #region Initialization

        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="fileDirectory">Directory filepath</param>
        public DirectoryObject(string fileDirectory)
            : base(fileDirectory)
        {
            // Load Directory Properties
            this.LoadProperties();
        }

        /// <summary>
        /// Load the properties for this file directory
        /// </summary>
        /// <returns></returns>
        private void LoadProperties()
        {
            try
            {
                #region Validation

                // Validation
                if (this.Exists == false) { return; }

                #endregion

                #region Size

                // Get Short Name
                this.Name = new DirectoryInfo(this.FilePath).Name;

                decimal decSize = this.GetSize(FileInformation.FileSizeType.Bytes, false);

                // Get Directory Size In Bytes
                this.Size = new FileSizeInformation(decSize);

                #endregion

                #region Permissions

                Permissions = new DirectorySecurity(this);

                #endregion
            }
            catch (Exception ex)
            {
                string strError = "An Error Occurred In Method '" + System.Reflection.MethodBase.GetCurrentMethod().Name + "'. Detail: " + ex.Message;
                Console.WriteLine(strError);
            }
        }

        #endregion

        #region Validation

        /// <summary>
        /// Check file directory exists
        /// </summary>
        /// <returns></returns>
        private bool CheckExists()
        {
            try
            {
                // Check Directory Exists
                bool boolExits = System.IO.Directory.Exists(this.FilePath);

                return boolExits;
            }
            catch (Exception ex)
            {
                // To Be Implemented: Throw Custom Exception...
                Console.WriteLine(ex.ToString());
                return false;
            }
        }

        #endregion

        #region Refresh

        /// <summary>
        /// Refresh the contents of this directory
        /// </summary>
        /// <returns></returns>
        public override void Refresh()
        {
            // Delete File
            this.LoadProperties();
        }

        #endregion

        #region Actions

        #region Archive

#if NET45
        /// <summary>
        /// Archive an entire directory
        /// </summary>
        /// <param name="destinationDirectoryObject">Destination directory</param>
        /// <param name="compressionLevel">File compression level</param>
        /// <param name="fileMode">File mode. Default is to create a new archive file or add to an existing archive</param>
        /// <returns></returns>
        public bool ArchiveDirectory(
            DirectoryObject destinationDirectoryObject, CompressionLevel compressionLevel, FileMode fileMode = FileMode.OpenOrCreate)
        {
            try
            {
                // Create Archive
                ZipFile.CreateFromDirectory(this.FilePath, destinationDirectoryObject.FilePath, compressionLevel, true);

                return true;
            }
            catch (Exception ex)
            {
                // To Be Implemented: Throw Custom Exception...
                Console.WriteLine(ex.ToString());
                return false;
            }
        }
#endif

        #endregion

        #region Delete

        /// <summary>
        /// Delete this directory
        /// </summary>        
        /// <returns></returns>
        public sealed override bool Delete()
        {
            try
            {
                // Attempt File Deletion
                System.IO.Directory.Delete(this.FilePath, true);

                return true;
            }
            catch (Exception ex)
            {
                // To Be Implemented: Throw Custom Exception...
                Console.WriteLine(ex.ToString());

                return false;
            }
        }

        /// <summary>
        /// Deletes files from a directory
        /// </summary>
        /// <param name="recurse">Flag that determines whether files are retrieved recursively including files from sub-directories</param>
        /// <returns></returns>
        public bool DeleteFiles(bool recurse)
        {
            // Get Files
            FileObjectList listFiles = this.GetFiles(recurse);

            // Delete Files
            bool deleteResult = listFiles.Delete();

            return deleteResult;
        }

        #endregion

        #region Rename

        /// <summary>
        /// Rename this directory
        /// </summary>
        /// <param name="strNewName">New file directory name</param>
        /// <returns></returns>
        public sealed override bool Rename(string strNewName)
        {
            try
            {
                // Get New File Path
                string strNewPath = Path.Combine(this.ParentDirectory.FilePath, strNewName);

                // Copy Existing Directory With New Name
                System.IO.Directory.Move(this.FilePath, strNewPath);

                // Set New File Path
                this.FilePath = strNewPath;

                // Refresh File
                this.Refresh();

                return true;
            }
            catch (Exception ex)
            {
                // To Be Implemented: Throw Custom Exception...
                Console.WriteLine(ex.ToString());
                return false;
            }
        }

        #endregion

        #region Create

        /// <summary>
        /// Create a new directory
        /// </summary>
        /// <param name="fileSystemRights">Type of file system rights to be set on the new directory</param>
        /// <param name="boolAllowEveryone">Flag to include Everyone permissions on the new directory</param>
        /// <returns></returns>
        public bool Create(System.Security.AccessControl.FileSystemRights fileSystemRights, bool boolAllowEveryone = false)
        {
            try
            {
                // Create Directory
                System.IO.DirectoryInfo directoryInfo = System.IO.Directory.CreateDirectory(this.FilePath);

                // Validation
                if (directoryInfo == null || directoryInfo.Exists == false) { return false; }

                // Set Directory Permissions
                bool createResultType = this.SetPermissions(fileSystemRights, System.Security.AccessControl.AccessControlType.Allow);

                // Validation
                return createResultType != false;
            }
            catch (Exception ex)
            {
                // To Be Implemented: Throw Custom Exception...
                Console.WriteLine(ex.ToString());
                return false;
            }
        }

        #endregion

        #region Move

        /// <summary>
        /// Moves a file directory to a destination location
        /// </summary>
        /// <param name="destinationDirectoryObject">Destination for directory to be moved to</param>
        /// <returns></returns>
        public sealed override bool Move(DirectoryObject destinationDirectoryObject)
        {
            try
            {
                // Validation
                if (destinationDirectoryObject.Exists == false) { return false; }

                // Get Directory Info
                System.IO.DirectoryInfo directoryInfo = new DirectoryInfo(this.FilePath);

                // Move Directory
                directoryInfo.MoveTo(destinationDirectoryObject.FilePath);

                return true;
            }
            catch (Exception ex)
            {
                // To Be Implemented: Throw Custom Exception...
                Console.WriteLine(ex.ToString());
                return false;
            }
        }

        #endregion

        #region Copy

        /// <summary>
        /// Copies a file directory to a destination directory
        /// </summary>
        /// <param name="destinationDirectoryObject">Destination path for directory to be copied to</param> 
        /// <returns></returns>
        public bool Copy(DirectoryObject destinationDirectoryObject)
        {
            return this.Copy(destinationDirectoryObject, this);
        }

        /// <summary>
        /// Copies a file directory recursively to a destination directory
        /// </summary>
        /// <param name="destinationDirectoryObject">Destination path for directory to be copied to</param> 
        /// <param name="directoryToCopy">Directory to be copied</param>
        /// <returns></returns>
        private bool Copy(DirectoryObject destinationDirectoryObject, DirectoryObject directoryToCopy)
        {
            try
            {
                // Get New Path
                string strNewDirectory = Path.Combine(destinationDirectoryObject.FilePath, directoryToCopy.Name);

                // Get Directory
                DirectoryObject directory = new DirectoryObject(strNewDirectory);

                // Validation
                if (directory.Exists == false)
                {
                    // Create Directory
                    var createResult = directory.Create(System.Security.AccessControl.FileSystemRights.Write);

                    // Validation
                    if (createResult == false) { return false; }
                }

                // Copy All Files To New Directory
                this.Files.ForEach(file => file.Copy(directory, true));

                // Loop SubDirectories
                foreach (DirectoryObject childDirectory in directoryToCopy.SubDirectories)
                {
                    // Copy Directory
                    var copyResult = directoryToCopy.Copy(directory, childDirectory);

                    // Validation
                    if (copyResult == false) { return false; }
                }

                return true;
            }
            catch (Exception ex)
            {
                // To Be Implemented: Throw Custom Exception...
                Console.WriteLine(ex.ToString());
                return false;
            }
        }

        /// <summary>
        /// Copies directory files to a destination directory
        /// </summary>
        /// <param name="destinationDirectoryObject">Destination for directory files to be copied to</param>
        /// <param name="find">Search pattern to use retrieving files</param>
        /// <param name="recurse">Flag that determines whether files are retrieved recursively including files from sub-directories</param>
        /// <param name="boolDeleteOriginal">Flag that determines whether to delete the original files</param>
        /// <returns></returns>
        public bool CopyFiles(DirectoryObject destinationDirectoryObject, string find, bool recurse, bool boolDeleteOriginal = false)
        {
            return this.CopyFiles(destinationDirectoryObject, find, recurse, boolDeleteOriginal);
        }

        /// <summary>
        /// Copies directory files to a destination directory
        /// </summary>
        /// <param name="destinationDirectoryObject">Destination for directory files to be copied to</param>
        /// <param name="boolOverWrite">Flag for whether or not to overwrite any existing file in the destination directory</param>
        /// <param name="find">Search pattern to use retrieving files</param>
        /// <param name="recurse">Flag that determines whether files are retrieved recursively including files from sub-directories</param>
        /// <param name="boolDeleteOriginal">Flag that determines whether to delete the original files</param>
        /// <returns></returns>
        public bool CopyFiles(DirectoryObject destinationDirectoryObject, bool boolOverWrite, string find, bool recurse, bool boolDeleteOriginal = false)
        {
            // Get Files
            FileObjectList listFiles = this.GetFiles(find, recurse);
            var copyResult = true;

            // Loop Files
            foreach (FileObject file in listFiles)
            {
                // Copy File
                bool copyFileResult = file.Copy(destinationDirectoryObject, boolOverWrite, boolDeleteOriginal);

                // Validation
                copyResult = (copyFileResult != true) ? false : true;
            }

            return copyResult;
        }

        #endregion

        #endregion

        #region Get Files

        #region Synchronously

        /// <summary>
        /// Get file in directory by name
        /// </summary>
        /// <param name="strFileShortName">File name to retrieve, not case sensitive. The name may be with or without extension (e.g. "ReadMe.txt" or just "ReadMe")</param>
        /// <returns></returns>
        public FileObject File(string strFileShortName)
        {
            return this.Files.Where(file =>
                file.FullName.ToLower() == strFileShortName.ToLower() ||
                file.Name.ToLower() == strFileShortName.ToLower())
                .FirstOrDefault();
        }

        /// <summary>
        /// Retrieves files from a file directory
        /// </summary>
        /// <param name="recurse">Flag that determines whether files are retrieved recursively including files from sub-directories</param>
        /// <returns></returns>
        public FileObjectList GetFiles(bool recurse)
        {
            // Create Directory List
            FileObjectList files = this.GetFiles("*", recurse);

            return files;
        }

        /// <summary>
        /// Retrieves files from a file directory
        /// </summary>        
        /// <param name="find">Search string to find</param>
        /// <param name="recurse">Flag that determines whether files are retrieved recursively including files from sub-directories</param>
        /// <returns></returns>
        public FileObjectList GetFiles(string find, bool recurse)
        {
            try
            {
                // Determine Search Pattern
                find = (find == null || find == "") ? "*" : find;
                find = (find.Length > 0 && find[0].ToString() != "*") ? "*" + find + "*" : find;

                // Determine Search Option
                System.IO.SearchOption option = (recurse == true) ? System.IO.SearchOption.AllDirectories : System.IO.SearchOption.TopDirectoryOnly;

                // Get Directory Files
                List<string> listFileNames = System.IO.Directory.GetFiles(this.FilePath, find, option).ToList();

                // Create File List
                FileObjectList files = new FileObjectList(listFileNames);

                return files;
            }
            catch (UnauthorizedAccessException e)
            {
                // To Be Implemented: Throw Custom Exception...
                Console.WriteLine(e.Message);
                return null;
            }
            catch (DirectoryNotFoundException e)
            {
                // To Be Implemented: Throw Custom Exception...
                Console.WriteLine(e.Message);
                return null;
            }
            catch (IOException e)
            {
                // To Be Implemented: Throw Custom Exception...
                Console.WriteLine(e.Message);
                return null;
            }
            catch (Exception ex)
            {
                // To Be Implemented: Throw Custom Exception...
                Console.WriteLine(ex.ToString());
                return null;
            }
        }
        public FileObjectList GetFiles(string[] find, bool recurse)
        {
            try
            {
                // Determine Search Pattern
                var finds = (find == null || find.Any() == false) ? "*" : string.Join(",", find);

                // Determine Search Option
                System.IO.SearchOption option = (recurse == true) ? System.IO.SearchOption.AllDirectories : System.IO.SearchOption.TopDirectoryOnly;

                // Get Directory Files
                List<string> listFileNames = System.IO.Directory.GetFiles(this.FilePath, finds, option).ToList();

                // Create File List
                FileObjectList files = new FileObjectList(listFileNames);

                return files;
            }
            catch (UnauthorizedAccessException e)
            {
                // To Be Implemented: Throw Custom Exception...
                Console.WriteLine(e.Message);
                return null;
            }
            catch (DirectoryNotFoundException e)
            {
                // To Be Implemented: Throw Custom Exception...
                Console.WriteLine(e.Message);
                return null;
            }
            catch (IOException e)
            {
                // To Be Implemented: Throw Custom Exception...
                Console.WriteLine(e.Message);
                return null;
            }
            catch (Exception ex)
            {
                // To Be Implemented: Throw Custom Exception...
                Console.WriteLine(ex.ToString());
                return null;
            }
        }


        #endregion

        #region Asynchronous

        /// <summary>
        /// Retrieves files from a file directory. 
        /// </summary>
        /// <param name="searchInformation">Search information to query the directory with</param>
        /// <param name="callback">Callback method to be invoked after each provided timeout period elapses</param>
        /// <param name="intCallbackTimeoutSecounds">Number of seconds to wait before invoking the callback method</param>
        /// <returns></returns>
        public FileObjectList GetFilesAsync(SearchInformation searchInformation, SearchInformation.SearchCallback callback, int intCallbackTimeoutSecounds = 10)
        {
            try
            {
                // Validation
                FileObjectList fileObjectList = [];

                // Set Initial Search
                DateTime lastDate = DateTime.Now;

                // Create Directories Searched
                DirectoryObjectList directoriesSearched = [];

                // Get File Asynchronously
                this.GetFilesAsync(searchInformation, callback, ref fileObjectList, ref directoriesSearched, ref lastDate, intCallbackTimeoutSecounds);

                return fileObjectList;
            }
            catch (Exception ex)
            {
                // To Be Implemented: Throw Custom Exception...
                Console.WriteLine(ex.ToString());
                return null;
            }
        }

        /// <summary>
        /// Retrieves files from a file directory. This method is recursive and requires a helper method to ensure proper encapsulation
        /// </summary>
        /// <param name="searchInformation">Search information to query the directory with</param>
        /// <param name="callback">Callback method to be invoked after each provided timeout period elapses</param>
        /// <param name="fileObjectList">Number of seconds to wait before invoking the callback method</param>
        /// <param name="directoriesSearched">The directories that have been searched</param>
        /// <param name="intCallbackTimeoutSecounds">Number of seconds to wait before invoking the callback method</param>
        /// <param name="lastDate">Last time the callback was invoked</param>
        /// <returns></returns>
        [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
        private FileObjectList GetFilesAsync(SearchInformation searchInformation, SearchInformation.SearchCallback callback,
            ref FileObjectList fileObjectList, ref DirectoryObjectList directoriesSearched, ref DateTime lastDate, int intCallbackTimeoutSecounds = 10)
        {
            try
            {
                #region Get Files

                // Get Directory Files - Top Level Only
                List<string> listFileNames = System.IO.Directory.GetFiles(this.FilePath, searchInformation.SearchPattern, System.IO.SearchOption.TopDirectoryOnly).ToList();

                // Create File List
                FileObjectList listFiles = new FileObjectList(listFileNames);

                // Validation
                if (listFiles.Count > 0)
                {
                    // Filter Files By Search Information
                    List<FileObject> filesToAdd = searchInformation.GetSearchFilesFiltered(listFiles);

                    // Create File List                
                    fileObjectList.AddRange(filesToAdd.ToArray());

                    // Distinct List
                    fileObjectList = [.. fileObjectList.Distinct().ToList()];
                }

                // Add Directory To Searched List
                directoriesSearched.Add(this);

                #endregion

                #region Recurse Sub Directories

                // Loop SubDirectories
                foreach (DirectoryObject directory in this.SubDirectories)
                {
                    // Get Sub Directory Files
                    directory.GetFilesAsync(searchInformation, callback, ref fileObjectList, ref directoriesSearched, ref lastDate, intCallbackTimeoutSecounds);
                }

                #endregion

                #region Invoke Callback

                // Check Time For Callback
                if (fileObjectList != null && DateTime.Now > lastDate.AddSeconds(intCallbackTimeoutSecounds))
                {
                    // Invoke Callback
                    callback.Invoke(fileObjectList, directoriesSearched);
                    lastDate = DateTime.Now;
                }

                #endregion

                return fileObjectList;
            }
            catch (Exception ex)
            {
                // To Be Implemented: Throw Custom Exception...
                Console.WriteLine(ex.ToString());
                return null;
            }
        }

        #endregion

        #endregion

        #region Get Directories

        /// <summary>
        /// Retrieves a list of sub-directory paths from a file directory
        /// </summary>        
        /// <param name="strFolderName">Folder name to retrieve</param>        
        /// <returns></returns>
        public DirectoryObject SubDirectory(string strFolderName)
        {
            return this.SubDirectories.Where(directory => directory.Name == strFolderName).FirstOrDefault();
        }

        /// <summary>
        /// Retrieves a list of sub-directory paths from a file directory
        /// </summary>
        /// <param name="recurse">Flag that determines whether files are retrieved recursively including files from sub-directories</param>
        /// <returns></returns>
        public DirectoryObjectList GetSubDirectories(bool recurse)
        {
            // Create Directory List
            DirectoryObjectList directories = this.GetSubDirectories("*", recurse);

            return directories;
        }

        /// <summary>
        /// Retrieves a list of sub-directory paths from a file directory
        /// </summary>
        /// <param name="find">Search string to find</param>
        /// <param name="recurse">Flag that determines whether files are retrieved recursively including files from sub-directories</param>
        /// <returns></returns>
        public DirectoryObjectList GetSubDirectories(string find, bool recurse)
        {
            try
            {
                // Determine Search Pattern
                find = (find != "") ? "*" + find + "*" : "";

                // Determine Search Option
                System.IO.SearchOption option = (recurse == true) ? System.IO.SearchOption.AllDirectories : System.IO.SearchOption.TopDirectoryOnly;

                // Get File Directories
                List<string> listDirectories = System.IO.Directory.GetDirectories(this.FilePath, find, option).ToList();

                // Create Directory List
                DirectoryObjectList directories = new DirectoryObjectList(listDirectories);

                return directories;
            }
            catch (Exception ex)
            {
                // To Be Implemented: Throw Custom Exception...
                Console.WriteLine(ex.ToString());
                return null;
            }
        }

        #endregion
    }
}