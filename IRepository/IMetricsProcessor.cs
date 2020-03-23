using System.Threading.Tasks;

namespace zoneswitch.metricsgenerator.IRepository
{
    public interface IMetricsProcessor
    {
         Task<bool> ProcessFundsTransferInitiatedEvent(string eventData);
         Task<bool> ProcessFundsTransferCompletedEvent(string eventData);
         Task<bool> ProcessFundsTransferProcessedEvent(string eventData);
    }
}