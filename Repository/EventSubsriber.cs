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

            // Console.WriteLine("26 subscriber ....Feeded with event ", JsonConvert.SerializeObject(x.Event));
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
                        //  Console.WriteLine("33 subscriber ....Picking Initiated transaction", data);
                        
                        
                        var posted = await MetricsProcessor.ProcessFundsTransferInitiatedEvent(data);
                        if(posted)
                        {
                            sub.Acknowledge(x);
                            break;
                        }
                        else
                        {
                            sub.Fail(x, PersistentSubscriptionNakEventAction.Park, "Unable to Process event");
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
                            sub.Fail(x, PersistentSubscriptionNakEventAction.Park, "Unable to Process event");
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
                            sub.Fail(x, PersistentSubscriptionNakEventAction.Park, "Unable to Process event");
                            break;
                        }
                    default:
                        sub.Fail(x, PersistentSubscriptionNakEventAction.Park, "Unable to Process event");
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