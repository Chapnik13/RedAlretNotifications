using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Timers;

namespace RedAlertTelegramNotifications
{
    public class PersonAttackCache
    {
        private const int INTERVAL = 1 * 60 * 1000;

        private readonly Timer timer;
        private readonly ConcurrentDictionary<string, DateTime> cache;

        private readonly ILogger logger;

        public PersonAttackCache(ILogger<PersonAttackCache> logger)
        {
            timer = new(INTERVAL);
            timer.Elapsed += Timer_Elapsed;
            timer.AutoReset = true;
            timer.Enabled = true;

            cache = new();

            this.logger = logger;
        }

        public List<string> Get()
        {
            return cache.Keys.ToList();
        }

        public void Update(List<string> people)
        {
            var now = DateTime.Now;

            foreach (var person in people)
            {
                cache.AddOrUpdate(person, now, (_, _) => now);
            }
        }

        private void Timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            logger.LogInformation("Cleaning cache");
            var timeToClear = DateTime.Now - new TimeSpan(0, 10, 0);
            var itemsToClear = cache.Where(p => p.Value <= timeToClear);

            foreach (var item in itemsToClear)
            {
                if (!cache.TryRemove(item))
                {
                    logger.LogWarning($"Could not remove item {item}");
                }
            }

            logger.LogInformation($"Cleaned {itemsToClear.Count()} items");
        }
    }
}
