using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace RedAlertTelegramNotifications
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private readonly RedAlertFetcher redAlertFetcher;
        private readonly CityPersonMatcher cityPersonMatcher;
        private readonly PersonAttackCache personAttackCache;
        private readonly BotClient botClient;

        public Worker(ILogger<Worker> logger, RedAlertFetcher redAlertFetcher, CityPersonMatcher cityPersonMatcher, PersonAttackCache personAttackCache, BotClient botClient)
        {
            _logger = logger;
            this.redAlertFetcher = redAlertFetcher;
            this.cityPersonMatcher = cityPersonMatcher;
            this.personAttackCache = personAttackCache;
            this.botClient = botClient;
        }

        public override Task StartAsync(CancellationToken cancellationToken)
        {
            cityPersonMatcher.InitializeMatcherData();
            botClient.Start(cancellationToken);

            return base.StartAsync(cancellationToken);
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var lastResponseId = 0L;

            while (!stoppingToken.IsCancellationRequested)
            {
                var response = await redAlertFetcher.Fetch(stoppingToken);
                if (response == null || response.id == lastResponseId) continue;
                lastResponseId = response.id;
                _logger.LogInformation("A missle attack occurs!!!");
                var peopleAttacked = cityPersonMatcher.GetPeople(response.data);
                if (peopleAttacked.Count > 0)
                {
                    var cache = personAttackCache.Get();
                    personAttackCache.Update(peopleAttacked);

                    var peopleToUpdateAbout = peopleAttacked.Except(cache);

                    if (peopleToUpdateAbout.Any())
                    {
                        await botClient.SendTextMessageAsync("🚨" + peopleToUpdateAbout.Aggregate((s1, s2) => s1 + ", " + s2) + "🚨", cancellationToken: stoppingToken);
                    }
                }

                await Task.Delay(1000, stoppingToken);
            }
        }
    }
}
