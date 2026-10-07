using Microsoft.WindowsAPICodePack.Shell;
using Microsoft.WindowsAPICodePack.Shell.PropertySystem;

namespace JEO3.IO.File
{
    /// <summary>
    /// This class provides extended system file properties
    /// </summary>
    public sealed class ExtendedProperties
    {

        #region Properties

        /// <summary>
        /// The file object these properties are attributed to
        /// </summary>
        /// <returns></returns>
        private FileObject m_File;

        /// <summary>
        /// Application name
        /// </summary>
        public string ApplicationName { get; private set; } = "";

        /// <summary>
        /// File author
        /// </summary>
        public string Author { get; private set; } = "";

        /// <summary>
        /// Copmuter name
        /// </summary>
        public string ComputerName { get; private set; } = "";

        /// <summary>
        /// Copyright
        /// </summary>
        public string CopyRight { get; private set; } = "";

        /// <summary>
        /// File comments
        /// </summary>
        public string Comments { get; private set; } = "";

        /// <summary>
        /// Company
        /// </summary>
        public string Company { get; private set; } = "";

        /// <summary>
        /// File owner
        /// </summary>
        public string FileOwner { get; private set; } = "";

        /// <summary>
        /// File version
        /// </summary>
        public string FileVersion { get; private set; } = "";

        /// <summary>
        /// File description
        /// </summary>
        public string FileDescription { get; private set; } = "";

        #endregion

        #region Initialization

        internal ExtendedProperties(FileObject fileObject)
        {
            this.m_File = fileObject;

            //this.Load();
        }

        #endregion

        #region Functions

        /// <summary>
        /// Load extended file properties. Calling this method is computationally expensive therefore properties are not loaded by default
        /// </summary>
        public void Load()
        {
            try
            {
                // Get File Attribute
                ShellObject objFile = ShellObject.FromParsingName(this.m_File.FilePath);

                // Get Attributes
                ApplicationName = this.GetAttribute(objFile, SystemProperties.System.ApplicationName);
                Author = this.GetAttribute(objFile, SystemProperties.System.Author);
                Comments = this.GetAttribute(objFile, SystemProperties.System.Comment);
                Company = this.GetAttribute(objFile, SystemProperties.System.Company);
                ComputerName = this.GetAttribute(objFile, SystemProperties.System.ComputerName);
                CopyRight = this.GetAttribute(objFile, SystemProperties.System.Copyright);
                FileDescription = this.GetAttribute(objFile, SystemProperties.System.FileDescription);
                FileOwner = this.GetAttribute(objFile, SystemProperties.System.FileOwner);
                FileVersion = this.GetAttribute(objFile, SystemProperties.System.FileVersion);
            }
            catch (Exception ex)
            {
                // TODO: Error Handling Here ???
            }
        }

        /// <summary>
        /// Retrieve an attribute property on a file
        /// </summary>
        /// <returns></returns>        
        [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
        private string GetAttribute(ShellObject objFile, PropertyKey key)
        {
            try
            {
                // Get File Property
                IShellProperty attribute = objFile.Properties.GetProperty(key);

                // Validation
                return attribute == null || attribute.ValueAsObject == null ? "" : attribute.ValueAsObject.ToString();
            }
            catch (Exception ex)
            {
                // To Be Implemented: Throw Custom Exception...
                Console.WriteLine(ex.ToString());
                return "";
            }
        }


        #endregion
    }
}
