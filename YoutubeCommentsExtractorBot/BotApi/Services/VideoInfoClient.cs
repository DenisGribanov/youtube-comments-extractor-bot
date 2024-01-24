using BotApi.Dto;
using CommentsExtractor;
using Google.Apis.YouTube.v3.Data;

namespace BotApi.Services
{
    public class VideoInfoClient
    {
        private readonly IYouTube youTubeClient;

        public VideoInfoClient(IYouTube youTubeClient)
        {
            this.youTubeClient = youTubeClient;
        }

        public async Task<VideoInfoDto> Get(string videoId)
        {
            if (videoId == null) throw new ArgumentNullException(nameof(videoId));

            var video = await youTubeClient.GetVideo(videoId);

            return new VideoInfoDto
            {
                ChannelTitle = video.Snippet.ChannelTitle,
                VideoTitle = video.Snippet.Title,
                VideoId = videoId,
                CommentCount = video.Statistics.CommentCount
            };
        }

        public static VideoInfoClient Init (List<string> apiKeys)
        {
            return new VideoInfoClient(new YouTubeClient(apiKeys));
        }
    }
}
