using System.Data;
using JEO3.Extensions;
using JEO3.IO.Directory;
using JEO3.IO.Foundation;
using JEO3.IO.Information;

namespace JEO3.IO.File
{
    /// <summary>
    /// This class represents a file system file
    /// </summary>
    public sealed class FileObject : FileSystemObject, IFileObject
    {
        #region Properties

        #region File Path

        /// <summary>
        /// The short filename relative to the parent directory including the file extension
        /// </summary>
        public string FullName { get; private set; } = "";

        /// <summary>
        /// The type of extension the file has - Includes Period Suffix
        /// </summary>
        public string Extension { get; private set; } = "";

        #endregion

        #region Content

        /// <summary>
        /// The contents of the file
        /// </summary>
        public FileContent Content
        {
            get
            {
                // Validation
                if (field != null && field.Value == "")
                {
                    // Load Content
                    field.Load();
                }

                return field;
            }

            private set;
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

        #endregion

        #region Initialization

        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="fileName">Filename to use for this object</param>
        public FileObject(string fileName)
            : base(fileName)
        {
            // Set File Properties
            this.LoadProperties();
        }

        /// <summary>
        /// Load the properties for this file
        /// </summary>
        /// <returns></returns>
        private bool LoadProperties()
        {
            try
            {
                #region Validation

                // Validation
                if (this.Exists == false) { return false; }

                #endregion

                #region Path

                // Get File Name With Extension
                FullName = this.GetNameWithExtenstion();

                // Get File Name Without Extension
                this.Name = this.GetNameWithOutExtenstion();

                // Get File Extention
                Extension = this.GetExtension();

                #endregion

                #region Size

                // Get Directory Size In Bytes
                decimal decSize = new FileInfo(this.FilePath).Length;

                // Get Directory Size In Bytes
                this.Size = new FileSizeInformation(decSize);

                #endregion

                #region Content

                // Set File Content Information
                Content = new FileContent(this);

                #endregion

                return true;
            }
            catch (Exception ex)
            {
                string strError = "An Error Occurred In Method '" + System.Reflection.MethodBase.GetCurrentMethod().Name + "'. Detail: " + ex.Message;
                Console.WriteLine(strError);

                return false;
            }
        }

        #endregion

        #region Refresh

        /// <summary>
        /// Refresh the properties of this file
        /// </summary>
        /// <returns></returns>
        public sealed override void Refresh()
        {
            // Delete File
            this.LoadProperties();
        }

        #endregion

        #region Validation

        /// <summary>
        /// Check to see if the file exists
        /// </summary>
        /// <returns></returns>
        private bool CheckExists()
        {
            try
            {
                // Check File Exists
                bool boolExists = System.IO.File.Exists(this.FilePath);

                return boolExists;
            }
            catch (Exception ex)
            {
                // To Be Implemented: Throw Custom Exception...
                Console.WriteLine(ex.ToString());
                return false;
            }
        }

        #endregion

        #region Actions

        #region Archive

#if NET45
        /// <summary>
        /// Archive a file
        /// </summary>
        /// <param name="compressionLevel">File compression level</param>
        /// <param name="fileMode">File mode. Default is to create a new archive file or add to an existing archive</param>
        /// <returns></returns>
        public bool Archive(CompressionLevel compressionLevel, FileMode fileMode = FileMode.OpenOrCreate)
        {
            try
            {
                // Get Zip FileName
                string strPath = Path.Combine(this.ParentDirectory.FilePath, this.Name + ".zip");

                // Create New Archive File
                // FileStream streamZipFile = new FileStream(strPath, FileMode.CreateNew);

                // Create File Stream With Open Accessor
                using (FileStream streamZipFile = new FileStream(strPath, fileMode))
                {
                    // Create New ZipArchive
                    ZipArchive zipArchive = new ZipArchive(streamZipFile, ZipArchiveMode.Create);

                    // Get Zip Entry
                    using (ZipArchive zipEntry = new ZipArchive(streamZipFile, ZipArchiveMode.Update))
                    {
                        // Create File
                        ZipArchiveEntry readmeEntry = zipEntry.CreateEntry(this.FullName, compressionLevel);

                        // Create new StreamWriter
                        using (StreamWriter writer = new StreamWriter(readmeEntry.Open()))
                        {
                            // Write File Content
                            writer.Write(this.Content.Value);
                        }
                    }

                    // Close The Zip File
                    streamZipFile.Close();
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

        #region Delete

        /// <summary>
        /// Delete this file
        /// </summary>
        /// <returns></returns>
        public sealed override bool Delete()
        {
            try
            {
                // Attempt File Deletion
                System.IO.File.Delete(this.FilePath);

                // Refresh Object
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

        #region Rename

        /// <summary>
        /// Rename this file
        /// </summary>
        /// <param name="strNewFileName">New filename</param>
        /// <returns></returns>
        public sealed override bool Rename(string strNewFileName)
        {
            try
            {
                // Get New File Path
                string strNewPath = Path.Combine(this.ParentDirectory.FilePath, strNewFileName);

                // Copy Existing File With New File Name
                System.IO.File.Copy(this.FilePath, strNewPath);

                // Delete Original File
                System.IO.File.Delete(this.FilePath);

                // Set New File Path
                this.FilePath = strNewFileName;

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

        #region Move

        /// <summary>
        /// Move a file to a different directory
        /// </summary>
        /// <param name="directoryObject">Directory to be copied to</param>
        /// <returns></returns>
        public sealed override bool Move(DirectoryObject directoryObject)
        {
            return this.Move(directoryObject, this.FullName);
        }

        /// <summary>
        /// Move a file to a different directory
        /// </summary>
        /// <param name="directoryObject">Directory to be copied to</param>
        /// <param name="strDestinationFileName">Destination filename</param>
        /// <returns></returns>
        public bool Move(DirectoryObject directoryObject, string strDestinationFileName)
        {
            try
            {
                string strPath = Path.Combine(directoryObject.FilePath, strDestinationFileName);

                // Attempt Move File
                System.IO.File.Move(this.FilePath, strDestinationFileName);

                // RefreshFile Object
                this.FilePath = strPath;
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

        #region Copy

        /// <summary>
        /// Copy a file to a different directory
        /// </summary>
        /// <param name="directoryObject">Directory to be copied to</param>
        /// <param name="boolOverWrite">Whether or not to overwrite any existing file of the same name</param>
        /// <param name="boolDeleteOriginal">Optional: Delete the original file being copied</param>
        /// <returns></returns>
        public bool Copy(DirectoryObject directoryObject, bool boolOverWrite, bool boolDeleteOriginal = false)
        {
            return this.Copy(directoryObject, this.FullName, boolOverWrite, boolDeleteOriginal);
        }

        /// <summary>
        /// Copy a file to a different directory
        /// </summary>
        /// <param name="directoryObject">Directory to be copied to</param>
        /// <param name="strNewFileName">New filename to be copied to</param>
        /// <param name="boolOverWrite">Whether or not to overwrite any existing file of the same name</param>
        /// <param name="boolDeleteOriginal">Optional: Delete the original file being copied</param>
        /// <returns></returns>
        public bool Copy(DirectoryObject directoryObject, string strNewFileName, bool boolOverWrite, bool boolDeleteOriginal = false)
        {
            try
            {
                string strNewPath = Path.Combine(directoryObject.FilePath, strNewFileName);

                // Copy File              
                System.IO.File.Copy(this.FilePath, strNewPath, boolOverWrite);

                // Check Delete Original
                if (boolDeleteOriginal == true)
                {
                    System.IO.File.Delete(this.FilePath);
                }

                // Refresh Path
                this.FilePath = strNewPath;
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
        /// Create a new file
        /// </summary>
        /// <param name="strFileContent"></param>
        /// <returns></returns>
        public bool Create(string strFileContent)
        {
            var createResult = this.Write(strFileContent);

            // Refresh File Object
            this.Refresh();

            return createResult;
        }

        #endregion

        #region Write

        /// <summary>
        /// Write to a file.
        /// </summary>
        /// <param name="strFileContent">File content to be written</param>
        /// <returns></returns>
        public bool Write(string strFileContent)
        {
            try
            {
                // OverWrite File
                System.IO.File.WriteAllText(this.FilePath, strFileContent);

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

        #region Append

        /// <summary>
        /// Append Value To File. Either At The Beginning Or End Of The File
        /// </summary>
        /// <param name="strFileContent">File content to be appended</param>
        /// <param name="appendLocation">The location to append content to</param>
        /// <returns></returns>
        public bool Append(string strFileContent, FileInformation.FileAppendLocationType appendLocation)
        {
            try
            {
                // Determine Append Location
                switch (appendLocation)
                {
                    case FileInformation.FileAppendLocationType.Top:
                        this.Content.Value = strFileContent + this.Content.Value;
                        break;
                    case FileInformation.FileAppendLocationType.Bottom:
                        this.Content.Value += strFileContent;
                        break;
                    case FileInformation.FileAppendLocationType.Unknown:
                        return false;
                }

                // Append File
                System.IO.StreamWriter writer = System.IO.File.AppendText(this.FilePath);
                writer.Write(strFileContent);
                writer.Flush();
                writer.Close();

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

        #region Show / Hide

        /// <summary>
        /// Set a file to be visible in the file system
        /// </summary>
        /// <returns></returns>
        public bool SetVisible()
        {
            try
            {
                // Get File Attributes
                System.IO.FileAttributes attributes = System.IO.File.GetAttributes(this.FilePath);
                attributes = attributes & ~FileAttributes.Hidden;

                // Set File Attributes
                this.SetAttributes(attributes);

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
        /// Set a file to be hidden in the file system
        /// </summary>
        /// <returns></returns>
        public bool SetHidden()
        {
            try
            {
                // Get File Attributes
                System.IO.FileAttributes attributes = System.IO.File.GetAttributes(this.FilePath);
                attributes = attributes | FileAttributes.Hidden;

                // Set File Attributes
                this.SetAttributes(attributes);

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
        /// Set an attribute on a file
        /// </summary>
        /// <param name="fileAttributes"></param>
        /// <returns></returns>
        [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
        private bool SetAttributes(System.IO.FileAttributes fileAttributes)
        {
            try
            {
                // Set File Attributes
                System.IO.File.SetAttributes(this.FilePath, fileAttributes);

                // Refresh File Object
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

        #region Change Extension

        /// <summary>
        /// Change a file extension to a new extension
        /// </summary>        
        /// <param name="strExtension"></param>
        /// <returns></returns>
        public bool ChangeExtension(string strExtension)
        {
            try
            {
                strExtension = (strExtension.Contains(".") == false) ? "." + strExtension : strExtension;

                // Change Path
                Path.ChangeExtension(this.FilePath, strExtension);

                // Set New File Path
                this.FilePath = Path.Combine(this.ParentDirectory.FilePath, this.Name, strExtension);

                // Refresh File Object
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

        public DataTable AsDataTable()
            => ImportHelper.FileToDataTable(this.Content.MemoryStream, this.FilePath);
        public List<T> ToList<T>() where T : class, new()
            => ImportHelper.FileToDataTable(this.Content.MemoryStream, this.FilePath).ToListCaseInsensitive<T>();

        #endregion
    }
}
