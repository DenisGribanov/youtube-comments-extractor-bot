namespace BotApi.Services.MessageBroker
{
    public class NewTaskEventArgs : EventArgs
    {
        public readonly long TaskId;

        public NewTaskEventArgs(long taskId)
        {
            TaskId = taskId;
        }
    }
}
