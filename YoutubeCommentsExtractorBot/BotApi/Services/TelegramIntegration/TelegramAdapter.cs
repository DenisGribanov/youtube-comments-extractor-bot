using NLog;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace BotApi.Services.TelegramIntegration
{
    public class TelegramAdapter : ITelegram
    {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();
        private readonly ITelegramBotClient BotClient;

        public TelegramAdapter(ITelegramBotClient botClient)
        {
            BotClient = botClient;
        }

        public async Task SendFileMessage(long destanationChatId, FileStream file, string fileName )
        {
            await BotClient.SendDocumentAsync(destanationChatId, new InputFileStream(file, fileName));
        }

        public async Task SendTextMessage(long destanationChatId, string text)
        {
            await BotClient.SendTextMessageAsync(destanationChatId, text);
        }
    }
}
