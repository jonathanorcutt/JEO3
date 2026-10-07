using JEO3.IO.File;

namespace JEO3.IO.Directory
{
    /// <summary>
    /// Directory object list extension methods
    /// </summary>
    public static class DirectoryObjectListExtensions
    {
        #region Get Files

        /// <summary>
        /// Get file in directory by name
        /// </summary>
        /// <param name="directoryObjectList">Directory list to retrieve files from. Search file name is not case sensitive.</param>
        /// <param name="name"></param>
        /// <returns></returns>
        public static FileObject GetFile(this List<DirectoryObject> directoryObjectList, string name)
        {
            return directoryObjectList
                .Where(directory => directory.Files != null)
                .SelectMany(directory => directory.Files
                    .Where(file => file.Name.ToLower() == name.ToLower()
                        || file.FullName.ToLower() == name.ToLower())).FirstOrDefault();
        }

        /// <summary>
        /// Retrieves files from a file directory
        /// </summary>
        /// <param name="directoryObjectList">Directory list to retrieve files from</param>
        /// <param name="recurse">Flag that determines whether files are retrieved recursively including files from sub-directories</param>
        /// <returns></returns>
        public static FileObjectList GetFiles(this List<DirectoryObject> directoryObjectList, bool recurse)
        {
            return directoryObjectList.GetFiles("*", recurse);
        }

        /// <summary>
        /// Retrieves files from a file directory
        /// </summary>
        /// <param name="directoryObjectList">Directory list to retrieve files from</param>
        /// <param name="find">Search string to find</param>
        /// <param name="recurse">Flag that determines whether files are retrieved recursively including files from sub-directories</param>
        /// <returns></returns>
        public static FileObjectList GetFiles(this List<DirectoryObject> directoryObjectList, string find, bool recurse)
        {
            return [.. directoryObjectList
                .SelectMany(directory => directory.GetFiles(find, recurse)).ToList()];
        }

        #endregion

        #region Get Directory

        /// <summary>
        /// Get sub directory in directory by name
        /// </summary>
        /// <param name="directoryObjectList">Directory list to retrieve files from</param>
        /// <param name="folderName"></param>
        /// <returns></returns>
        public static DirectoryObject GetDirectory(this List<DirectoryObject> directoryObjectList, string folderName)
        {
            return directoryObjectList
                .SelectMany(directory => directory.SubDirectories).Where(directory => directory.Name == folderName).FirstOrDefault();
        }

        #endregion
    }
}
