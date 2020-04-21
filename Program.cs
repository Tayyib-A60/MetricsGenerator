using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using zoneswitch.metricsgenerator.Repository;

namespace zoneswitch.metricsgenerator
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureServices((hostContext, services) =>
                {
                    services.AddSingleton<UniqueAccountProcessor>();
                    services.AddSingleton<UniqueCardProcessor>();
                    // services.AddSingleton<IMetricsProcessor, MetricsProcessor>();
                    // services.AddSingleton<IEventSubscriber, EventSubsriber>();
                    services.AddHostedService<Worker>();
                });
    }
}
