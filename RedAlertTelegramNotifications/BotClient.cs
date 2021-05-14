using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Args;
using File = System.IO.File;

namespace RedAlertTelegramNotifications
{
    public class BotClient
    {
        private const string CHATS_FILE_NAME = "chats.txt";
        private readonly TelegramBotClient botClient;
        private readonly List<long> chatIds;

        private readonly ILogger<BotClient> _logger;

        public BotClient(ILogger<BotClient> logger)
        {
            botClient = new("SECRET");
            botClient.OnMessage += BotClient_OnMessage;

            chatIds = new List<long>();
            _logger = logger;
        }

        public void Start(CancellationToken cancellationToken)
        {
            botClient.StartReceiving(cancellationToken: cancellationToken);

            if (File.Exists(CHATS_FILE_NAME))
            {
                _logger.LogInformation("Adding chats");
                chatIds.AddRange(File.ReadAllLines(CHATS_FILE_NAME).Select(c => long.Parse(c)));
            }
        }

        public Task SendTextMessageAsync(string message, CancellationToken cancellationToken)
        {
            foreach (var chatId in chatIds)
            {
                botClient.SendTextMessageAsync(chatId, message, cancellationToken: cancellationToken);
            }

            return Task.CompletedTask;
        }

        private void BotClient_OnMessage(object sender, MessageEventArgs e)
        {
            var chatId = e.Message.Chat.Id;
            if (e.Message.Text == "/start")
            {
                botClient.SendTextMessageAsync(e.Message.Chat, $"נרשמת בהצלחה לבוט!");
                botClient.SendTextMessageAsync(e.Message.Chat, $"אם ברצונך לבטל את הרישום שלח /stop");

                if (!chatIds.Contains(chatId))
                {
                    _logger.LogInformation($"Added new chat {e.Message.Chat.FirstName} {e.Message.Chat.LastName}");
                    chatIds.Add(chatId);
                    File.AppendAllText(CHATS_FILE_NAME, chatId + Environment.NewLine);
                }
            }
            else if (e.Message.Text == "/stop")
            {
                botClient.SendTextMessageAsync(e.Message.Chat, "בוטל רישומך לעדכונים מהבוט");

                if (chatIds.Contains(chatId))
                {
                    _logger.LogInformation($"Remove chat with {e.Message.Chat.FirstName} {e.Message.Chat.LastName}");
                    chatIds.Remove(chatId);
                    File.WriteAllLines(CHATS_FILE_NAME, chatIds.Select(c => c.ToString()));
                }
            }
        }
    }
}