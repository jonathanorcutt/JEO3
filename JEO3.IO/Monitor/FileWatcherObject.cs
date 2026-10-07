namespace JEO3.IO.Monitor
{
    /// <summary>
    /// FileWatcher class for monitoring filesystem events
    /// </summary>
    public sealed class FileWatcherObject
    {
        #region Properties

        /// <summary>
        /// File directory to monitor
        /// </summary>
        public string FileDirectory { get; } = "";

        /// <summary>
        /// File watcher object
        /// </summary>
        public FileSystemWatcher FileWatcher { get; private set; } = null;

        /// <summary>
        /// File copy directory
        /// </summary>
        public string FileCopyDirectory { get; private set; } = "";


        /// <summary>
        /// File event delegate
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="e"></param>
        public delegate void FileEventMethod(object obj, FileSystemEventArgs e);

        /// <summary>
        /// File renamed delegate
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="e"></param>
        public delegate void FileRenamedEventMethod(object obj, RenamedEventArgs e);

        /// <summary>
        /// File error event delegate
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="e"></param>
        public delegate void FileErrorEventMethod(object obj, ErrorEventArgs e);

        #endregion

        #region Initialization

        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="directory">Directory to monitor</param>
        public FileWatcherObject(string directory)
        {
            FileDirectory = directory;
        }

        #endregion

        #region Watch Directory

        /// <summary>
        /// Start monitoring the file directory for events
        /// </summary>
        /// <param name="boolIncludeSubDirectories">Option to monitor sub-directories</param>
        public void BeginWatchFiles(bool boolIncludeSubDirectories, ref string strError)
        {
            // Create New FileWatcher
            FileWatcher = new FileSystemWatcher(FileDirectory)
            {
                IncludeSubdirectories = boolIncludeSubDirectories,
                Filter = "*.*",
                EnableRaisingEvents = true,
                NotifyFilter = NotifyFilters.Attributes |
                    NotifyFilters.CreationTime |
                    NotifyFilters.FileName |
                    NotifyFilters.LastAccess |
                    NotifyFilters.LastWrite |
                    NotifyFilters.Size |
                    NotifyFilters.Security
            };

            // Add Event Handler Lambda For Logging
            FileWatcher.Changed += new FileSystemEventHandler((obj, e) =>
            {
                string strLogMessage = this.GetFileSystemNotificationMessageFormatted(e);
                Console.WriteLine(strLogMessage);
            });

            // Add Event Handler Lambda For Logging
            FileWatcher.Deleted += new FileSystemEventHandler((obj, e) =>
            {
                string strLogMessage = this.GetFileSystemNotificationMessageFormatted(e);
                Console.WriteLine(strLogMessage);
            });

            // Add Event Handler Lambda For Logging
            FileWatcher.Created += new FileSystemEventHandler((obj, e) =>
            {
                string strLogMessage = this.GetFileSystemNotificationMessageFormatted(e);
                Console.WriteLine(strLogMessage);
            });

            // Add Event Handler Lambda For Logging
            FileWatcher.Renamed += new RenamedEventHandler((obj, e) =>
            {
                string strLogMessage = this.GetFileSystemNotificationMessageFormatted(e);
                Console.WriteLine(strLogMessage);
            });

            // Add Event Handler Lambda For Logging
            FileWatcher.Error += new ErrorEventHandler((obj, e) =>
            {
                string strLogMessage = this.GetFileSystemNotificationMessageFormatted(e);
                Console.WriteLine(strLogMessage);
            });
        }

        #endregion               

        #region Event Subscription

        /// <summary>
        /// Subscribe to file changed event
        /// </summary>
        /// <param name="type">Watcher change type to monitor</param>
        /// <param name="method">Callback method to be invoked on event execution</param>
        public void SubscribeToFileChangedEvent(WatcherChangeTypes type, FileEventMethod method)
        {
            this.FileWatcher.Changed += new FileSystemEventHandler(method);
        }

        /// <summary>
        /// Subscribe to file changed event
        /// </summary>
        /// <param name="type">Watcher change type to monitor</param>
        /// <param name="method">Callback method to be invoked on event execution</param>
        public void SubscribeToFileCreatedEvent(WatcherChangeTypes type, FileEventMethod method)
        {
            // Add Created Event Handler
            this.FileWatcher.Created += new FileSystemEventHandler(method);
        }

        /// <summary>
        /// Subscribe to file deleted event
        /// </summary>
        /// <param name="type">Watcher change type to monitor</param>
        /// <param name="method">Callback method to be invoked on event execution</param>
        public void SubscribeToFileDeletedEvent(WatcherChangeTypes type, FileEventMethod method)
        {
            // Add Deleted Event Handler
            this.FileWatcher.Deleted += new FileSystemEventHandler(method);
        }

        /// <summary>
        /// Subscribe to file renamed event
        /// </summary>
        /// <param name="type">Watcher change type to monitor</param>
        /// <param name="method">Callback method to be invoked on event execution</param>
        public void SubscribeToFileRenamedEvent(WatcherChangeTypes type, FileRenamedEventMethod method)
        {
            // Add Renamed Event Handler
            this.FileWatcher.Renamed += new RenamedEventHandler(method);
        }

        /// <summary>
        /// Subscribe to file error event
        /// </summary>
        /// <param name="type">Watcher change type to monitor</param>
        /// <param name="method">Callback method to be invoked on event execution</param>
        public void SubscribeToFileErrorEvent(WatcherChangeTypes type, FileErrorEventMethod method)
        {
            // Add Error Event Handler
            this.FileWatcher.Error += new ErrorEventHandler(method);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="strOutputDirectory"></param>
        /// <param name="strError"></param>
        public void AddCopyFileTriggerOnFileCreatedOrChanged(string strOutputDirectory, ref string strError)
        {
            FileCopyDirectory = strOutputDirectory;

            // Subscribe To Created Event
            this.SubscribeToFileCreatedEvent(WatcherChangeTypes.Created, this.CopyFile);

            // Subscribe To Changed Event
            this.SubscribeToFileCreatedEvent(WatcherChangeTypes.Changed, this.CopyFile);
        }

        #endregion

        #region Copy File On Event

        /// <summary>
        /// Copy file to destination directory
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="e">Event arguments</param>
        private void CopyFile(object obj, FileSystemEventArgs e)
        {
            //string strError = "";

            //try
            //{
            //    // Get Short File Name
            //    string fileName = FileFunctions.GetFileNameWithExtenstion(e.FullPath, ref strError);

            //    // Validation
            //    if (fileName == "") { return; }

            //    // Check Path Is File
            //    bool boolIsFile = FileFunctions.CheckIsFile(e.FullPath, ref strError);

            //    // Validation
            //    if (boolIsFile == false) { return; }

            //    // Ensure File Still Exists
            //    bool fileExists = FileFunctions.CheckFileExists(e.FullPath, ref strError);

            //    // Copy File
            //    Globals.ResultType copyResult = FileFunctions.CopyFile(e.FullPath, this.m_FileCopyDirectory, fileName, true, ref strError);
            //    string strMessage = "";

            //    // Validation
            //    if (copyResult == false)
            //    {
            //        strMessage = "Error: Could Not Copy File '" + e.FullPath + "' to Directory '" + this.m_FileCopyDirectory + "'. Detail: " + strError;
            //    }
            //    else
            //    {
            //        strMessage = "File '" + e.FullPath + "' was Copied Successfully to Directory '" + this.m_FileCopyDirectory;
            //    }

            //    string strNewFileName = Path.Combine(this.m_FileCopyDirectory, fileName);

            //    //Globals.ResultType addPropertyResult = FileFunctions.SetFileCustomProperty(strNewFileName, "ChangeType", e.ChangeType.ToString(), ref strError);

            //    Console.WriteLine(strMessage);
            //}
            //catch (Exception ex)
            //{
            //    Console.WriteLine(ex.ToString());
            //}
        }

        #endregion

        #region Messages

        /// <summary>
        /// Get file system event notification message formatted
        /// </summary>
        /// <param name="e">Event arguments</param>
        /// <returns></returns>
        private string GetFileSystemNotificationMessageFormatted(FileSystemEventArgs e)
        {
            // Populate Notification Message
            string strMessage =
                "Change Type: " + e.ChangeType.ToString() + "\r\n"
                + "Name: " + e.Name + "\r\n"
                + "Full Path: " + e.FullPath + "\r\n";

            return strMessage;
        }

        /// <summary>
        /// Get file system renamed event notification message formatted
        /// </summary>
        /// <param name="e">Renamed event arguments</param>
        /// <returns></returns>
        private string GetFileSystemNotificationMessageFormatted(RenamedEventArgs e)
        {
            // Populate Notification Message
            string strMessage =
                "Change Type: " + e.ChangeType.ToString() + "\r\n"
                + "Name: " + e.Name + "\r\n"
                + "Full Path: " + e.FullPath + "\r\n"
                + "Old Name: " + e.OldName + "\r\n"
                + "Old Full Path: " + e.OldFullPath + "\r\n";

            return strMessage;
        }

        /// <summary>
        /// Get file system error event notification message formatted
        /// </summary>
        /// <param name="e">Error event arguments</param>
        /// <returns></returns>
        private string GetFileSystemNotificationMessageFormatted(ErrorEventArgs e)
        {
            // Populate Notification Message
            string strMessage = "Exception: " + e.GetException().ToString();

            return strMessage;
        }

        #endregion

        #region Dispose


        #endregion
    }
}
