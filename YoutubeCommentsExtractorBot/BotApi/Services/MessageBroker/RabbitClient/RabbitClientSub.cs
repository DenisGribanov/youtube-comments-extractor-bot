using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;

namespace BotApi.Services.MessageBroker.RabbitClient
{
    public class RabbitClientSub : RabbitClientBase, IMessageBrokerSub
    {
        private bool subscribed;

        public event EventHandler<NewTaskEventArgs> handler;

        public RabbitClientSub(string hostName, string userName, string password) : base(hostName, userName, password)
        {
        }

        public void Subscribe()
        {
            if (subscribed) return;

            var conn = GetConnection();

            var channel = conn.CreateModel();

            var consumer = new EventingBasicConsumer(channel);

            consumer.Received += Consumer_Received;

            channel.BasicConsume("dev-queue", true, consumer);

            subscribed = true;
        }

        private void Consumer_Received(object? sender, BasicDeliverEventArgs e)
        {
            long taskId = Convert.ToInt64(Encoding.UTF8.GetString(e.Body.ToArray()));
            handler.Invoke(this, new NewTaskEventArgs(taskId));
        }
    }
}
