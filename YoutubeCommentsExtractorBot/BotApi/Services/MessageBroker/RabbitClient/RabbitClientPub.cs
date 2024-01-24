using RabbitMQ.Client;
using System.Text;

namespace BotApi.Services.MessageBroker.RabbitClient
{
    public class RabbitClientPub : RabbitClientBase, IMessageBrokerPub
    {
        public RabbitClientPub(string hostName, string userName, string password) : base(hostName, userName, password)
        {
        }

        public void Publishing(long taskId)
        {
            var conn = GetConnection();

            var ch = conn.CreateModel();

            var body = Encoding.UTF8.GetBytes(taskId.ToString());

            ch.BasicPublish("", "dev-queue", null, body);

            conn.Close();
        }
    }
}
