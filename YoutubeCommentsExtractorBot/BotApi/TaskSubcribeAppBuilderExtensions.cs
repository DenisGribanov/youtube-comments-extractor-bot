using BotApi.Services.MessageBroker;
using BotApi.Services.TelegramIntegration;
using BotApi.Services.TasksClient;
using BotApi.Services.DataStore;
using NLog;

namespace BotApi
{
    public static class TaskSubcribeAppBuilderExtensions
    {   
        private static readonly Logger Log = LogManager.GetCurrentClassLogger();
        public static IApplicationBuilder SubscribeDownloadTask(this IApplicationBuilder app, 
            IMessageBrokerSub messageBrokerSub,
            IDataStore dataStore, 
            ITelegram telegram)
        {
            try
            {
                var TaskHandlerClient = new TaskHandlerClient(dataStore, telegram);
                messageBrokerSub.Subscribe();
                messageBrokerSub.handler += async (object? sender, NewTaskEventArgs e) => await TaskHandlerClient.Handle(e.TaskId);

                return app;
            } catch (Exception ex)
            {
                Log.Error(ex);
                throw;
            }

        }

    }
}
