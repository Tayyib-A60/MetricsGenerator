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
        public static Dictionary<string, DateTime> IsoFTTransactionDetails = new Dictionary<string, DateTime>();
        public static async Task<bool> ProcessFundsTransferInitiatedEvent(string eventData)
        {
            var transactionInitiatedEvent = JsonConvert.DeserializeObject<TransactionInitiatedEvent>(eventData);
            var transactionDetail = new FTTransactionDetail {
                SenderBank = transactionInitiatedEvent.senderBankId,
                TransactionType = transactionInitiatedEvent.transactionType,
                Amount = transactionInitiatedEvent.amount,
                StartTime = Convert.ToDateTime(transactionInitiatedEvent.dateCreated)
            };

            if(!FTTransactionDetails.ContainsKey(transactionInitiatedEvent.transactionReference)) {
                FTTransactionDetails.Add(transactionInitiatedEvent.transactionReference, transactionDetail);
            }

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
                Name = InfluxDataTables.FundsTransferProcessedTable,
                Tags = new Dictionary<string, object>() 
                {
                    { "status", transactionProcessedEvent.status }
                },
                Fields = new Dictionary<string, object>()
                {
                    { "transactionReference", transactionProcessedEvent.transactionReference },
                    { "transactionType", transactionDetail.TransactionType },
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

            if(!NITransactionDetails.ContainsKey(nameInquiryInitiated.TransactionReference)) {
                NITransactionDetails.Add(nameInquiryInitiated.TransactionReference, Convert.ToDateTime(nameInquiryInitiated.DateUpdated));
            }

             var pointToWrite = new Point()
            {
                Name = InfluxDataTables.NameInquiryInitiatedTable,
                Tags = new Dictionary<string, object>() 
                {
                    { "status", TransactionStatus.Initiated}
                },
                Fields = new Dictionary<string, object>()
                {
                    { "transactionReference", nameInquiryInitiated.TransactionReference },
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
                Name = InfluxDataTables.NameInquiryProcessedTable,
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

        public static async Task<bool> ProcessISOFundsTransferInitiatedEvent(string eventData)
        {
            var isoFTEvent = JsonConvert.DeserializeObject<IsoFundsTransferEvent>(eventData);

            if(!IsoFTTransactionDetails.ContainsKey(isoFTEvent.MsgTypeAndTransactionReference.Substring(4))) {
                IsoFTTransactionDetails.Add(isoFTEvent.MsgTypeAndTransactionReference.Substring(4), Convert.ToDateTime(isoFTEvent.DateUpdated));
            }

             var pointToWrite = new Point()
            {
                Name = InfluxDataTables.IsoFundsTransferInitiatedTable,
                Tags = new Dictionary<string, object>() 
                {
                    { "Status", TransactionStatus.Initiated }
                },
                Fields = new Dictionary<string, object>()
                {
                    { "transactionReference", isoFTEvent.MsgTypeAndTransactionReference.Substring(4) },
                    { "transactionType", "Iso FundsTransfer" }
                },
                Timestamp = DateTime.UtcNow
            };

            var isSuccessful = await PostToInfluxDb(pointToWrite, InfluxDatabases.IsoFundsTransfer);
            return isSuccessful;
        }

        public static async Task<bool> ProcessISOFundsTransferProcessedEvent(string eventData)
        {
            var isoFTEvent = JsonConvert.DeserializeObject<IsoFundsTransferEvent>(eventData);

            if(String.IsNullOrEmpty(isoFTEvent.ResponseCode) || isoFTEvent.MessageTypeIndicator != "210" || !Convert.ToBoolean(isoFTEvent.FromSwitch)) {
                return true;
            }
            var totalTimeTaken = 0.0;
            NITransactionDetails.TryGetValue(isoFTEvent.MsgTypeAndTransactionReference.Substring(4), out DateTime timeUpdated);

            if(timeUpdated.Year > 1) {
                totalTimeTaken = (Convert.ToDateTime(isoFTEvent.DateUpdated) - timeUpdated).TotalSeconds;
            }

             var pointToWrite = new Point()
            {
                Name = InfluxDataTables.IsoFundsTransferProcessedTable,
                Tags = new Dictionary<string, object>() 
                {
                    { "Status", isoFTEvent.ResponseCode }
                },
                Fields = new Dictionary<string, object>()
                {
                    { "transactionReference", isoFTEvent.MsgTypeAndTransactionReference.Substring(4) },
                    { "transactionType", "Iso FundsTransfer" },
                    { "totalTime", totalTimeTaken }
                },
                Timestamp = DateTime.UtcNow
            };

            var isSuccessful = await PostToInfluxDb(pointToWrite, InfluxDatabases.IsoFundsTransfer);
            return isSuccessful;
        }

        public static async Task<bool> ProcessLinuxEvents(string eventData)
        {
            var linuxEnvironmentEvent = JsonConvert.DeserializeObject<LinuxEnvironmentEvent>(eventData);
            var postSuccessful = false;

            foreach (var serviceMetrics in linuxEnvironmentEvent.ServiceStatistics)
            {
                var pointToWrite = new Point
                {
                    Name = serviceMetrics.ServiceName,
                    Tags = new Dictionary<string, object>
                    {
                        { "FunctionName", serviceMetrics.FunctionName },
                    },
                    Fields = new Dictionary<string, object>
                    {
                        { "AverageResponseTime", serviceMetrics.AverageResponseTime },
                        { "RequestRate", serviceMetrics.RequestRate },
                        { "ResponseRate", serviceMetrics.ResponseRate },
                        { "SuccessRate", serviceMetrics.SuccessRate }
                    },
                    Timestamp = DateTime.UtcNow
                };
                var isSuccessful = await PostToInfluxDb(pointToWrite, InfluxDatabases.LinuxEnvironment);
            }

            foreach (var linuxServerMetrics in linuxEnvironmentEvent.SystemStatistics)
            {
                // Convert entry(ies) to number type(s)
                Double.TryParse(linuxServerMetrics.CPUUsage.Substring(0, linuxServerMetrics.CPUUsage.Length-2), out double cpuUsage);

                Double.TryParse(linuxServerMetrics.RAMUsage.Substring(0, linuxServerMetrics.CPUUsage.Length-2), out double ramUsage);

                Double.TryParse(linuxServerMetrics.HardDiskUsage.Substring(0, linuxServerMetrics.CPUUsage.Length-2), out double hdUsage);

                Double.TryParse(linuxServerMetrics.NetworkSpeed.Substring(0, linuxServerMetrics.CPUUsage.Length-4), out double networkSpeed);

                var pointToWrite = new Point
                {
                    Name = InfluxDataTables.LinuxResourcesTable,
                    Tags = new Dictionary<string, object>
                    {
                        { "OS", "Linux" },
                    },
                    Fields = new Dictionary<string, object>
                    {
                        { "CPUUsage", cpuUsage },
                        { "RAMUsage", ramUsage },
                        { "HardDiskUsage", hdUsage },
                        { "NetworkSpeed",  networkSpeed }
                    },
                    Timestamp = DateTime.UtcNow
                };
                postSuccessful = await PostToInfluxDb(pointToWrite, InfluxDatabases.LinuxEnvironment);
            }

            return postSuccessful;
        }

        public static async Task<bool> ProcessResourceEvents(string eventData)
        {
            var resourceEventData = JsonConvert.DeserializeObject<ResourceEventData>(eventData);

             var pointToWrite = new Point()
            {
                Name = InfluxDataTables.WindowResourcesTable,
                Tags = new Dictionary<string, object>() 
                {
                    { "OS", "Windows" }
                },
                Fields = new Dictionary<string, object>()
                {
                    { "TotalDiskSpace", resourceEventData.TotalDiskSpace },
                    { "AvailableDiskSpace", resourceEventData.AvailableDiskSpace },
                    { "AvailableRamInMB", resourceEventData.AvailableRamInMB },
                    { "Processor", resourceEventData.Processor }
                },
                Timestamp = DateTime.UtcNow
            };

            var isSuccessful = await PostToInfluxDb(pointToWrite, InfluxDatabases.WindowResources);
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