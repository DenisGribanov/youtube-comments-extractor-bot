using BotApi.Database;

namespace BotApi.Services.DataStore
{
    public interface IDataStore
    {
        public void AddUser(TgUser tgUser);

        public TgUser? GetUser(long chatId);

        public DownloadFromVideoTask AddDownloadTask(DownloadFromVideoTask task);

        public void UpdateDownloadTask(DownloadFromVideoTask task);

        public List<YoutubeApiKey> GetActiveApiKeys();

        public List<DownloadFromVideoTask> GetDownloadFromVideoTasks(long userChatId);
        DownloadFromVideoTask GetDownloadFromVideoTasksById(long taskId);
        void UpdateApiKey(YoutubeApiKey model);
    }
}
