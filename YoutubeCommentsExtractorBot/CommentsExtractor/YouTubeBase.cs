using Google.Apis.Services;
using Google.Apis.YouTube.v3;
using NLog;

namespace CommentsExtractor
{
    public abstract class YouTubeBase
    {
        protected static readonly Logger logger = LogManager.GetCurrentClassLogger();
        
        private const string APP_NAME = "My app";
        private const string QUOTA_ERROR = "quota";
        protected const int MAX_RESULT = 100;

        public string CurrentApiKey { get; private set; }
        private readonly List<string> API_KEYS;
        
        protected delegate void ApiKeyWasBannedHandler(YouTubeBase sender, ApiKeyWasBannedEventArgs e);
        protected event ApiKeyWasBannedHandler ApiKeyBannedNotify;

        public YouTubeBase(List<string> apiKeys)
        {
            if (apiKeys == null || apiKeys.Count == 0)
            {
                throw new ArgumentNullException($"{nameof(apiKeys)} is null or is empty");
            }

            this.API_KEYS = apiKeys;
            CurrentApiKey = API_KEYS.ToArray().GetRandElement();
        }

        protected YouTubeService GetYouTubeService()
        {
            if (string.IsNullOrEmpty(CurrentApiKey))
            {
                throw new Exception("ApiKey не указан !");
            }

            return new YouTubeService(new BaseClientService.Initializer
            {
                ApplicationName = APP_NAME,
                ApiKey = CurrentApiKey
            });
        }


        protected async Task<T?> CallRequest<T>(Func<string, string, YouTubeBaseServiceRequest<T>> func, string id, string? nextPageToken = null)
        {
            YouTubeBaseServiceRequest<T>? req = default;
            T? resp = default;

            try
            {
                req = func.Invoke(id, nextPageToken);

                logger.Info($"{req.HttpMethod} |{req.RestPath} | {req.MethodName} | id {id} | nextPageToken {nextPageToken}");

                resp = req.Execute();

                return resp;
            }
            catch (Google.GoogleApiException ex)
            {
                if (ex.Message.Contains(QUOTA_ERROR) && ex.HttpStatusCode == System.Net.HttpStatusCode.Forbidden ||
                    ex.HttpStatusCode == System.Net.HttpStatusCode.BadRequest && ex.Message.Contains("API key not valid"))
                {
                    if (ex.Message.Contains(QUOTA_ERROR))
                    {
                        logger.Warn($"{req.RestPath}| id {id} | Quota limit, API-key Replace");
                    }

                    ApiKeyBannedNotify?.Invoke(this, new ApiKeyWasBannedEventArgs(CurrentApiKey));

                    ReplaceCurrentApiKey();
                    
                    req = func.Invoke(id, nextPageToken);
                    
                    logger.Info($"{req.HttpMethod} |{req.RestPath} | {req.MethodName}");
                    
                    return await req.ExecuteAsync();
                }
                else if (ex.Message.Contains("NotFound") || ex.Message.Contains("disabled comments"))
                {
                    logger.Warn($"{req.RestPath}| id {id} | {ex.Message}");
                    return default;
                }
                else
                {
                    logger.Warn($"{req.RestPath}| id {id} | {ex.Message}");
                    return default;
                }
            }
            catch
            {
                throw;
            }

        }

        private string ReplaceCurrentApiKey()
        {
            if (CurrentApiKey == null)
            {
                CurrentApiKey = API_KEYS.ToArray().GetRandElement();
                return CurrentApiKey;
            }

            string oldApi = CurrentApiKey;

            API_KEYS.Remove(CurrentApiKey);

            CurrentApiKey = API_KEYS.ToArray().GetRandElement();

            logger.Warn($"Replace Api Key | old {oldApi} | new {CurrentApiKey}");

            return CurrentApiKey;
        }

    }
}
