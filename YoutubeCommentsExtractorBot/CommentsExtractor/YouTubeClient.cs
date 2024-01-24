using Google.Apis.YouTube.v3;
using Google.Apis.YouTube.v3.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommentsExtractor
{
    public class YouTubeClient : YouTubeBase, IYouTube
    {
        private const string SEARCH_PART_STRING = "id,snippet";
        private const string VIDEOS_PART_STRING = "statistics,contentDetails, snippet";
        private List<string> blockedApiKeys = new List<string>();

        public YouTubeClient(List<string> apiKeys) : base(apiKeys)
        {
            this.ApiKeyBannedNotify += YouTubeClient_ApiKeyBannedNotify;
        }

        private void YouTubeClient_ApiKeyBannedNotify(YouTubeBase sender, ApiKeyWasBannedEventArgs e)
        {
            blockedApiKeys.Add(sender.CurrentApiKey);
        }

        public List<string> GetBlockedApiKeys()
        {
            return blockedApiKeys;
        }

        public async Task<Video> GetVideo(string videoId)
        {
            if (string.IsNullOrEmpty(videoId)) throw new ArgumentNullException(nameof(videoId));

            var response = await CallRequest(GetRequestVideoList, videoId);

            return response?.Items?.FirstOrDefault();
        }


        public async Task<CommentThreadListResponse> CallRequestCommentThread(string videoId, string? nextPageToken = null)
        {
            var result = await CallRequest(GetRequestCommentThreadList, videoId, nextPageToken);
            return result;
        }

        public async Task<CommentListResponse> CallRequestTheadsByComment(string comment_id, string? nextPageToken = null)
        {
            var result = await CallRequest(GetRequestTheadsByComment, comment_id, nextPageToken);
            return result;
        }

        private YouTubeBaseServiceRequest<CommentListResponse> GetRequestTheadsByComment(string id, string? nextPageToken = null)
        {
            var ytRequest = GetYouTubeService().Comments.List(SEARCH_PART_STRING);
            ytRequest.ParentId = id;
            ytRequest.MaxResults = MAX_RESULT;
            ytRequest.PageToken = nextPageToken;
            return ytRequest;
        }

        private YouTubeBaseServiceRequest<CommentThreadListResponse> GetRequestCommentThreadList(string video_id, string? nextPageToken = null)
        {
            var ytRequest = GetYouTubeService().CommentThreads.List(SEARCH_PART_STRING);
            ytRequest.VideoId = video_id;
            ytRequest.MaxResults = MAX_RESULT;
            ytRequest.PageToken = nextPageToken;
            ytRequest.Order = CommentThreadsResource.ListRequest.OrderEnum.Time;
            return ytRequest;
        }

        private YouTubeBaseServiceRequest<VideoListResponse> GetRequestVideoList(string video_id_list_str, string nextPageToken)
        {
            var request = GetYouTubeService().Videos.List(VIDEOS_PART_STRING);
            request.Id = video_id_list_str.Split(',');
            request.MaxResults = MAX_RESULT;
            request.PageToken = nextPageToken;
            return request;
        }

    }
}
