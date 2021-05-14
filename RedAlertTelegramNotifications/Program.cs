using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.IO;
using System.Reflection;

namespace RedAlertTelegramNotifications
{
    public class Program
    {
        public static void Main(string[] args)
        {
            Directory.SetCurrentDirectory(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location));
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureServices((hostContext, services) =>
                {
                    services.AddSingleton<BotClient>();
                    services.AddSingleton<PersonAttackCache>();
                    services.AddSingleton<CityPersonMatcher>();
                    services.AddSingleton<RedAlertFetcher>();
                    services.AddHostedService<Worker>();
                }).UseWindowsService();
    }
}
