namespace BotApi.Services.MessageBroker
{
    public interface IMessageBrokerSub
    {
        void Subscribe();
        event EventHandler<NewTaskEventArgs> handler;
    }
}
