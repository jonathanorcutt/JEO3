using JEO3.IO.Directory;

namespace JEO3.IO.File
{
    /// <summary>
    /// This class provides extensions methods for file object lists
    /// </summary>
    public static class FileObjectListExtensions
    {
        #region Archive

#if NET45
        /// <summary>
        /// Archive a list of files
        /// </summary>
        /// <param name="fileObjectList">Files to archive</param>
        /// <param name="directoryObject">Directory to create archive file in</param>
        /// <param name="strArchiveName">Directory to create archive file in</param>
        /// <param name="compressionLevel">File compression level</param>
        /// <param name="fileMode">File mode. Default is to create a new archive file or add to an existing archive</param>
        /// <returns></returns>
        public static Globals.ResultType Archive(this List<FileObject> fileObjectList, DirectoryObject directoryObject, string strArchiveName, 
            CompressionLevel compressionLevel, FileMode fileMode = FileMode.OpenOrCreate)
        {
            try
            { 
                // Loop Files
                foreach (FileObject fileObject in fileObjectList)
                {
                    // Archive File
                    Globals.ResultType archiveFileResultType = fileObject.Archive(compressionLevel, fileMode);

                    // Validation
                    if (archiveFileResultType == false) { return false; }
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

#endif

        #endregion

        #region Copy Files

        /// <summary>
        /// Copy a file to a different directory
        /// </summary>
        /// <param name="fileObjectList">File list to be copied</param>
        /// <param name="directoryObject">Directory to be copied to</param>
        /// <param name="boolOverWrite">Whether or not to overwrite any existing file of the same name</param>
        /// <param name="boolDeleteOriginal">Optional: Delete the original file being copied</param>
        /// <param name="boolReLoadFilesFromDestinationDirectory">Optional: Flag to reload file objects information from the destination directory</param>
        /// <returns></returns>
        public static bool Copy(this List<FileObject> fileObjectList, DirectoryObject directoryObject, bool boolOverWrite, bool boolDeleteOriginal = false, bool boolReLoadFilesFromDestinationDirectory = false)
        {
            try
            {
                bool copyResult = true;

                // Loop Files
                foreach (FileObject fileObject in fileObjectList)
                {
                    // Copy File
                    bool copyFileResult = fileObject.Copy(directoryObject, fileObject.FullName, boolOverWrite, boolDeleteOriginal);

                    // Validation
                    if (boolReLoadFilesFromDestinationDirectory == true)
                    {
                        // Refresh File Object
                        fileObject.Refresh();
                    }

                    // Validation
                    copyResult = (copyFileResult == false) ? false : true;
                }

                return copyResult;
            }
            catch (Exception ex)
            {
                // To Be Implemented: Throw Custom Exception...
                Console.WriteLine(ex.ToString());
                return false;
            }
        }

        #endregion

        #region Move Files

        /// <summary>
        /// Move a file to a different directory
        /// </summary>
        /// <param name="fileObjectList">File list to be moved</param>
        /// <param name="directoryObject">Directory to be copied to</param>
        /// <returns></returns>
        public static bool Move(this List<FileObject> fileObjectList, DirectoryObject directoryObject)
        {
            var moveResult = true;

            // Loop Files
            foreach (FileObject fileObject in fileObjectList)
            {
                // Copy File
                bool moveFileResult = fileObject.Move(directoryObject, fileObject.FullName);

                // Refresh File Object
                fileObject.Refresh();

                // Validation
                moveResult = (moveFileResult == false) ? false : true;
            }

            return moveResult;
        }

        #endregion

        #region Delete Files

        /// <summary>
        /// Delete a file
        /// </summary>
        /// <param name="fileObjectList">File list to be deleted</param>        
        /// <returns></returns>
        public static bool Delete(this List<FileObject> fileObjectList)
        {
            try
            {
                // Attempt File Deletion
                fileObjectList.ForEach(fileObject =>
                    System.IO.File.Delete(fileObject.FilePath));

                // Refresh Files
                fileObjectList.ForEach(fileObject => fileObject.Refresh());

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
    }
}
