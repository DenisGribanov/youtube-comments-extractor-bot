using BotApi.Database;
using BotApi.Dto;
using BotApi.Services.DataStore;
using BotApi.Services.MessageBroker;
using BotApi.Services.TelegramIntegration;
using CommentsExtractor;
using NLog;

namespace BotApi.Services.TasksClient
{
    public class TaskCreatorClient
    {
        private readonly DataStore.IDataStore dataStore;
        private readonly ITelegram telegram;
        private readonly IMessageBrokerPub messageBroker;
        private int MAX_COMMENTS => Variables.GetInstance().MAX_COMMENTS;

        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        public TaskCreatorClient(IDataStore dataStore, ITelegram telegram, IMessageBrokerPub messageBroker)
        {
            this.dataStore = dataStore;
            this.telegram = telegram;
            this.messageBroker = messageBroker;
        }

        public async Task CreateDownloadTask(TgUser author, string videoId)
        {
            var user = dataStore.GetUser(author.ChatId);

            if (user == null)
            {
                CreateUser(author);
            }

            var notCompletedTaskExist = dataStore.GetDownloadFromVideoTasks(author.ChatId).FirstOrDefault(x => !x.Completed);

            if (notCompletedTaskExist != null)
            {
                await telegram.SendTextMessage(author.ChatId, $"Есть не завершенная задача \n\n {notCompletedTaskExist.VideoUrl}\n\nДождитесь выполнения");
                return;
            }

            var video = await GetVideoInfo(videoId);

            if (video == null)
            {
                await telegram.SendTextMessage(author.ChatId, $"Видео не найдено");
                return;
            }

            if (video.CommentCount.Value > Convert.ToUInt64(MAX_COMMENTS))
            {
                await telegram.SendTextMessage(author.ChatId, $"Превышен лимит комментариев. Максимум {MAX_COMMENTS}");
                return;
            }

            CreateAndPublishTask(video, author.ChatId);
        }

        private async Task<VideoInfoDto> GetVideoInfo(string videoId)
        {
            var keys = dataStore.GetActiveApiKeys().Select(x => x.ApiKey).ToList();

            var video = await VideoInfoClient.Init(keys).Get(videoId);

            return video;
        }

        private void CreateAndPublishTask(VideoInfoDto video, long authorChatId)
        {
            var newTask = CreateTaskModel(video, authorChatId);

            dataStore.AddDownloadTask(newTask);

            PublishTask(newTask.Id);
        }

        private static DownloadFromVideoTask CreateTaskModel(VideoInfoDto video, long authorChatId)
        {
            return new DownloadFromVideoTask()
            {
                ChannelTitle = video.ChannelTitle,
                ChatId = authorChatId,
                CreateDate = DateTime.Now,
                TotalComments = Convert.ToInt32(video.CommentCount.Value),
                UidTask = Guid.NewGuid(),
                VideoTitle = video.VideoTitle,
                VideoUrl = $"https://youtu.be/{video.VideoId}",
                VideoId = video.VideoId,

            };
        }

        private void CreateUser(TgUser tgUser)
        {
            dataStore.AddUser(tgUser);
        }

        private void PublishTask(long taskId)
        {
            messageBroker.Publishing(taskId);
        }
    }
}
