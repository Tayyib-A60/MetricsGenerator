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
    public class UniqueCardProcessor
    {
        public AppSettings appSettings { get; set; }
        private readonly ILogger<UniqueCardProcessor> _logger;

        public UniqueCardProcessor(ILogger<UniqueCardProcessor> logger)
        {
            _logger = logger;
        }
        public async Task<List<UniqueCard>> GetUniqueCards()
        {
            var cards = new List<UniqueCard>();
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
            commandQuery = $"SELECT *  from {appSettings.UniqueCardTable} where MonthYear = @monthYear and Monitoring = 0";

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
                            cards.Add(new UniqueCard
                            {
                                Id = Convert.ToInt64(sqlDataReader["Id"].ToString()),
                                CardNo = sqlDataReader["CardNo"].ToString(),
                                MonthYear = sqlDataReader["MonthYear"].ToString(),
                                TransactionCount = Convert.ToInt32(
                                                    sqlDataReader["TransactionCount"].ToString()),
                            });
                        }
                        return cards;
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
            return cards;
        }
        
        public async Task<bool> UpdateUniqueCards(List<long> cardIds)
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
            commandQuery = $"UPDATE {appSettings.UniqueCardTable} set Monitoring=1 where MonthYear = @monthYear and ";
            commandQuery  += generateRemainingQuery(cardIds);

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

        private string generateRemainingQuery(List<long> cardIds)
        {
            var remainingQuery = string.Empty;
            for(int i = 0; i < cardIds.Count; i++)
            {
                remainingQuery += $" Id={cardIds[i]} ";

                if(cardIds.Count > 1 || i != cardIds.Count -1)
                {
                    remainingQuery += " or ";
                }
            }

            return remainingQuery;
        }
    }
}