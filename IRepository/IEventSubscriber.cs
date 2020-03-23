using System.Threading.Tasks;
using EventStore.ClientAPI;

namespace zoneswitch.metricsgenerator.IRepository
{
    public interface IEventSubscriber
    {
        Task SubscribeToEvents(EventStorePersistentSubscriptionBase sub, ResolvedEvent x,int ? y);
    }
}