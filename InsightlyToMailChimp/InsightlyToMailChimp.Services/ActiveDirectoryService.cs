using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using InsightlyToMailChimp.Core;
using InsightlyToMailChimp.Core.Models.ActiveDirectory;
using InsightlyToMailChimp.Core.Services;

namespace InsightlyToMailChimp.Services
{
    public class ActiveDirectoryService : IActiveDirectoryService
    {
        #region Public methods
        public async Task<List<ActiveADUser>> GetAllActiveADUsers()
        {
            var emails = (await CreateCommand<ActiveADUser>(GetConnection(), "dbo.AllActiveADUsers"))
                .ToList();

            LogHelper.Debug($"There are {emails.Count} emails in ActiveDirectory.");

            return emails;
        }
        #endregion

        #region Private methods
        private SqlConnection GetConnection()
        {
            var connectionString = ConfigurationManager.ConnectionStrings["DBConnectionString"].ConnectionString;
            var sqlConnection = new SqlConnection(connectionString);
            sqlConnection.Open();

            return sqlConnection;
        }

        private static async Task<IEnumerable<T>> CreateCommand<T>(SqlConnection connection, string procedureName, object param = null)
        {
            return await connection.QueryAsync<T>(procedureName, param, commandType: CommandType.StoredProcedure);
        }
        #endregion
    }
}