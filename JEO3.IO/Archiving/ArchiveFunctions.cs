using System.IO.Compression;

namespace JEO3.IO.Archiving
{
    /// <summary>
    /// Class for generic archive functions
    /// </summary>
    public static class ArchiveFunctions
    {
        #region Create Archive

        /// <summary>
        /// Create a zip file archive
        /// </summary>
        /// <param name="zipFileName">Zipfile name</param>
        /// <returns></returns>
        public static bool CreateArchive(string zipFileName)
        {
            try
            {
                // Create New Archive File
                FileStream streamZipFile = new FileStream(zipFileName, FileMode.CreateNew);

                // Create New ZipArchive
                ZipArchive zipArchive = new ZipArchive(streamZipFile, ZipArchiveMode.Create);

                // Close The Zip File
                streamZipFile.Close();

                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        #endregion

        #region Extract Archive

        /// <summary>
        /// Extract a zip file archive
        /// </summary>
        /// <param name="zipFileName">Zipfile name</param>
        /// <param name="directory">Directory to extract the archive to</param>
        /// <returns></returns>
        public static bool ExtractArchive(string zipFileName, string directory)
        {
            try
            {
                // Check Zip File Exists
                bool zipFileExists = System.IO.File.Exists(zipFileName);

                // Validation
                if (zipFileExists == false) { return false; }

                // Check Output Directory Exists
                bool boolDirectoryExists = System.IO.Directory.Exists(directory);

                // Validation
                if (zipFileExists == false) { return false; }

                // Create File Stream
                using (FileStream fileStream = new FileStream(zipFileName, FileMode.OpenOrCreate))
                {
                    // Create New ZipArchive
                    using (ZipArchive zipEntry = new ZipArchive(fileStream, ZipArchiveMode.Update))
                    {
                        // Extract Zip File to Directory
                        zipEntry.ExtractToDirectory(directory);
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        #endregion

        #region Archive File

        /// <summary>
        /// Archive a file
        /// </summary>
        /// <param name="zipFileName">Zipfile name</param>
        /// <param name="fileName">Filename to archive</param>
        /// <param name="compressionLevel">File compression level</param>
        /// <param name="createIfNotExists">Flag to create the archive file if it does not exist</param>
        /// <returns></returns>
        public static bool ArchiveFile(string zipFileName, string fileName, CompressionLevel compressionLevel, bool createIfNotExists = true)
        {
            try
            {
                // Check Zip File Exists
                bool zipFileExists = System.IO.File.Exists(zipFileName);

                // Validation
                if (zipFileExists == false && createIfNotExists == true)
                {
                    // Attempt Create Zip File
                    bool createResultType = CreateArchive(zipFileName);

                    // Validation
                    if (createResultType == false) { return false; }
                }
                else if (zipFileExists == false && createIfNotExists == false) { return false; }

                // Check File Exists
                bool fileExists = System.IO.File.Exists(fileName);

                // Validation
                if (fileExists == false) { return false; }

                // Get File Short Name
                string shortFileName = System.IO.Path.GetFileNameWithoutExtension(fileName);

                // Validation
                if (shortFileName == "") { return false; }

                // Get File Content
                string strFileContent = System.IO.File.ReadAllText(fileName);

                // Create File Stream
                using (FileStream fileStream = new FileStream(zipFileName, FileMode.OpenOrCreate))
                {
                    // Get Zip Entry
                    using (ZipArchive zipEntry = new ZipArchive(fileStream, ZipArchiveMode.Update))
                    {
                        // Create File
                        ZipArchiveEntry readmeEntry = zipEntry.CreateEntry(shortFileName, compressionLevel);

                        // Create new StreamWriter
                        using (StreamWriter writer = new StreamWriter(readmeEntry.Open()))
                        {
                            // Write File Content
                            writer.Write(strFileContent);
                        }
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        /// <summary>
        /// Archive a list of files retaining the folder hierarchy
        /// </summary>
        /// <param name="zipFileName">Zipfile name</param>
        /// <param name="fileName">Filename to archive</param>
        /// <param name="startFolderName">Zipfile base folder name</param>
        /// <param name="compressionLevel">File compression level</param>
        /// <returns></returns>
        public static bool ArchiveFileWithFolderStructure(string zipFileName, string fileName, string startFolderName, CompressionLevel compressionLevel)
        {
            try
            {
                string relativeFolderPath = "";

                if (startFolderName != "")
                {
                    // Get Relative File Path to Create
                    relativeFolderPath = System.IO.Path.GetRelativePath(fileName, startFolderName);

                    // Validation
                    if (relativeFolderPath == "")
                    {
                        return false;
                    }
                }

                // Create File Stream
                using (FileStream fileStream = new FileStream(zipFileName, FileMode.OpenOrCreate))
                {
                    // Get Zip Entry
                    using (ZipArchive zipEntry = new ZipArchive(fileStream, ZipArchiveMode.Update))
                    {
                        // Get File Info
                        FileInfo fileInfo = new FileInfo(fileName);

                        // Check Create Folder
                        if (relativeFolderPath != "")
                        {
                            // Create Zip File Entry
                            zipEntry.CreateEntry(relativeFolderPath, compressionLevel);
                        }

                        // Get File Short Name
                        string shortFileName = System.IO.Path.GetFileNameWithoutExtension(fileName);

                        // Validation
                        if (shortFileName == "") { return false; }

                        // Get File Content
                        string strFileContent = System.IO.File.ReadAllText(fileName);


                        // Create File
                        ZipArchiveEntry readmeEntry = zipEntry.CreateEntry(relativeFolderPath + shortFileName, compressionLevel);

                        // Create New StreamWriter
                        using (StreamWriter writer = new StreamWriter(readmeEntry.Open()))
                        {
                            // Write File Content
                            writer.Write(strFileContent);
                        }
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        #endregion

        #region Archive File List

        /// <summary>
        /// Archive a list of files
        /// </summary>
        /// <param name="zipFileName">Zipfile name</param>
        /// <param name="listFileNames">List of filenames to archive</param>
        /// <param name="compressionLevel">File compression level</param>
        /// <param name="createIfNotExists">Flag to create the archive file if it does not exist</param>
        /// <returns></returns>
        public static bool ArchiveFileList(string zipFileName, List<string> listFileNames, CompressionLevel compressionLevel, bool createIfNotExists = true)
        {
            try
            {
                // Loop Files
                foreach (string strFile in listFileNames)
                {
                    // Archive File
                    bool archiveFileResultType = ArchiveFile(zipFileName, strFile, compressionLevel, createIfNotExists);

                    // Validation
                    if (archiveFileResultType == false) { return false; }
                }

                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        /// <summary>
        /// Archive a list of files without retaining the folder hierarchy
        /// </summary>
        /// <param name="zipFileName">Zipfile name</param>
        /// <param name="listFiles">List of filenames to archive</param>
        /// <param name="startFolderName">Zipfile base folder name</param>
        /// <param name="compressionLevel">File compression level</param>
        /// <returns></returns>
        public static bool ArchiveFileListWithFolderStructure(string zipFileName, List<string> listFiles, string startFolderName, CompressionLevel compressionLevel)
        {
            try
            {
                string relativeFolderPath = "";

                // Loop Files
                foreach (string fileName in listFiles)
                {
                    // Create File Stream
                    using (FileStream fileStream = new FileStream(zipFileName, FileMode.OpenOrCreate))
                    {
                        // Get Zip Entry
                        using (ZipArchive zipEntry = new ZipArchive(fileStream, ZipArchiveMode.Update))
                        {
                            // Get File Info
                            FileInfo fileInfo = new FileInfo(fileName);

                            // Check Create Folder
                            if (relativeFolderPath != "")
                            {
                                zipEntry.CreateEntry(relativeFolderPath);
                            }

                            // Get File Short Name
                            string shortFileName = System.IO.Path.GetFileNameWithoutExtension(fileName);

                            // Validation
                            if (shortFileName == "") { return false; }

                            // Get File Content
                            string strFileContent = System.IO.File.ReadAllText(fileName);

                            // Create File
                            ZipArchiveEntry readmeEntry = zipEntry.CreateEntry(relativeFolderPath + shortFileName, compressionLevel);

                            // Create New StreamWriter
                            using (StreamWriter writer = new StreamWriter(readmeEntry.Open()))
                            {
                                // Write File Content
                                writer.Write(strFileContent);
                            }
                        }
                    }

                }

                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        #endregion

        #region Archive Directory

        /// <summary>
        /// Archive an entire directory
        /// </summary>
        /// <param name="zipFileName">Zipfile name</param>
        /// <param name="directory">Directory to archive</param>
        /// <param name="recursive">Flag for whether or not to retrieve files to archive recursively (including sub folder files)</param>
        /// <param name="compressionLevel">File compression level</param>
        /// <param name="createIfNotExists">Flag to create the archive file if it does not exist</param>
        /// <returns></returns>
        public static bool ArchiveDirectory(string zipFileName, string directory, bool recursive, CompressionLevel compressionLevel, bool createIfNotExists = true)
        {
            try
            {
                // Retrieve Directory Files
                List<string> listFiles = System.IO.Directory.GetFiles(directory, "*", recursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly).ToList();

                // Validation
                if (listFiles == null) { return false; }

                // Archive Files
                bool archiveDirectoryResultType = ArchiveFileList(zipFileName, listFiles, compressionLevel, createIfNotExists);

                // Validation
                if (archiveDirectoryResultType == false) { return false; }

                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="zipFileName">The File Path of the Zip File to be Archived</param>
        /// <param name="directory">The Directory to Archive</param>
        /// <param name="startFolderName">The Folder in the Directory Path to start with in the Root of the Zip File</param>
        /// <param name="recursive">Whether or not to Recursively Retrieve Files</param>
        /// <param name="compressionLevel">File Compression Level</param>
        /// <returns>Success or Failure</returns>
        public static bool ArchiveDirectoryWithFolderStructure(string zipFileName, string directory, string startFolderName, bool recursive, CompressionLevel compressionLevel)
        {
            try
            {
                // Check Directory Exists
                bool directoryExists = System.IO.Directory.Exists(directory);

                // Validation
                if (directoryExists == false) { return false; }

                // Retrieve Directory Files
                List<string> listFiles = System.IO.Directory.GetFiles(directory, "*", recursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly).ToList();

                // Validation
                if (listFiles == null) { return false; }

                // Archive Files
                bool archiveResultType = ArchiveFileListWithFolderStructure(zipFileName, listFiles, startFolderName, CompressionLevel.Fastest);

                // Validation
                if (archiveResultType == false) { return false; }

                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="zipFileName">Zipfile name</param>
        /// <param name="directory">The directory to archive</param>
        /// <param name="includeBaseDirectory">Flag to include the base directory in the archive</param>
        /// <param name="compressionLevel">File compression level</param>
        /// <returns></returns>
        public static bool ArchiveDirectoryWithFolderStructure(string zipFileName, string directory, bool includeBaseDirectory, CompressionLevel compressionLevel)
        {
            try
            {
                // Check Directory Exists
                bool directoryExists = System.IO.Directory.Exists(directory);

                // Validation
                if (directoryExists == false) { return false; }

                // Create Zip File From Directory
                ZipFile.CreateFromDirectory(directory, zipFileName, compressionLevel, includeBaseDirectory);

                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        #endregion
    }
}