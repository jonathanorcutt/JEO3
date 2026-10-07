using System.Security.AccessControl;
using System.Security.Cryptography;
using System.Text;
using Microsoft.WindowsAPICodePack.Shell;
using Microsoft.WindowsAPICodePack.Shell.PropertySystem;

namespace JEO3.IO.File
{
    /// <summary>
    /// File Object Extension Methods
    /// </summary>
    public static class FileObjectExtensions
    {
        #region Internal Methods

        #region Check

        /// <summary>
        /// Check to see if a file is locked
        /// </summary>
        /// <param name="fileObject">File to check is locked</param>        
        /// <param name="boolIncludeCurrentProcess"></param>
        /// <returns></returns>
        [Obsolete("To Be Reviewed For Implementation")]
        [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
        private static bool CheckIsLocked(this FileObject fileObject, bool boolIncludeCurrentProcess = false)
        {
            return false;
            //return FileFunctions.CheckIsFileLocked(fileObject.FilePath, ref strError, boolIncludeCurrentProcess);
        }

        /// <summary>
        /// Check to see if a file can be opened
        /// </summary>
        /// <param name="fileObject">Filename to check if it can be opened</param>
        /// <returns></returns>
        private static bool CheckCanOpen(this FileObject fileObject)
        {
            FileStream stream = null;

            try
            {
                // Create File Info
                FileInfo fileInfo = new FileInfo(fileObject.FilePath);

                // Open File
                stream = fileInfo.Open(FileMode.Open, FileAccess.Read, FileShare.None);
            }
            catch (IOException)
            {
                return true;
            }
            finally
            {
                // If File Opend, Close Stream
                if (stream != null)
                    stream.Close();
            }

            return false;
        }

        #endregion

        #region Attributes

        /// <summary>
        /// Retrieve an attribute property on a file
        /// </summary>
        /// <param name="fileObject">File to retrieve information from</param>
        /// <param name="propertyKey"></param>
        /// <returns></returns>
        [Obsolete("To Be Reviewed For Implementation")]
        [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
        private static IShellProperty GetAttribute(this FileObject fileObject, PropertyKey propertyKey, ref string strError)
        {
            try
            {
                // Get File Attribute
                ShellObject objFile = ShellObject.FromParsingName(fileObject.FilePath);

                // Get File Property
                IShellProperty attribute = objFile.Properties.GetProperty(propertyKey);

                return attribute;
            }
            catch (Exception ex)
            {
                // To Be Implemented: Throw Custom Exception...
                Console.WriteLine(ex.ToString());
                return null;
            }
        }

        /// <summary>
        /// Retrieve an attribute property value on a file
        /// </summary>
        /// <param name="fileObject">File to retrieve information from</param>
        /// <param name="propertyKey"></param>
        /// <param name="propertyFormat"></param>
        /// <returns></returns>
        [Obsolete("To Be Reviewed For Implementation")]
        [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
        private static string GetAttributeValue(this FileObject fileObject, PropertyKey propertyKey, PropertyDescriptionFormatOptions propertyFormat, ref string strError)
        {
            return "";
            //return FileFunctions.GetFileAttributeValue(fileObject.FilePath, propertyKey, propertyFormat, ref strError);
        }

        /// <summary>
        /// Set an attribute on a file
        /// </summary>
        /// <param name="fileObject">File to set information for</param>
        /// <param name="fileAttributes"></param>
        /// <returns></returns>
        [Obsolete("To Be Reviewed For Implementation")]
        [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
        private static bool SetAttributes(this FileObject fileObject, System.IO.FileAttributes fileAttributes)
        {
            try
            {
                // Set File Attributes
                System.IO.File.SetAttributes(fileObject.FilePath, fileAttributes);

                // Refresh File Object
                fileObject.Refresh();

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

        #region Permissions

        /// <summary>
        /// Set permissions on a file directory
        /// </summary>
        /// <param name="fileObject">File to set permissions for</param>
        /// <param name="fileRights">The type of file rights to set access for</param>
        /// <param name="accessControlType">The type of access to </param>
        /// <param name="strSpecificUserName">Optional: Define a specific username to set permissions for</param>
        /// <returns></returns>
        private static bool SetPermission(this FileObject fileObject, FileSystemRights fileRights, AccessControlType accessControlType, string strSpecificUserName = "")
        {
            try
            {
                // Get User Name
                string strFullyQualifiedUserName = (strSpecificUserName == "") ? Environment.UserDomainName + "\\" + Environment.UserName : strSpecificUserName;

                // Create FileInfo
                FileInfo fileInfo = new FileInfo(fileObject.FilePath);

                // Get File Security Settings
                FileSecurity fileSecurity = fileInfo.GetAccessControl();

                // Create Access Rule
                FileSystemAccessRule fileAccessRule = new FileSystemAccessRule(strFullyQualifiedUserName, fileRights, accessControlType);

                // Add the FileSystemAccessRule to the Security Settings.
                fileSecurity.AddAccessRule(fileAccessRule);

                // Set Access Control
                fileInfo.SetAccessControl(fileSecurity);

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

        #region Encrypt / Decrypt

        /// <summary>
        /// Encrypt a file
        /// </summary>
        /// <param name="fileObject">File to encrypt</param>
        /// <returns></returns>
        [Obsolete("To Be Reviewed For Implementation")]
        [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
        private static bool Encrypt(this FileObject fileObject)
        {
            try
            {
                System.IO.File.Encrypt(fileObject.FilePath);

                // Refresh File Object
                fileObject.Refresh();

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
        /// Encrypt a file
        /// </summary>
        /// <param name="fileObject">File to encrypt</param>
        /// <param name="strEncryptedFilename">Encrypted filename</param>
        /// <param name="strKey">Key to use to encrypt the file</param>
        /// <returns></returns>
        [Obsolete("To Be Reviewed For Implementation")]
        [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
        private static bool Encrypt(this FileObject fileObject, string strEncryptedFilename, string strKey)
        {
            try
            {
                // Read File
                FileStream stream = new FileStream(fileObject.FilePath, FileMode.Open, FileAccess.Read);
                FileStream streamEncrypted = new FileStream(strEncryptedFilename, FileMode.Create, FileAccess.Write);
                DESCryptoServiceProvider DES = new DESCryptoServiceProvider
                {
                    Key = ASCIIEncoding.ASCII.GetBytes(strKey),
                    IV = ASCIIEncoding.ASCII.GetBytes(strKey)
                };

                // Create Encryptor
                ICryptoTransform desencrypt = DES.CreateEncryptor();
                CryptoStream cryptostream = new CryptoStream(streamEncrypted, desencrypt, CryptoStreamMode.Write);

                byte[] bytearrayinput = new byte[stream.Length];
                stream.Read(bytearrayinput, 0, bytearrayinput.Length);

                // Write to File
                cryptostream.Write(bytearrayinput, 0, bytearrayinput.Length);
                cryptostream.Close();
                stream.Close();

                // Refresh File Object
                fileObject.Refresh();

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
        /// Decrypt a file
        /// </summary>
        /// <param name="fileObject">Encrypted filename</param>
        /// <returns></returns>
        [Obsolete("To Be Reviewed For Implementation")]
        [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
        private static bool Decrypt(this FileObject fileObject)
        {
            try
            {
                // Decrypt File
                System.IO.File.Decrypt(fileObject.FilePath);

                // Refresh File Object
                fileObject.Refresh();

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
        /// Decrypt a file
        /// </summary>
        /// <param name="fileObject">Encrypted filename</param>
        /// <param name="strDecryptedFilename">Decrypted filename</param>
        /// <param name="strKey">Key to use to decrypt the file</param>
        /// <returns></returns>
        [Obsolete("To Be Reviewed For Implementation")]
        [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
        private static bool Decrypt(this FileObject fileObject, string strDecryptedFilename, string strKey)
        {
            try
            {
                // Create DES Provider
                DESCryptoServiceProvider DES = new DESCryptoServiceProvider
                {
                    Key = ASCIIEncoding.ASCII.GetBytes(strKey),
                    IV = ASCIIEncoding.ASCII.GetBytes(strKey)
                };

                // Read Encrypted File
                FileStream stream = new FileStream(fileObject.FilePath, FileMode.Open, FileAccess.Read);
                ICryptoTransform desdecrypt = DES.CreateDecryptor();
                CryptoStream cryptostreamDecr = new CryptoStream(stream, desdecrypt, CryptoStreamMode.Read);

                // Write Decrypted File
                StreamWriter fsDecrypted = new StreamWriter(strDecryptedFilename);
                fsDecrypted.Write(new StreamReader(cryptostreamDecr).ReadToEnd());
                fsDecrypted.Flush();
                fsDecrypted.Close();

                // Refresh File Object
                fileObject.Refresh();

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

        #region Path Information

        /// <summary>
        /// Retrieve the file extension of a file
        /// </summary>
        /// <param name="fileObject">Filename to retrieve extension from</param>
        /// <returns></returns>
        internal static string GetExtension(this FileObject fileObject)
        {
            try
            {
                // Get File Extenstion
                string strExtension = System.IO.Path.GetExtension(fileObject.FilePath);

                return strExtension;
            }
            catch (Exception ex)
            {
                // To Be Implemented: Throw Custom Exception...
                Console.WriteLine(ex.ToString());
                return "";
            }
        }

        /// <summary>
        /// Retrieve a filename with file extension
        /// </summary>
        /// <param name="fileObject">Filename to retrieve name with extension from</param>
        /// <returns></returns>
        internal static string GetNameWithExtenstion(this FileObject fileObject)
        {
            try
            {
                // Get File Name Without Extension
                string fileName = System.IO.Path.GetFileName(fileObject.FilePath);

                return fileName;
            }
            catch (Exception ex)
            {
                // To Be Implemented: Throw Custom Exception...
                Console.WriteLine(ex.ToString());
                return "";
            }
        }

        /// <summary>
        /// Retrieve a filename without file extension
        /// </summary>
        /// <param name="fileObject">Filename to retrieve name without extension from</param>
        /// <returns></returns>
        internal static string GetNameWithOutExtenstion(this FileObject fileObject)
        {
            try
            {
                // Get File Name Without Extension
                string fileName = System.IO.Path.GetFileNameWithoutExtension(fileObject.FilePath);

                return fileName;
            }
            catch (Exception ex)
            {
                // To Be Implemented: Throw Custom Exception...
                Console.WriteLine(ex.ToString());
                return "";
            }
        }

        #endregion

        #endregion
    }
}
