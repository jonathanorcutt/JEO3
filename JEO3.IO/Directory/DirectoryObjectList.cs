using JEO3.IO.File;

namespace JEO3.IO.Directory
{
    /// <summary>
    /// This class represents a list of DirectoryObjects
    /// </summary>
    public sealed class DirectoryObjectList : List<DirectoryObject>
    {
        #region Properties

        #endregion

        #region Initialization

        /// <summary>
        /// Default constructor
        /// </summary>        
        public DirectoryObjectList()
        {
        }

        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="listFileDirectories"></param>
        public DirectoryObjectList(List<string> listFileDirectories)
        {
            // Validation
            if (listFileDirectories == null || listFileDirectories.Count == 0) { return; }

            // Loop Directories
            foreach (string fileDirectory in listFileDirectories)
            {
                // Create a new directory object
                DirectoryObject directoryObject = new DirectoryObject(fileDirectory);

                // Add directory object to the list
                this.Add(directoryObject);
            }
        }

        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="listFileDirectories"></param>
        public DirectoryObjectList(List<DirectoryObject> listFileDirectories)
        {
            // Validation
            if (listFileDirectories == null || listFileDirectories.Count == 0) { return; }

            // Add Directory Files
            this.AddRange(listFileDirectories.ToArray());
        }

        #endregion

        #region Get Files

        /// <summary>
        /// All files from every directory in the list - top-level only, non-recursive. Note: this method can be very expensive in certain large directory structures as it is fully recursive.
        /// </summary>
        public FileObjectList AllFiles
        {
            get
            {
                // Validation
                if (field == null)
                {
                    // Get Files From All Directories - Top Level Only
                    List<FileObject> listFiles = this.SelectMany(directory => directory.GetFiles(false)).ToList();

                    // Create New File Object List
                    field = [.. listFiles];
                }

                return field;
            }
        }

        #endregion
    }
}
