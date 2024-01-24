namespace BotApi.Services.TelegramIntegration
{
    public interface ITelegram
    {
        Task SendTextMessage(long destanationChatId, string text);

        Task SendFileMessage(long destanationChatId, FileStream file, string fileName);
    }
}
