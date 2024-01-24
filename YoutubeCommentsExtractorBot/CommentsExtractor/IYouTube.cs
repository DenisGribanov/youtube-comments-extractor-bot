using Google.Apis.YouTube.v3.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommentsExtractor
{
    public interface IYouTube
    {
        Task<Video> GetVideo(string videoId);
        Task<CommentListResponse> CallRequestTheadsByComment(string comment_id, string? nextPageToken = null);

        Task<CommentThreadListResponse> CallRequestCommentThread(string videoId, string? nextPageToken = null);

        List<string> GetBlockedApiKeys();
    }
}
