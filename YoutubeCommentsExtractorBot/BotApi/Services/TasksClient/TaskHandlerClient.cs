using BotApi.Database;
using BotApi.Services.DataStore;
using BotApi.Services.TelegramIntegration;
using CommentsExtractor;
using NLog;
using System.Text;
using Telegram.Bot.Types;

namespace BotApi.Services.TasksClient
{
    public class TaskHandlerClient
    {
        private readonly IDataStore dataStore;
        private readonly ITelegram telegram;
        private ExtractorAdapter extractorClient;
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();
        private List<YoutubeApiKey> apiKeys;

        DownloadFromVideoTask task;

        public TaskHandlerClient(IDataStore dataStore, ITelegram telegram)
        {
            this.dataStore = dataStore;
            this.telegram = telegram;
        }

        public async Task Handle(long taskId)
        {
            task = dataStore.GetDownloadFromVideoTasksById(taskId);
            task.BeginDate = DateTime.Now;

            try
            {
                await Extract();
                SaveTaskComplete();
            }
            catch (Exception ex)
            {
                string guid = Guid.NewGuid().ToString();
                logger.Error(guid, ex);
                SaveTaskFailed(ex.Message);
                await telegram.SendTextMessage(task.ChatId, "При выгрузке комментариев произошла неизвестная ошибка. Администратор работает над решением.");
                await telegram.SendTextMessage(Variables.GetInstance().BOT_OWNER_CHAT_ID, $"Произошла ошибка во время работы бота {guid}");
            }
            finally
            {
                UpdateApiKeys();
            }

        }

        private async Task Extract()
        {
            string videoId = task.VideoId;

            logger.Info("TASK HANDLER ID = " + videoId);

            extractorClient = initExtractorClient(videoId);

            await extractorClient.Run(videoId);

            var fs = extractorClient.GetExcelFile();

            await SendExcelFile(fs);

        }

        private ExtractorAdapter initExtractorClient(string videoId)
        {
            apiKeys = dataStore.GetActiveApiKeys().ToList();

            IYouTube youTubeClient = new YouTubeClient(apiKeys.Select(x => x.ApiKey).ToList());
            string pathCache = Path.Combine(Variables.GetInstance().CACHE_FOLDER, videoId.ToCharArray().FirstOrDefault().ToString(), videoId);

            IYouTube youTubeProxy = new YouTubeProxyClient(youTubeClient, pathCache);
            youTubeProxy.GetBlockedApiKeys();

            ExtractorAdapter extractorClient = new ExtractorAdapter(youTubeProxy);

            return extractorClient;
        }

        private async Task SendExcelFile(FileStream fs)
        {
            await telegram.SendFileMessage(task.ChatId, fs, $"{task.VideoId}.xlsx");

            fs.Close();
        }

        private void SaveTaskComplete()
        {
            task.CompleteDate = DateTime.Now;
            task.Completed = true;

            dataStore.UpdateDownloadTask(task);
        }

        private void SaveTaskFailed(string errorText)
        {
            task.CompleteDate = DateTime.Now;
            task.Completed = true;
            task.Failed = true;
            task.ErrorText = errorText;

            dataStore.UpdateDownloadTask(task);
        }

        private void UpdateApiKeys()
        {
            foreach (var key in extractorClient.GetBlockedApiKeys())
            {
                var model = apiKeys.Where(x => x.ApiKey.Equals(key)).FirstOrDefault();

                if (model == null) continue;

                model.UnblockingDate = DateOnly.FromDateTime(DateTime.Now.AddDays(1).Date);

                dataStore.UpdateApiKey(model);
            }
        }
    }
}
