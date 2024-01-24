using RabbitMQ.Client;

namespace BotApi.Services.MessageBroker.RabbitClient
{
    public abstract class RabbitClientBase
    {
        private readonly string hostName;
        private readonly string userName;
        private readonly string password;

        protected RabbitClientBase(string hostName, string userName, string password)
        {
            this.hostName = hostName;
            this.userName = userName;
            this.password = password;
        }

        protected IConnection GetConnection()
        {
            var factory = new ConnectionFactory()
            {
                HostName = hostName,
                Port = 5672,
                UserName = userName,
                Password = password,
                RequestedHeartbeat = new TimeSpan(60)

            };

            var connection = factory.CreateConnection();
            return connection;
        }
    }
}
