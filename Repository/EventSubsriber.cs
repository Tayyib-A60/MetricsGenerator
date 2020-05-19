using System;
using System.Text;
using System.Threading.Tasks;
using EventStore.ClientAPI;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using zoneswitch.metricsgenerator.Models.Events;

namespace zoneswitch.metricsgenerator.Repository
{
    public class EventSubscriber
    {
        public static async Task Process(EventStorePersistentSubscriptionBase sub, ResolvedEvent x)
        {
            var data = Encoding.ASCII.GetString(x.Event.Data);

            var eventObject = JObject.Parse(JsonConvert.SerializeObject(x).ToString());
            var mainEvent = eventObject["Event"];
            var eventData = mainEvent["Data"].ToString();

            var decodedData = Convert.FromBase64String(eventData);
            var datao = Encoding.UTF8.GetString(decodedData);

            try
            {
                switch (x.Event.EventType)
                {
                    case FundsTransferEvents.INITIATED:
                        var posted = await MetricsProcessor.ProcessFundsTransferInitiatedEvent(data);
                        if(posted)
                        {
                            sub.Acknowledge(x);
                            break;
                        }
                        else
                        {
                            sub.Fail(x, PersistentSubscriptionNakEventAction.Retry, "Unable to Process event, retrying");
                            break;
                        }
                    case FundsTransferEvents.PROCESSED:
                        var ftSuccessPosted = await MetricsProcessor.ProcessFundsTransferProcessedEvent(data);
                        
                        if(ftSuccessPosted)
                        {
                            sub.Acknowledge(x);
                            break;
                        }
                        else
                        {
                            sub.Fail(x, PersistentSubscriptionNakEventAction.Retry, "Unable to Process event, retrying");
                            break;
                        }
                    case NameInquiryEvents.INITIATED:
                        var nameInquiryInitiated = await MetricsProcessor.ProcessNameInquiryInitiatedEvent(data);

                        if(nameInquiryInitiated)
                        {
                            sub.Acknowledge(x);
                            break;
                        }
                        else
                        {
                            sub.Fail(x, PersistentSubscriptionNakEventAction.Retry, "Unable to Process event, retrying");
                            break;
                        }
                    case NameInquiryEvents.PROCESSED:
                        var nameInquiryProcessed = await MetricsProcessor.ProcessNameInquiryProcessedEvent(data);

                        if(nameInquiryProcessed)
                        {
                            sub.Acknowledge(x);
                            break;
                        }
                        else
                        {
                            sub.Fail(x, PersistentSubscriptionNakEventAction.Retry, "Unable to Process event, retrying");
                            break;
                        }
                    case IsoFundsTransferEvents.INITIATED:
                        var isoFTInitiated = await MetricsProcessor.ProcessISOFundsTransferInitiatedEvent(data);

                        if(isoFTInitiated)
                        {
                            sub.Acknowledge(x);
                            break;
                        }
                        else
                        {
                            sub.Fail(x, PersistentSubscriptionNakEventAction.Retry, "Unable to Process event, retrying");
                            break;
                        }
                    case IsoFundsTransferEvents.PROCESSED:
                        var isoFTProcessed = await MetricsProcessor.ProcessISOFundsTransferProcessedEvent(data);

                        if(isoFTProcessed)
                        {
                            sub.Acknowledge(x);
                            break;
                        }
                        else
                        {
                            sub.Fail(x, PersistentSubscriptionNakEventAction.Retry, "Unable to Process event, retrying");
                            break;
                        }
                    case EnvironmentSpecificEvents.WINDOWS_RESOURCES_EN:
                        var resourceEventProcessed = await MetricsProcessor.ProcessWindowsResourceEvents(data);

                        if(resourceEventProcessed)
                        {
                            sub.Acknowledge(x);
                            break;
                        }
                        else
                        {
                            sub.Fail(x, PersistentSubscriptionNakEventAction.Retry, "Unable to Process event, retrying");
                            break;
                        }
                    case EnvironmentSpecificEvents.LINUX_RESOURCES_EN:
                        var linuxEventProcessed = await MetricsProcessor.ProcessLinuxResourcesEvents(data);

                        if(linuxEventProcessed)
                        {
                            sub.Acknowledge(x);
                            break;
                        }
                        else
                        {
                            sub.Fail(x, PersistentSubscriptionNakEventAction.Retry, "Unable to Process event, retrying");
                            break;
                        }
                    default:
                        sub.Fail(x, PersistentSubscriptionNakEventAction.Retry, "Unable to Process event, retrying");
                        break;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
    }
}