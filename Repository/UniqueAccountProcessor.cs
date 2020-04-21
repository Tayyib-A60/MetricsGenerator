using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using zoneswitch.metricsgenerator.Extensions;
using zoneswitch.metricsgenerator.Models.DbData;

namespace zoneswitch.metricsgenerator.Repository
{
    public class UniqueAccountProcessor
    {
        public AppSettings appSettings { get; set; }
        private readonly ILogger<UniqueAccountProcessor> _logger;

        public UniqueAccountProcessor(ILogger<UniqueAccountProcessor> logger)
        {
            _logger = logger;
        }
        public async Task<List<UniqueAccount>> GetUniqueAccounts()
        {
            var accounts = new List<UniqueAccount>();
            appSettings = new AppSettings();
            using (StreamReader r = File.OpenText("appsettings.json"))
            {
                string json = r.ReadToEnd();
                json = json.Replace("\r\n","").Trim();
                appSettings = new AppSettings();
                appSettings = JsonConvert.DeserializeObject<AppSettings>(json);
            }

            var monthYear = $"{DateTime.Now.Month}/{DateTime.Now.Year}";
            var sqlConnection = new SqlConnection(appSettings.SqlServerConnectionString);
            var commandQuery = string.Empty;
            commandQuery = $"SELECT *  from {appSettings.UniqueAccountTable} where MonthYear = @monthYear and Monitoring = 0";

            try
            {
                using (var sqlCommand = new SqlCommand(commandQuery, sqlConnection))
                {

                    sqlCommand.CommandType = System.Data.CommandType.Text;
                    await sqlConnection.OpenAsync();

                    sqlCommand.Parameters.AddWithValue("@monthYear", monthYear);

                    var sqlDataReader = await sqlCommand.ExecuteReaderAsync();
                    if (sqlDataReader.HasRows)
                    {
                        while (sqlDataReader.Read())
                        {
                            accounts.Add(new UniqueAccount
                            {
                                Id = Convert.ToInt64(sqlDataReader["Id"].ToString()),
                                AccountNumber = sqlDataReader["AccountNumber"].ToString(),
                                MonthYear = sqlDataReader["MonthYear"].ToString(),
                                TransactionCount = Convert.ToInt32(
                                                    sqlDataReader["TransactionCount"].ToString())
                            });
                        }
                        _logger.LogDebug($"{JsonConvert.SerializeObject(accounts)}");
                        return accounts;
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogDebug($"{ex.Message}{ex.StackTrace}**");
            }
            finally
            {
                sqlConnection.Close();
            }
            return accounts;
        }
        
        public async Task<bool> UpdateUniqueAccounts(List<long> accountIds)
        {
            appSettings = new AppSettings();
            using (StreamReader r = File.OpenText("appsettings.json"))
            {
                string json = r.ReadToEnd();
                json = json.Replace("\r\n","").Trim();
                appSettings = new AppSettings();
                appSettings = JsonConvert.DeserializeObject<AppSettings>(json);
            }

            var monthYear = $"{DateTime.Now.Month}/{DateTime.Now.Year}";
            var sqlConnection = new SqlConnection(appSettings.SqlServerConnectionString);
            var commandQuery = string.Empty;
            commandQuery = $"UPDATE {appSettings.UniqueAccountTable} set Monitoring=1 where MonthYear = @monthYear and ";
            commandQuery  += generateRemainingQuery(accountIds);

            _logger.LogInformation("About to update processed accounts");
            _logger.LogInformation($"using this query: {commandQuery}");
            try
            {
                using (var sqlCommand = new SqlCommand(commandQuery, sqlConnection))
                {

                    sqlCommand.CommandType = System.Data.CommandType.Text;
                    sqlCommand.Parameters.AddWithValue("@monthYear", monthYear);

                    await sqlConnection.OpenAsync();


                    var rowsAffected = await sqlCommand.ExecuteNonQueryAsync();
                    return rowsAffected > 0 ? true : false;
                }
            }
            catch (Exception ex)
            {
                _logger.LogDebug($"{ex.Message}{ex.StackTrace}**");
            }
            finally
            {
                sqlConnection.Close();
            }
            return false;
        }

        private string generateRemainingQuery(List<long> accountIds)
        {
            var remainingQuery = string.Empty;
            for(int i = 0; i < accountIds.Count; i++)
            {
                remainingQuery += $" Id={accountIds[i]} ";

                if(accountIds.Count > 1 && i != accountIds.Count -1)
                {
                    remainingQuery += " or ";
                }
            }

            return remainingQuery;
        }
    }
}