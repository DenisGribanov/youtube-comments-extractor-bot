using Google.Apis.YouTube.v3.Data;
using Newtonsoft.Json;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace CommentsExtractor
{
    public class YouTubeProxyClient : IYouTube
    {
        private readonly Logger logger = LogManager.GetCurrentClassLogger();
        private readonly IYouTube youTubeClient;
        private const string CACHE_FILE_POSTFIX = "cache";
        private readonly string CacheDic;

        public YouTubeProxyClient(IYouTube youTube, string CacheDic)
        {
            this.youTubeClient = youTube;
            this.CacheDic = CacheDic;
        }

        public List<string> GetBlockedApiKeys()
        {
            return youTubeClient.GetBlockedApiKeys();
        }

        public async Task<Video> GetVideo(string videoId)
        {
            if(string.IsNullOrEmpty(videoId)) throw new ArgumentNullException(nameof(videoId));

            var video = await youTubeClient.GetVideo(videoId);

            logger.Info($"GetVideo {videoId} | title  {video.Snippet.Title} | channel {video.Snippet.ChannelTitle}");

            return video;
        }

        public async Task<CommentListResponse> CallRequestTheadsByComment(string comment_id, string? nextPageToken = null)
        {
            var responseFromCache = await ReadCache<CommentListResponse>(getArgs());

            if(responseFromCache != null)
            {
                logger.Info($"CallRequestTheadsByComment read from cache | comment_id = {comment_id} | nextPageToken = {nextPageToken} | count = {responseFromCache.Items.Count}");
                return responseFromCache;
            }

            var response = await youTubeClient.CallRequestTheadsByComment(comment_id, nextPageToken);

            WriteCache(typeof(CommentListResponse).Name, getArgs(), response);

            return response;

            string getArgs()
            {
                return $"ParentId={comment_id}&PageToken={nextPageToken}";
            }
        }

        public async Task<CommentThreadListResponse> CallRequestCommentThread(string videoId, string? nextPageToken = null)
        {
            var responseFromCache = await ReadCache<CommentThreadListResponse>(getArgs());

            if (responseFromCache != null)
            {
                logger.Info($"CallRequestCommentThread read from cache | videoId = {videoId} | nextPageToken = {nextPageToken} | count = {responseFromCache.Items.Count}");
                return responseFromCache;
            }


            var response = await youTubeClient.CallRequestCommentThread(videoId, nextPageToken);

            WriteCache(typeof(CommentThreadListResponse).Name, getArgs(), response);

            return response;

            string getArgs()
            {
                return $"PageToken={nextPageToken}";
            }
        }

        private async Task<T> ReadCache<T>(string args)
        {
            if (string.IsNullOrEmpty(args))
            {
                return default;
            }

            string filename = $"{GetHash(args)}.{CACHE_FILE_POSTFIX}";

            string file_path = Path.Combine(CacheDic, typeof(T).Name, filename);

            if (!File.Exists(file_path))
            {
                return default;
            }

            try
            {
                string data = await File.ReadAllTextAsync(file_path);
                return JsonConvert.DeserializeObject<T>(data);
            }
            catch (Exception e)
            {
                logger.Warn(e);
                return default;
            }
        }

        private async void WriteCache<T>(string kind, string args, T data)
        {
            string pathCacheFolder = Path.Combine(CacheDic, kind);

            string filename = $"{GetHash(args)}.{CACHE_FILE_POSTFIX}";


            if (!Directory.Exists(pathCacheFolder))
                Directory.CreateDirectory(pathCacheFolder);

            if (File.Exists(Path.Combine(pathCacheFolder, filename)))
                return;

            await File.WriteAllTextAsync(Path.Combine(pathCacheFolder, filename), JsonConvert.SerializeObject(data));
        }

        private static string GetHash(string input)
        {
            using (SHA256 hashAlgorithm = SHA256.Create())
            {
                // Convert the input string to a byte array and compute the hash.
                byte[] data = hashAlgorithm.ComputeHash(Encoding.UTF8.GetBytes(input));

                // Create a new Stringbuilder to collect the bytes
                // and create a string.
                var sBuilder = new StringBuilder();

                // Loop through each byte of the hashed data
                // and format each one as a hexadecimal string.
                for (int i = 0; i < data.Length; i++)
                {
                    sBuilder.Append(data[i].ToString("x2"));
                }

                // Return the hexadecimal string.
                return sBuilder.ToString();
            }
        }

    }
}
