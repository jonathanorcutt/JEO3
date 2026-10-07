using System.Diagnostics;
using System.Security;

namespace JEO3.IO.Processes
{
    /// <summary>
    /// This class contains information for executing a file process
    /// </summary>
    public sealed class ProcessInformation
    {
        #region Properties

        /// <summary>
        /// Process filename
        /// </summary>
        public string FileName = "";

        /// <summary>
        /// Arguments to pass to the application
        /// </summary>
        public string Arguments = "";

        /// <summary>
        /// Username to run the process under
        /// </summary>
        public string Username = "";

        /// <summary>
        /// User password
        /// </summary>
        public SecureString Password = null;

        /// <summary>
        /// List of environment variables
        /// </summary>
        public List<KeyValuePair<string, string>> EnvironmentVariables = [];

        /// <summary>
        /// The style of the console window when executing
        /// </summary>
        public System.Diagnostics.ProcessWindowStyle WindowStyle = ProcessWindowStyle.Normal;

        /// <summary>
        /// Flag to use shell execute
        /// </summary>
        public bool UseShellExecute
        {
            get => this.EnvironmentVariables != null && this.EnvironmentVariables.Count > 0 ? false : (field);
            set;
        } = false;

        #endregion

        #region Initialization

        /// <summary>
        /// Default constructor
        /// </summary>
        public ProcessInformation()
        {

        }

        /// <summary>
        /// Default constructor 
        /// </summary>
        /// <param name="fileName"></param>
        public ProcessInformation(string fileName)
        {
            this.FileName = fileName;
        }

        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="listEnvironmentVariables"></param>
        public ProcessInformation(string fileName, System.Collections.Specialized.StringDictionary listEnvironmentVariables)
        {
            this.FileName = fileName;
            //this.EnvironmentVariables = listEnvironmentVariables;
        }

        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="strArguments"></param>
        public ProcessInformation(string fileName, string strArguments)
        {
            this.FileName = fileName;
            this.Arguments = strArguments;
        }

        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="strArguments"></param>
        /// <param name="listEnvironmentVariables"></param>
        public ProcessInformation(string fileName, string strArguments, System.Collections.Specialized.StringDictionary listEnvironmentVariables)
        {
            this.FileName = fileName;
            this.Arguments = strArguments;
            //this.EnvironmentVariables = listEnvironmentVariables;
        }

        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="strUsername"></param>
        /// <param name="strPassword"></param>
        public ProcessInformation(string fileName, string strUsername, SecureString strPassword)
        {
            this.FileName = fileName;
            this.Username = strUsername;
            this.Password = strPassword;
        }

        #endregion
    }
}
