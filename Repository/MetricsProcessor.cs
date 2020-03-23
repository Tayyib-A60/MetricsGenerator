using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using InfluxData.Net.InfluxDb;
using InfluxData.Net.InfluxDb.Models;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using zoneswitch.metricsgenerator.Extensions;
using zoneswitch.metricsgenerator.Models;

namespace zoneswitch.metricsgenerator.Repository
{
    public class MetricsProcessor
    {
        public static AppSettings appsettings { get; set; }
        public static Dictionary<string, FTTransactionDetail> FTTransactionDetails = new Dictionary<string, FTTransactionDetail>();
        public static Dictionary<string, DateTime> NITransactionDetails = new Dictionary<string, DateTime>();
        public static async Task<bool> ProcessFundsTransferInitiatedEvent(string eventData)
        {
            var transactionInitiatedEvent = JsonConvert.DeserializeObject<TransactionInitiatedEvent>(eventData);
            var transactionDetail = new FTTransactionDetail {
                SenderBank = transactionInitiatedEvent.senderBankId,
                TransactionType = transactionInitiatedEvent.transactionType,
                Amount = transactionInitiatedEvent.amount,
                StartTime = Convert.ToDateTime(transactionInitiatedEvent.dateCreated)
            };

            FTTransactionDetails.Add(transactionInitiatedEvent.transactionReference, transactionDetail);

            var pointToWrite = new Point()
            {
                Name = InfluxDataTables.FundsTransferInitiatedTable,
                Tags = new Dictionary<string, object>() 
                {
                    { "status", TransactionStatus.Initiated}
                },
                Fields = new Dictionary<string, object>()
                {
                    { "transactionReference", transactionInitiatedEvent.transactionReference },
                    { "transactionType", transactionInitiatedEvent.transactionType },
                    { "startTime", transactionInitiatedEvent.dateCreated },
                    { "status", TransactionStatus.Initiated}
                },
                Timestamp = DateTime.UtcNow
            };
            
            var isSuccessful = await PostToInfluxDb(pointToWrite, InfluxDatabases.FundsTransfer);

            return isSuccessful;
        }
        public static async Task<bool> ProcessFundsTransferProcessedEvent(string eventData)
        {
            var transactionProcessedEvent = JsonConvert.DeserializeObject<TransactionProcessedEvent>(eventData);

            var totalTimeTaken = 0.0;
            FTTransactionDetails.TryGetValue(transactionProcessedEvent.transactionReference, out FTTransactionDetail transactionDetail);

            if(transactionDetail.StartTime.Year > 1) {
                totalTimeTaken = (Convert.ToDateTime(transactionProcessedEvent.dateUpdated) - transactionDetail.StartTime).TotalSeconds;
            }

             var pointToWrite = new Point()
            {
                Name = InfluxDataTables.FundsTransferInitiatedTable,
                Tags = new Dictionary<string, object>() 
                {
                    { "status", transactionProcessedEvent.status }
                },
                Fields = new Dictionary<string, object>()
                {
                    { "transactionReference", transactionProcessedEvent.transactionReference },
                    { "transactionType", transactionDetail.TransactionType },
                    { "startTime", transactionDetail.StartTime },
                    { "status", transactionProcessedEvent.status},
                    { "timeTaken", totalTimeTaken}
                },
                Timestamp = DateTime.UtcNow
            };

            var isSuccessful = await PostToInfluxDb(pointToWrite, InfluxDatabases.FundsTransfer);

            return isSuccessful;
        }
        public static async Task<bool> ProcessNameInquiryInitiatedEvent(string eventData)
        {
            var nameInquiryInitiated = JsonConvert.DeserializeObject<NameInquiryInitiatedEvent>(eventData);


            NITransactionDetails.Add(nameInquiryInitiated.Trxref, Convert.ToDateTime(nameInquiryInitiated.DateUpdated));

             var pointToWrite = new Point()
            {
                Name = InfluxDataTables.NameInquiryInitiatedTable,
                Tags = new Dictionary<string, object>() 
                {
                    { "status", TransactionStatus.Initiated}
                },
                Fields = new Dictionary<string, object>()
                {
                    { "transactionReference", nameInquiryInitiated.Trxref },
                    { "transactionType", "Name Inquiry" }
                },
                Timestamp = DateTime.UtcNow
            };

            var isSuccessful = await PostToInfluxDb(pointToWrite, InfluxDatabases.NameInquiry);
            return isSuccessful;
        }

        public static async Task<bool> ProcessNameInquiryProcessedEvent(string eventData)
        {
            var nameInquiryProcessed = JsonConvert.DeserializeObject<NameInquiryProcessedEvent>(eventData);

            var totalTimeTaken = 0.0;
            NITransactionDetails.TryGetValue(nameInquiryProcessed.TransactionReference, out DateTime timeUpdated);

            if(timeUpdated.Year > 1) {
                totalTimeTaken = (Convert.ToDateTime(nameInquiryProcessed.DateUpdated) - timeUpdated).TotalSeconds;
            }

             var pointToWrite = new Point()
            {
                Name = InfluxDataTables.NameInquiryInitiatedTable,
                Tags = new Dictionary<string, object>() 
                {
                    { "Status", nameInquiryProcessed.Status }
                },
                Fields = new Dictionary<string, object>()
                {
                    { "transactionReference", nameInquiryProcessed.TransactionReference },
                    { "transactionType", "Name Inquiry" },
                    { "status", nameInquiryProcessed.Status },
                    { "timeTaken", totalTimeTaken}
                },
                Timestamp = DateTime.UtcNow
            };

            var isSuccessful = await PostToInfluxDb(pointToWrite, InfluxDatabases.NameInquiry);
            return isSuccessful;
        }

        private static async Task<bool> PostToInfluxDb(Point pointToWrite, string databaseName)
        {
            appsettings = new AppSettings();
            using (StreamReader r = File.OpenText("appsettings.json"))
            {
                string json = r.ReadToEnd();
                json = json.Replace("\r\n","").Trim();
                appsettings = new AppSettings();
                appsettings = JsonConvert.DeserializeObject<AppSettings>(json);
            }
            var version = InfluxData.Net.Common.Enums.InfluxDbVersion.Latest;

            var username = appsettings.InfluxDbUser;
            var password = appsettings.InfluxDbPassword;

            var influxDbUrl = appsettings.InfluxDbUrl;
            var influxDbClient = new InfluxDbClient(influxDbUrl, username, password, version);

            var response = await influxDbClient.Database.CreateDatabaseAsync($"{databaseName}_{appsettings.BankCode}");
            var written = await influxDbClient.Client.WriteAsync(pointToWrite, $"{databaseName}_{appsettings.BankCode}");
            return written.Success;
        }
    }
}