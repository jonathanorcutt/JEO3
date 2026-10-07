using System.Data;
using System.Diagnostics;
using Microsoft.Data.SqlClient;

namespace JEO3.Logging
{
    internal sealed class CodeException
    {
        #region Properties

        // Instances
        internal Exception Exception { get; set; }
        internal string InnerException { get; set; }

        // Properties
        internal long Id { get; set; }
        internal DateTime TimeStamp { get; set; }
        internal string UserName { get; set; }
        internal string Type { get; set; }
        internal string Class { get; set; }
        internal string Method { get; set; }
        internal string LineNumber { get; set; }
        internal string Message { get; set; }
        internal string AttemptedMethod { get; set; }
        internal string AttemptedMethodReturnType { get; set; }
        internal string AttemptedMethodSignature { get; set; }
        internal string StackTrace { get; set; }
        internal string ClassFullName { get; set; }
        internal string Namespace { get; set; }
        internal string AssemblyFilePath { get; set; }
        internal string AssemblyVersion { get; set; }

        #endregion

        #region Initialization

        internal CodeException(Exception ex)
        {
        }

        #endregion

        #region Functions
        internal static void Insert(CodeException exception, string connectionString)
        {
            try
            {
                if (string.IsNullOrEmpty(connectionString)) { return; }

                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    // Point directly to the stored procedure name
                    string procedureName = "[Configuration].[sp_InsertExceptionLog]";

                    using (SqlCommand cmd = new SqlCommand(procedureName, connection))
                    {
                        AddParameters(cmd, exception);
                        cmd.Connection.Open();
                        cmd.ExecuteNonQuery();
                        cmd.Connection.Close();
                        cmd.Connection.Dispose();
                    }
                }
            }
            catch (Exception e)
            {
                Trace.WriteLine(Constants.GenericExceptionPrefix + e);
            }
        }

        internal static async Task InsertAsync(CodeException exception, string connectionString)
        {
            try
            {
                if (string.IsNullOrEmpty(connectionString)) { return; }
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    // Point directly to the stored procedure name
                    string procedureName = "[Configuration].[sp_InsertExceptionLog]";

                    using (SqlCommand cmd = new SqlCommand(procedureName, connection))
                    {
                        // Crucial step: change from Text to StoredProcedure
                        cmd.CommandType = CommandType.StoredProcedure;
                        AddParameters(cmd, exception);
                        await cmd.Connection.OpenAsync();
                        await cmd.ExecuteNonQueryAsync();
                        await cmd.Connection.CloseAsync();
                        await cmd.Connection.DisposeAsync();
                    }
                }
            }
            catch (Exception e)
            {
                Trace.WriteLine(Constants.GenericExceptionPrefix + e);
            }
        }

        private static void AddParameters(SqlCommand cmd, CodeException exception)
        {
            // Crucial step: change from Text to StoredProcedure
            cmd.CommandType = CommandType.StoredProcedure;

            cmd.Parameters.AddWithValue("@TimeStamp", exception.TimeStamp);
            cmd.Parameters.AddWithValue("@UserName", exception.UserName?.Trim() ?? string.Empty);
            cmd.Parameters.AddWithValue("@Type", exception.Type?.Trim() ?? string.Empty);
            cmd.Parameters.AddWithValue("@Namespace", exception.Namespace?.Trim() ?? string.Empty);
            cmd.Parameters.AddWithValue("@Class", exception.Class?.Trim() ?? string.Empty);
            cmd.Parameters.AddWithValue("@Method", exception.Method?.Trim() ?? string.Empty);
            cmd.Parameters.AddWithValue("@LineNumber", exception.LineNumber?.Trim() ?? string.Empty);
            cmd.Parameters.AddWithValue("@Message", exception.Message?.Trim() ?? string.Empty);
            cmd.Parameters.AddWithValue("@AttemptedMethod", exception.AttemptedMethod?.Trim() ?? string.Empty);
            cmd.Parameters.AddWithValue("@AttemptedMethodReturnType", exception.AttemptedMethodReturnType?.Trim() ?? string.Empty);
            cmd.Parameters.AddWithValue("@AttemptedMethodSignature", exception.AttemptedMethodSignature?.Trim() ?? string.Empty);
            cmd.Parameters.AddWithValue("@StackTrace", exception.StackTrace?.Trim() ?? string.Empty);
            cmd.Parameters.AddWithValue("@ClassFullName", exception.ClassFullName?.Trim() ?? string.Empty);
            cmd.Parameters.AddWithValue("@AssemblyFilePath", exception.AssemblyFilePath?.Trim() ?? string.Empty);
            cmd.Parameters.AddWithValue("@AssemblyVersion", exception.AssemblyVersion?.Trim() ?? string.Empty);
        }

        #endregion
    }
}
