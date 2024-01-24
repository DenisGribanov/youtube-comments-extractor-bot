namespace BotApi
{
    public class Variables
    {
        private static Variables instance;

        public long BOT_OWNER_CHAT_ID { get; private set; }
        public string BOT_DOMAIN_NAME { get; private set; }

        public string CACHE_FOLDER { get; private set; }

        public string SAVE_RESULT_FOLDER { get; private set; }

        public int MAX_COMMENTS { get; private set; }

        private Variables()
        {

        }

        private void Init(IConfiguration configuration)
        {
            BOT_OWNER_CHAT_ID = Convert.ToInt64(configuration["BOT_OWNER_CHAT_ID"]);
            BOT_DOMAIN_NAME = configuration["BOT_DOMAIN_NAME"];
            CACHE_FOLDER = configuration["CACHE_FOLDER"];
            SAVE_RESULT_FOLDER = configuration["SAVE_RESULT_FOLDER"];
            MAX_COMMENTS = Convert.ToInt32(configuration["MAX_COMMENTS"]);
        }

        public static Variables InitInstance(IConfiguration configuration)
        {
            if (instance == null)
            {
                instance = new Variables();
                instance.Init(configuration);
            }

            return instance;
        }

        public static Variables GetInstance()
        {
            return instance;
        }
    }
}
