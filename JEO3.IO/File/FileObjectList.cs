using JEO3.IO.Directory;

namespace JEO3.IO.File
{
    /// <summary>
    /// This class represents a list of FileObjects
    /// </summary>
    public sealed class FileObjectList : List<FileObject>
    {
        #region Initialization

        /// <summary>
        /// Default constructor
        /// </summary>
        public FileObjectList()
        {

        }

        /// <summary>
        /// Constructor instantiating a file list with a list of filenames
        /// </summary>
        /// <param name="listFileNames">List with a list of filenames to create FileObjects for</param>
        public FileObjectList(string path, bool recurse)
        {
            // Validation
            if (path == null) { return; }
            this.AddRange(new DirectoryObject(path).Files);
        }

        /// <summary>
        /// Constructor instantiating a file list with a list of filenames
        /// </summary>
        /// <param name="listFileNames">List with a list of filenames to create FileObjects for</param>
        public FileObjectList(List<string> listFileNames)
        {
            // Validation
            if (listFileNames == null || listFileNames.Count == 0) { return; }

            // Loop Filenames
            foreach (string fileName in listFileNames)
            {
                // Create New FileObject
                FileObject fileObject = new FileObject(fileName);

                // Add FileObject to List
                this.Add(fileObject);
            }
        }

        /// <summary>
        /// Construct new file list from existing files
        /// </summary>
        /// <param name="fileObjectList"></param>
        public FileObjectList(List<FileObject> fileObjectList)
        {
            // Validation
            if (fileObjectList == null || fileObjectList.Count == 0) { return; }

            this.AddRange(fileObjectList.ToArray());
        }

        #endregion

        #region Functions

        /// <summary>
        /// Retrieves a list of existing files from this collection
        /// </summary>
        /// <returns></returns>
        public FileObjectList ValidFiles()
        {
            // Get Existing FileNames
            List<string> listValidFileNames = this
                .Where(file => file.Exists == true)
                .Select(file => file.FilePath)
                .ToList();

            // Create New FileObjectList
            FileObjectList listValidFiles = new FileObjectList(listValidFileNames);

            return listValidFiles;
        }

        /// <summary>
        /// Retrieves a list of non-existing files from this collection
        /// </summary>
        /// <returns></returns>
        public FileObjectList InvalidFiles()
        {
            // Get Existing FileNames
            List<string> listValidFileNames = this
                .Where(file => file.Exists == false)
                .Select(file => file.FilePath)
                .ToList();

            // Create New FileObjectList
            FileObjectList listValidFiles = new FileObjectList(listValidFileNames);

            return listValidFiles;
        }

        #endregion
    }
}
