using BotApi.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using System.Threading.Tasks;

namespace BotApi.Services.DataStore
{
    public class DataStoreImpl : IDataStore
    {
        private readonly YotubeCommentsExtractorBotContext context;

        public DataStoreImpl(YotubeCommentsExtractorBotContext context)
        {
            this.context = context;
        }

        public DownloadFromVideoTask AddDownloadTask(DownloadFromVideoTask task)
        {
            if (task == null) throw new ArgumentNullException(nameof(task));

            context.Entry(task).State = EntityState.Added;

            context.SaveChanges();

            return task;
        }

        public void AddUser(TgUser tgUser)
        {
            if (tgUser == null) throw new ArgumentNullException(nameof(tgUser));

            context.Entry(tgUser).State = EntityState.Added;

            context.SaveChanges();
        }

        public List<YoutubeApiKey> GetActiveApiKeys()
        {
            return context.YoutubeApiKeys.Where(x => !x.Deleted.Value
                                                && (!x.UnblockingDate.HasValue
                                                || x.UnblockingDate.Value < DateOnly.FromDateTime(DateTime.Now))).ToList();
        }

        public List<DownloadFromVideoTask> GetDownloadFromVideoTasks(long userChatId)
        {
            return context.DownloadFromVideoTasks.Where(x => x.ChatId == userChatId).ToList();
        }

        public DownloadFromVideoTask GetDownloadFromVideoTasksById(long taskId)
        {
            return context.DownloadFromVideoTasks.Where(x => x.Id == taskId).FirstOrDefault();
        }

        public TgUser? GetUser(long chatId)
        {
            return context.TgUsers.FirstOrDefault(x => x.ChatId == chatId);
        }

        public void UpdateApiKey(YoutubeApiKey model)
        {
            context.Entry(model).State = EntityState.Modified;
            context.SaveChanges();
        }

        public void UpdateDownloadTask(DownloadFromVideoTask task)
        {
            context.Entry(task).State = EntityState.Modified;

            context.SaveChanges();
        }
    }
}
