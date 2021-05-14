using System;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace RedAlertTelegramNotifications
{
    public class RedAlertFetcher
    {
        private const string ALERTS_URL = "https://www.oref.org.il/WarningMessages/alert/alerts.json";
        private readonly HttpClient orefClient;

        public RedAlertFetcher()
        {
            orefClient = new();
            orefClient.DefaultRequestHeaders.Referrer = new Uri("https://www.oref.org.il/11226-he/pakar.aspx");
            orefClient.DefaultRequestHeaders.Add("X-Requested-With", "XMLHttpRequest");
        }

        public async Task<OrefApiResponse> Fetch(CancellationToken stoppingToken)
        {
            try
            {
                var response = await orefClient.GetAsync(ALERTS_URL, stoppingToken);
                var responseContent = await response.Content.ReadAsStringAsync(stoppingToken);
                if (responseContent.Length > 0)
                {
                    return JsonSerializer.Deserialize<OrefApiResponse>(responseContent);
                }

                return null;
            }
            catch (Exception)
            {
                return null;
            }
        }
    }

    public record OrefApiResponse(string[] data, long id, string title);
}
