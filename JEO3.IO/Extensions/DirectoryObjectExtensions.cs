using System.Security.AccessControl;
using JEO3.IO.File;
using JEO3.IO.Information;

namespace JEO3.IO.Directory
{
    /// <summary>
    /// Directory Object Extension Methods
    /// </summary>
    public static class DirectoryObjectExtensions
    {
        #region Internal Methods

        #region Get Files

        #region Synchronous

        /// <summary>
        /// Retrieves files that are locked in a file directory
        /// </summary>
        /// <param name="directoryObject">Directory to retrive locked files for</param>
        /// <param name="recurse">Flag that determines whether files are retrieved recursively including files from sub-directories</param>
        /// <param name="boolIncludeCurrentProcess"></param>
        /// <returns></returns>
        private static FileObjectList GetFilesLocked(this DirectoryObject directoryObject, bool recurse, bool boolIncludeCurrentProcess = false)
        {
            return null;
            //// Get FileNames
            //List<string> listFileNames = DirectoryFunctions.GetDirectoryFilesLocked(directoryObject.FilePath, recurse, ref strError, boolIncludeCurrentProcess);

            //// Create Directory List
            //FileObjectList files = new FileObjectList(listFileNames);

            //return files;           
        }

        #endregion

        #endregion

        #region File Content

        /// <summary>
        /// Retrieves all file content as a string from a directory
        /// </summary>
        /// <param name="directoryObject">Directory to retrieve content from</param>
        /// <param name="recurse">Flag that determines whether files are retrieved recursively including files from sub-directories</param>
        /// <returns></returns>
        private static string GetFileContentAsString(this DirectoryObject directoryObject, bool recurse)
        {
            string strContent = directoryObject.GetFileContentAsString("*", recurse);

            return strContent;
        }

        /// <summary>
        /// Retrieves all file content as a string from a directory
        /// </summary>
        /// <param name="directoryObject">Directory to retrieve content from</param>
        /// <param name="find">Search string to find</param>
        /// <param name="recurse">Flag that determines whether files are retrieved recursively including files from sub-directories</param>
        /// <returns></returns>
        private static string GetFileContentAsString(this DirectoryObject directoryObject, string find, bool recurse)
        {
            // Create File List
            FileObjectList files = directoryObject.GetFiles(find, recurse);

            // Load All File Content
            files.ForEach(file => file.Content.Load());

            string strContent = String.Join("\r\n", files.Select(file => file.Content.Value).ToArray());

            return strContent;
        }

        /// <summary>
        /// Retrieves all file content as a line list from a directory
        /// </summary>
        /// <param name="directoryObject">Directory to retrieve content from</param>
        /// <param name="find">Search string to find</param>
        /// <param name="recurse">Flag that determines whether files are retrieved recursively including files from sub-directories</param>
        /// <returns></returns>
        private static List<string> GetFileContentAsLineList(this DirectoryObject directoryObject, string find, bool recurse)
        {
            string strContent = directoryObject.GetFileContentAsString(find, recurse);
            List<string> listLines = strContent.Split('\n').ToList();

            return listLines;
        }

        /// <summary>
        /// Retrieves all file content as a line list from a directory
        /// </summary>
        /// <param name="directoryObject">Directory to retrieve content from</param>
        /// <param name="recurse">Flag that determines whether files are retrieved recursively including files from sub-directories</param>
        /// <returns></returns>
        private static List<string> GetFileContentAsLineList(this DirectoryObject directoryObject, bool recurse)
        {
            List<string> listLines = directoryObject.GetFileContentAsLineList("*", recurse);

            return listLines;
        }

        #endregion

        #region Property Information

        /// <summary>
        /// Retrieves size information of a directory
        /// </summary>
        /// <param name="directoryObject">Directory to retrieve size information from</param>
        /// <param name="fileSizeType">Type of file size to be retrieved</param>
        /// <param name="recurse">Flag that determines whether files are retrieved recursively including files from sub-directories</param>
        /// <returns></returns>
        [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
        internal static decimal GetSize(this DirectoryObject directoryObject, FileInformation.FileSizeType fileSizeType, bool recurse)
        {
            try
            {
                // Get Directory Files
                FileObjectList listFiles = directoryObject.GetFiles(recurse);

                // Get Total Size
                decimal decDirectorySize = listFiles.Select(file =>
                    (fileSizeType == FileInformation.FileSizeType.Bytes) ? file.Size.Bytes :
                    (fileSizeType == FileInformation.FileSizeType.KiloBytes) ? file.Size.KiloBytes :
                    (fileSizeType == FileInformation.FileSizeType.MegaBytes) ? file.Size.MegaBytes :
                    (fileSizeType == FileInformation.FileSizeType.GigaBytes) ? file.Size.GigaBytes : 0).Sum();

                return decDirectorySize;
            }
            catch (Exception ex)
            {
                // To Be Implemented: Throw Custom Exception...
                Console.WriteLine(ex.ToString());
                return -1;
            }
        }

        #endregion

        #region Permissions

        /// <summary>
        /// Sets permissions on a file directory
        /// </summary>
        /// <param name="directoryObject">Directory to set permissions for</param>
        /// <param name="fileRights">Type of directory file rights to set</param>
        /// <param name="accessControlType">Type of access control to set</param>        
        /// <param name="strSpecificUserName">Optional: Specific username to set permissions for</param>
        /// <param name="boolAllowEveryOne">Optional: Flag to determine whether or not to set permissions for all users</param>
        /// <returns></returns>
        internal static bool SetPermissions(this DirectoryObject directoryObject, FileSystemRights fileRights, AccessControlType accessControlType, string strSpecificUserName = "", bool boolAllowEveryOne = false)
        {
            try
            {
                // Get User Name
                string strFQUserName = (strSpecificUserName == "") ? Environment.UserDomainName + "\\" + Environment.UserName : strSpecificUserName;

                // Check Optional Bool AllowEveryone
                if (boolAllowEveryOne == true)
                {
                    // Create New Security Identifier
                    var sid = new System.Security.Principal.SecurityIdentifier(System.Security.Principal.WellKnownSidType.BuiltinUsersSid, null);

                    // Get Fully Qualified UserName
                    strFQUserName = ((System.Security.Principal.NTAccount)sid.Translate(typeof(System.Security.Principal.NTAccount))).ToString();
                }

                // Create DirectoryInfo
                DirectoryInfo directoryInfo = new DirectoryInfo(directoryObject.FilePath);

                // Get Directory Security Settings
                DirectorySecurity directorySecurity = directoryInfo.GetAccessControl();

                // Create Access Rule
                FileSystemAccessRule directoryAccessRule = new FileSystemAccessRule(strFQUserName, fileRights,
                    InheritanceFlags.ContainerInherit | InheritanceFlags.ObjectInherit,
                    PropagationFlags.None, accessControlType);

                // Add the FileSystemAccessRule to the Security Settings. 
                directorySecurity.AddAccessRule(directoryAccessRule);

                // Set Access Control
                directoryInfo.SetAccessControl(directorySecurity);

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

        #endregion
    }
}
