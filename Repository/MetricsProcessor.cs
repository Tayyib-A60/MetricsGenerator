using System;
using System.Collections.Generic;
using System.IO;
using InfluxDB.Client;
using InfluxDB.Client.Api.Domain;
using InfluxDB.Client.Writes;
using System.Threading.Tasks;
using InfluxData.Net.InfluxDb;
using InfluxData.Net.InfluxDb.Models;
using Newtonsoft.Json;
using zoneswitch.metricsgenerator.Extensions;
using zoneswitch.metricsgenerator.Models;
using zoneswitch.metricsgenerator.Models.DbData;
using NLog;

namespace zoneswitch.metricsgenerator.Repository
{
    public class MetricsProcessor
    {
        private static readonly Logger _logger = LogManager.GetCurrentClassLogger();
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

        public static async Task<bool> ProcessLinuxResourcesEvents(string eventData)
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
                postSuccessful = postSuccessful && await PostToInfluxDb(pointToWrite, InfluxDatabases.LinuxResources);
            }

            foreach (var linuxServerMetrics in linuxEnvironmentEvent.SystemStatistics)
            {
                double cpuUsage = 0;
                double ramUsage = 0;
                double hdUsage = 0;
                int networkSpeed = 0;

                // Convert entry(ies) to number type(s)
                if(linuxServerMetrics.CPUUsage.Length > 2)
                    Double.TryParse(linuxServerMetrics.CPUUsage.Substring(0, linuxServerMetrics.CPUUsage.Length-2), out cpuUsage);
                if(linuxServerMetrics.RAMUsage.Length > 2)
                    Double.TryParse(linuxServerMetrics.RAMUsage.Substring(0, linuxServerMetrics.RAMUsage.Length-2), out ramUsage);
                if(linuxServerMetrics.HardDiskUsage.Length > 2)
                    Double.TryParse(linuxServerMetrics.HardDiskUsage.Substring(0, linuxServerMetrics.HardDiskUsage.Length-2), out hdUsage);
                if(linuxServerMetrics.NetworkSpeed.Length > 2)
                    Int32.TryParse(linuxServerMetrics.NetworkSpeed.Substring(0, linuxServerMetrics.NetworkSpeed.Length-4), out networkSpeed);

                var pointToWrite = new Point
                {
                    Name = InfluxDataTables.LinuxResourcesTable,
                    Tags = new Dictionary<string, object>
                    {
                        { "OS", "Linux" },
                    },
                    Fields = new Dictionary<string, object>
                    {
                        { "CPUUsage", cpuUsage == 0.0 ? 1.0 : cpuUsage },
                        { "RAMUsage", ramUsage == 0.0 ? 1.0 : ramUsage },
                        { "HardDiskUsage", hdUsage == 0.0 ? 1.0 : ramUsage },
                        { "NetworkSpeed",  networkSpeed == 0 ? 1 : networkSpeed }
                    },
                    Timestamp = DateTime.UtcNow
                };
                postSuccessful = postSuccessful && await PostToInfluxDb(pointToWrite, InfluxDatabases.LinuxResources);
            }

            return postSuccessful;
        }

        public static async Task<bool> ProcessWindowsResourceEvents(string eventData)
        {
            var resourceEventData = JsonConvert.DeserializeObject<ResourceEventData>(eventData);

             _logger.Debug("Data for win resources received from event store");

            // PostToInfluxDbTwoPointO(resourceEventData);

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

            _logger.Debug($"Data for win resources Posted: {isSuccessful}");
            return isSuccessful;
        }


        public static async Task<List<long>> ProcessUniqueAccounts(List<UniqueAccount> accounts)
        {
            var processedAccounts = new List<long>();

            foreach (var account in accounts)
            {
                var pointToWrite = new Point()
                {
                    Name = InfluxDataTables.UniqueAccountsTable,
                    Tags = new Dictionary<string, object>() 
                    {
                        { "MonthYear", account.MonthYear }
                    },
                    Fields = new Dictionary<string, object>()
                    {
                        { "AccountNumber", account.AccountNumber },
                        { "TransactionCount", account.TransactionCount }
                    },
                    Timestamp = DateTime.UtcNow
                };

                var isSuccessful = await PostToInfluxDb(pointToWrite, InfluxDatabases.UniqueMetrics);

                if(isSuccessful) {
                    processedAccounts.Add(account.Id);
                }

            }

            return processedAccounts;
        }

        public static async Task<List<long>> ProcessUniqueCards(List<UniqueCard> cards)
        {
            var processedCardIds = new List<long>();

            foreach (var card in cards)
            {
                var pointToWrite = new Point()
                {
                    Name = InfluxDataTables.UniqueAccountsTable,
                    Tags = new Dictionary<string, object>() 
                    {
                        { "MonthYear", card.MonthYear }
                    },
                    Fields = new Dictionary<string, object>()
                    {
                        { "CardNo", card.CardNo },
                        { "TransactionCount", card.TransactionCount }
                    },
                    Timestamp = DateTime.UtcNow
                };

                var isSuccessful = await PostToInfluxDb(pointToWrite, InfluxDatabases.UniqueMetrics);

                if(isSuccessful) {
                    processedCardIds.Add(card.Id);
                } 

            }

            return processedCardIds;
        }
        
        private static void PostToInfluxDbTwoPointO(ResourceEventData eventData)
        {
            var influxDBClient = InfluxDBClientFactory.Create(appsettings.InfluxDbTwoPoint0DbUrl, appsettings.Token.ToCharArray());

            Console.WriteLine("Writing resource metrics data for 2.0");

            // Write Data
            using (var writeApi = influxDBClient.GetWriteApi())
            {
                // Write by Point
                var point = PointData.Measurement("windowsMetrics")
                    .Tag("bankCode", appsettings.BankCode)
                    // .Field("value", 55D)
                    .Timestamp(DateTime.UtcNow.AddSeconds(-10), WritePrecision.Ns);
                
                writeApi.WritePoint(appsettings.InfluxDbBucketName, appsettings.InfluxDbOrg, point);
                
                // Write by LineProtocol
                writeApi.WriteRecord(appsettings.InfluxDbBucketName, appsettings.InfluxDbOrg, WritePrecision.Ns, "WinMetrics");

                Console.WriteLine("Writing record");
                
                // Write by POCO
                var win_metrics = new WindowsMetrics {Processor = eventData.Processor, AvailableDiskSpace = eventData.AvailableDiskSpace, Time = DateTime.UtcNow, AvailableRamInMB = eventData.AvailableRamInMB, TotalDiskSpace = eventData.TotalDiskSpace };
                writeApi.WriteMeasurement(appsettings.InfluxDbBucketName, appsettings.InfluxDbOrg, WritePrecision.Ns, win_metrics);

                Console.WriteLine("Record written");


            // Query data
            // var flux = $"from({appsettings.InfluxDbBucketName}:\"TotalDiskSpace\")";

            // var fluxTables = await influxDBClient.GetQueryApi().QueryAsync(flux, appsettings.InfluxDbOrg);
            // fluxTables.ForEach(fluxTable =>
            // {
            //     var fluxRecords = fluxTable.Records;
            //     fluxRecords.ForEach(fluxRecord =>
            //     {
            //         Console.WriteLine($"{fluxRecord.GetTime()}: {fluxRecord.GetValue()}");
            //     });
            // });

            influxDBClient.Dispose();

            }
            
        }

        private static async Task<bool> PostToInfluxDb(Point pointToWrite, string databaseName)
        {
            appsettings = new AppSettings();
            using (StreamReader r = System.IO.File.OpenText("appsettings.json"))
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
