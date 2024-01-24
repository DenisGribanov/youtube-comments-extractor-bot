using CommentsExtractor;
using Microsoft.AspNetCore.Mvc;
using Telegram.Bot;
using Telegram.Bot.Types;
using RabbitMQ.Client;
using System.Text;
using RabbitMQ.Client.Events;
using BotApi.Services;
using BotApi.Database;
using BotApi.Services.TasksClient;
using BotApi.Services.TelegramIntegration;
using BotApi.Services.MessageBroker;
using BotApi.Services.DataStore;

namespace BotApi.Controllers
{
    [ApiController]
    [Route("comments-extract-bot")]
    public class BotController : Controller
    {
        private readonly ILogger<BotController> logger;
        private readonly IConfiguration configuration;
        private TaskCreatorClient taskCreator;
        private ITelegram telegramAdapter;
        private IMessageBrokerPub messageBrokerPub;

        public BotController(ILogger<BotController> logger, 
            IConfiguration configuration, 
            ITelegram telegram, 
            IMessageBrokerPub messageBrokerPub,
            YotubeCommentsExtractorBotContext dbContext)
        {
            this.logger = logger;
            this.configuration = configuration;
            this.telegramAdapter = telegram;
            this.messageBrokerPub = messageBrokerPub;
            this.taskCreator = new TaskCreatorClient(new DataStoreImpl(dbContext), this.telegramAdapter, this.messageBrokerPub);
        }


        [HttpGet]
        public IActionResult Index()
        {
            return Ok(Variables.GetInstance().BOT_DOMAIN_NAME);
        }

        [HttpPost("update")]
        public async Task<IActionResult> Post([FromBody] Update update)
        {
            if (update == null) return BadRequest();

            logger.LogInformation(Newtonsoft.Json.JsonConvert.SerializeObject(update));

            await CallBot(update);

            return Ok();
        }

        private async Task CallBot(Update update)
        {
            if (update.Message == null) return;

            if (update.Message.EntityValues != null && (update.Message.EntityValues.Contains("/start")
                || update.Message.EntityValues.Contains("/help")))
            {
                await telegramAdapter.SendTextMessage(update.Message.From.Id,
                    "Пришлите мне ссылку на youtube видео, а я выгружу комментарии в excel файл");
            }
            else if (update.Message.Entities != null && update.Message.Entities.Any(x => x.Type == Telegram.Bot.Types.Enums.MessageEntityType.Url))
            {
                string url = update.Message.EntityValues.FirstOrDefault();

                string videoId = YouTubeCommentsExtract.GetVideoIdFromUrl(url);

                if (videoId == null)
                {
                    await telegramAdapter.SendTextMessage(update.Message.From.Id, "Не удалось распознать ссылку на ютуб");
                    return;
                }
                else
                {
                    await telegramAdapter.SendTextMessage(update.Message.From.Id, "Работаю. Пожалуйста подождите");

                    await CreateDownloadTask(new TgUser()
                    {
                        ChatId = update.Message.From.Id,
                        FirstName = update.Message.From.FirstName,
                        UserName = update.Message.From.Username,
                        CreateData = DateTime.Now,
                    }, videoId);

                }

            }
        }

        private async Task CreateDownloadTask(TgUser author, string videoId)
        {
            await taskCreator.CreateDownloadTask(author, videoId);
        }

    }
}
