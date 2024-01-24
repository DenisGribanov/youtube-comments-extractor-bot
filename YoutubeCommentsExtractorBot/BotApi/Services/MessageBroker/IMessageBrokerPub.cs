namespace BotApi.Services.MessageBroker
{
    public interface IMessageBrokerPub
    {
        void Publishing(long taskId);
    }
}
