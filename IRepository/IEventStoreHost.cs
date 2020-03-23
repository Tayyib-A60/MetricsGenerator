using EventStore.ClientAPI;

namespace zoneswitch.metricsgenerator.IRepository
{
    public interface IEventStoreHost
    {
        void CreateSubscription(IEventStoreConnection conn, string stream, string group);
        IEventStoreConnection ConnectToEventStore();
        
    }
}