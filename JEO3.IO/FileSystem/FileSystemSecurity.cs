using System.Security.AccessControl;
using System.Security.Principal;

namespace JEO3.IO.Foundation
{
    /// <summary>
    /// This class contains file system security information for a FileSystemObjectBase instance
    /// </summary>    
    public class FileSystemSecurity : IFileSystemSecurity
    {
        #region Properties

        #region File System Object

        /// <summary>
        /// Copy of the file system object for refreshing permissions
        /// </summary>
        private FileSystemObject m_FileSystemObject { get; set; }

        #endregion

        #region Permissions

        /// <summary>
        /// Whether or not the current executing user has read privledges
        /// </summary>
        /// <returns></returns>
        public bool CanRead
        {
            get => CanExecute; protected set => CanExecute = value;
        }

        private bool m_CanWrite;
        /// <summary>
        /// Whether or not the current executing user has write privledges
        /// </summary>
        /// <returns></returns>
        public bool CanWrite
        {
            get => CanExecute; protected set => this.m_CanWrite = value;
        }

        private bool m_CanDelete;
        /// <summary>
        /// Whether or not the current executing user has delete privledges
        /// </summary>
        /// <returns></returns>
        public bool CanDelete => CanExecute;

        private bool m_CanModify;
        /// <summary>
        /// Whether or not the current executing user has modify privledges
        /// </summary>
        /// <returns></returns>
        public bool CanModify => CanExecute;

        private bool m_CanExecute;
        /// <summary>
        /// Whether or not the current executing user has execute privledges
        /// </summary>
        /// <returns></returns>
        public bool CanExecute { get; private set; }

        #endregion

        #endregion

        #region Initialization

        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="fileObject"></param>
        public FileSystemSecurity(FileSystemObject fileObject)
        {
            // Validation
            if (fileObject == null) { return; }

            this.m_FileSystemObject = fileObject;

            // Load Rights
            this.LoadPermissions();
        }

        /// <summary>
        /// Load the permissions for this file system object
        /// </summary>
        private void LoadPermissions()
        {
            try
            {
                // Attempt To Demand Permissions
                //this.CanRead = this.CheckPermission(System.Security.Permissions.FileIOPermissionAccess.Read);
                //this.CanWrite = this.CheckPermission(System.Security.Permissions.FileIOPermissionAccess.Write);

                // Get Current User Identity
                WindowsIdentity userIdentity = WindowsIdentity.GetCurrent();

                // Get Current NT User
                NTAccount userAccount = new NTAccount(userIdentity.Name);

                // Get File Security
                FileSecurity fileSecurity = new FileSecurity(this.m_FileSystemObject.FilePath, AccessControlSections.Access);

                // Get Access Rules
                AuthorizationRuleCollection securityRules = fileSecurity.GetAccessRules(true, true, typeof(NTAccount));

                // Loop Rules
                foreach (FileSystemAccessRule rule in securityRules)
                {
                    // If Denied Access - Continue
                    if (rule.AccessControlType == AccessControlType.Deny) { continue; }

                    // Determine If Rule Account Is System Role And Current User Belongs To That Group
                    bool boolIsInSystemRole = this.IsInSystemRole(rule.IdentityReference.Value, userIdentity) == false;

                    // Detemine If User Or Group Applies To The Current User
                    if (rule.IdentityReference != userAccount && boolIsInSystemRole == false) { continue; }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("An Error Occurred Loading Permission Information For Path '" + this.m_FileSystemObject.FilePath + "'. Detail: " + ex.Message);
            }
        }

        #endregion

        #region Functions

        /// <summary>
        /// Load file access rule permissions
        /// </summary>
        /// <param name="rule"></param>
        private void LoadRulePermissions(FileSystemAccessRule rule)
        {
#if NET40
            // Set Can Read Permission
            this.m_CanRead = (this.m_CanRead == false) ? rule.FileSystemRights.HasFlag(FileSystemRights.Read) : this.m_CanRead;

            // Set Can Write Permission
            this.m_CanWrite = (this.m_CanWrite == false) ? rule.FileSystemRights.HasFlag(FileSystemRights.Write) || rule.FileSystemRights.HasFlag(FileSystemRights.WriteData) : this.m_CanWrite;

            // Set Can Delete Permission
            this.m_CanDelete = (this.m_CanDelete == false) ? rule.FileSystemRights.HasFlag(FileSystemRights.Delete) : this.m_CanDelete;

            // Set Can Modify Permission
            this.m_CanModify = (this.m_CanModify == false) ? rule.FileSystemRights.HasFlag(FileSystemRights.Modify) : this.m_CanModify;

            // Set Can Execute Permission
            this.m_CanExecute = (this.m_CanExecute == false) ? rule.FileSystemRights.HasFlag(FileSystemRights.ExecuteFile) || rule.FileSystemRights.HasFlag(FileSystemRights.ExecuteFile) : this.m_CanExecute;
#endif
        }

        /// <summary>
        /// Checks permissions of a file system object
        /// </summary>
        /// <param name="accessType">Type of directory access to check permissions for</param>       
        /// <returns></returns>
        //private bool CheckPermission(FileIOPermissionAccess accessType)
        //{
        //    try
        //    {
        //        // Check Is Path Root
        //        bool boolIsRoot = System.IO.Path.GetPathRoot(this.m_FileSystemObject.FilePath) == this.m_FileSystemObject.FilePath;

        //        // Validation
        //        if (boolIsRoot == true) { return false; }

        //        // Get Permissions
        //        var permission = new FileIOPermission(accessType, this.m_FileSystemObject.FilePath);

        //        // Demand Permissions
        //        permission.Demand();

        //        return true;
        //    }
        //    catch (Exception ex)
        //    {
        //        // To Be Implemented: Throw Custom Exception...
        //        Console.WriteLine(ex.ToString());
        //        return false;
        //    }
        //}

        /// <summary>
        /// Determine whether or not an account is in a system role
        /// </summary>
        /// <param name="strAccountName">Account name to check</param>
        /// <param name="identity">Windows identity to check against</param>
        /// <returns></returns>
        protected bool IsInSystemRole(string strAccountName, WindowsIdentity identity)
        {
            // Create Windows Principle
            WindowsPrincipal principal = new WindowsPrincipal(identity);

            strAccountName = (strAccountName.Split('\\').Length > 1) ? strAccountName.Split('\\')[1] : strAccountName;
            strAccountName = (strAccountName.Split('/').Length > 1) ? strAccountName.Split('/')[1] : strAccountName;
            strAccountName = strAccountName.Trim();

            WindowsBuiltInRole role = strAccountName == "Administrator" || strAccountName == "Administrators" ? WindowsBuiltInRole.Administrator : WindowsBuiltInRole.Guest;
            role = strAccountName == "User" || strAccountName == "Users" ? WindowsBuiltInRole.User : role;

            // Check User Belongs To Role
            bool boolIsInRole = principal.IsInRole(role);

            return boolIsInRole;
        }

        /// <summary>
        /// Refresh the security priveldges for the file system object
        /// </summary>
        public virtual void Refresh()
        {
            // Load Rights
            this.LoadPermissions();
        }

        #endregion
    }
}

