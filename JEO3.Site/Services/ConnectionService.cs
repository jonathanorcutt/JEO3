using System.Configuration;
using System.Data;
using JEO3.Logging;
using JEO3.Providers;

namespace JEO3.Site.Services
{
    public sealed class ConnectionService
    {
        #region Properties

        public List<ConnectionStringSettings> ConnectionSettings { get; set; }

        #endregion

        #region Initialization

        public ConnectionService(List<ConnectionStringSettings> connSettings)
        {
            ConnectionSettings = connSettings;
        }
        public async Task<DataTable> Execute(string query)
        {
            try
            {
                // Primary Connection String
                if (System.Configuration.ConfigurationManager.ConnectionStrings.Count == 0) throw new Exception("Exception: No COnnection Strings.");
                var connSetting = ConnectionSettings.FirstOrDefault();

                // Primary Connection String
                IDatabaseProvider? activeTarget = connSetting.ToDatabaseProvider();

                // Get DataTable
                var dt = await activeTarget.GetDataTable(query);
                return dt == null ? throw new Exception("Null Generation Result!") : dt;
            }
            catch (Exception ex)
            {
                await ExceptionUtility.LogExceptionAsync(ex);
            }

            return null;
        }

        #endregion
    }
}
