using System.Text;

namespace JEO3.IO.File
{
    /// <summary>
    /// This class contains file content information
    /// </summary>
    public sealed class FileContent : IFileContent
    {
        #region Properties

        #region File

        /// <summary>
        /// The file object this content resides in
        /// </summary>
        /// <returns></returns>
        [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
        public FileObject File { get; }

        #endregion

        #region Content

        private string m_Value = "";
        /// <summary>
        /// Value of the file content
        /// </summary>
        /// <returns></returns>
        public string Value
        {
            get => this.m_Value;
            set
            {
                // Validation
                if (value == null)
                {
                    this.m_Value = "";
                    this.m_Lines = [];
                    this.m_MemoryStream = new MemoryStream();
                    return;
                }

                // Set Value
                this.m_Value = value;

                // Set Lines
                this.m_Lines = this.m_Value.Split('\r').ToList();

                // Set Memory Stream
                this.m_MemoryStream = new MemoryStream(Encoding.UTF8.GetBytes(this.m_Value ?? ""));
            }
        }

        private List<string> m_Lines = [];
        /// <summary>
        /// The list of lines that make up the file content
        /// </summary>
        /// <returns></returns>
        public List<string> Lines
        {
            get => this.m_Lines;
            set
            {
                // Validation
                if (value == null)
                {
                    this.m_Value = "";
                    this.m_Lines = [];
                    this.m_MemoryStream = new MemoryStream();
                    return;
                }

                // Get Memory Stream
                this.m_Lines = value;

                // Set Value
                this.m_Value = String.Join("\r", this.m_Lines.ToArray());

                // Set Memory Stream
                this.m_MemoryStream = new MemoryStream(Encoding.UTF8.GetBytes(this.m_Value ?? ""));
            }
        }

        private MemoryStream m_MemoryStream;
        /// <summary>
        /// The memory stream representing the value of the file content
        /// </summary>
        /// <returns></returns>
        public MemoryStream MemoryStream
        {
            get => this.m_MemoryStream;
            set
            {
                // Validation
                if (value == null)
                {
                    this.m_Value = "";
                    this.m_Lines = [];
                    this.m_MemoryStream = new MemoryStream();
                    return;
                }

                // Set Memory Stream
                this.m_MemoryStream = value;

                // Get Value 
                this.m_Value = new StreamReader(this.m_MemoryStream).ReadToEnd();

                // Get Lines
                this.m_Lines = this.m_Value.Split('\r').ToList();
            }
        }

        #endregion

        #endregion

        #region Initialization

        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="fileObject"></param>
        public FileContent(FileObject fileObject)
        {
            // Validation
            if (fileObject == null) { return; }

            // Set File Object
            File = fileObject;
        }

        #endregion

        #region Functions

        /// <summary>
        /// Load the file's content
        /// </summary>
        /// <returns></returns>
        public bool Load()
        {
            // Validation
            if (this.File == null || this.File.Exists == false)
            {
                return false;
            }

            // Get File Content
            this.m_Value = this.GetContentAsString();

            // Get File Lines
            this.m_Lines = this.GetContentAsLineList();

            // Get Memory Stream
            this.m_MemoryStream = this.GetContentAsMemoryStream();

            // Validation
            return this.m_Value != null;
        }

        /// <summary>
        /// Save the file's content
        /// </summary>
        /// <returns></returns>
        public bool Save()
        {
            try
            {
                // Validation
                if (this.File == null || this.File.Exists == false || this.File.Permissions.CanRead == false)
                {
                    return false;
                }

                // Save File Content
                var saveResult = this.File.Write(this.Value);

                // Validation
                return saveResult != false;
            }
            catch (Exception ex)
            {
                // To Be Implemented: Throw Custom Exception...
                Console.WriteLine(ex.ToString());
                return false;
            }
        }

        /// <summary>
        /// Refresh the file's content
        /// </summary>
        /// <returns></returns>
        public void Refresh()
        {
            // Reload This File's Content
            this.Load();
        }

        #endregion        
    }
}
