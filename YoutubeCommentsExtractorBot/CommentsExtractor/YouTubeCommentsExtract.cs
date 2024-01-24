using Google.Apis.YouTube.v3.Data;
using NLog;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace CommentsExtractor
{
    public class YouTubeCommentsExtract
    {
        private readonly Logger logger = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// id комента = > кол-во реплаев
        /// </summary>
        private readonly Dictionary<string, long> СommentsWithReplays = new Dictionary<string, long>();
        private readonly string VideoId;
        private readonly IYouTube youTube;

        private IEnumerable<CommentThread> Threads = new List<CommentThread>();
        private IEnumerable<Comment> Replays = new List<Comment>();
        private readonly DataTable DtResult = new DataTable();
        public bool DataTableFilled => DtResult != null && DtResult.Rows.Count > 0;

        public YouTubeCommentsExtract(IYouTube youTube, string videoId)
        {
            VideoId = videoId;
            this.youTube = youTube;

            CreateColumnsDataTable();
        }

        public async Task<DataTable> Extract()
        {
            await ExtractVideoCommentThread();

            await ExtractReplays();

            logger.Info($"Threads count =  {Threads.Count()} | Replays count {Replays.Count()}");

            return FillDataTable();
        }

        private DataTable FillDataTable()
        {
            if (DataTableFilled) return DtResult;

            var tmpDict = new Dictionary<string, IEnumerable<Comment>>();
            foreach (var group in Replays.GroupBy(x => x.Snippet.ParentId))
            {
                tmpDict.Add(group.Key, group.ToList());
            }

            foreach (var thread in Threads.OrderBy(x => x.Snippet.TopLevelComment.Snippet.PublishedAt))
            {
                string channelIdCommentAuthor = thread.Snippet.TopLevelComment.Snippet.AuthorChannelId?.Value;
                var newRow = DtResult.NewRow();

                newRow[0] = thread.Id;
                newRow[1] = thread.Snippet.TopLevelComment.Snippet.PublishedAt.Value.ToUniversalTime().ToString("dd.MM.yyyy HH:mm:ss");
                newRow[2] = thread.Snippet.TopLevelComment.Snippet.UpdatedAt.Value.ToUniversalTime().ToString("dd.MM.yyyy HH:mm:ss");
                newRow[3] = thread.Snippet.TopLevelComment.Snippet.LikeCount;
                newRow[4] = thread.Snippet.TotalReplyCount;
                newRow[5] = thread.Snippet.TopLevelComment.Snippet.TextDisplay;
                newRow[6] = thread.Snippet.TopLevelComment.Snippet.TextOriginal;
                newRow[7] = thread.Snippet.TopLevelComment.Snippet.AuthorDisplayName;
                newRow[8] = channelIdCommentAuthor;
                newRow[9] = string.Empty;
                newRow[10] = false;
                DtResult.Rows.Add(newRow);


                if (thread.Snippet.TotalReplyCount == null || thread.Snippet.TotalReplyCount == 0 || !tmpDict.ContainsKey(thread.Id))
                    continue;

                var replays = tmpDict[thread.Id];
                foreach (var replay in replays)
                {
                    var newRow2 = DtResult.NewRow();
                    channelIdCommentAuthor = replay.Snippet.AuthorChannelId?.Value;

                    newRow2[0] = replay.Id;
                    newRow2[1] = replay.Snippet.PublishedAt.Value.ToUniversalTime().ToString("dd.MM.yyyy HH:mm:ss");
                    newRow2[2] = replay.Snippet.UpdatedAt.Value.ToUniversalTime().ToString("dd.MM.yyyy HH:mm:ss");
                    newRow2[3] = replay.Snippet.LikeCount;
                    newRow2[4] = 0;
                    newRow2[5] = replay.Snippet.TextDisplay;
                    newRow2[6] = replay.Snippet.TextOriginal;
                    newRow2[7] = replay.Snippet.AuthorDisplayName;
                    newRow2[8] = channelIdCommentAuthor;
                    newRow2[9] = replay.Snippet.ParentId;
                    newRow2[10] = true;
                    DtResult.Rows.Add(newRow2);
                }
            }


            logger.Info($"FillDataTable success. Rows {DtResult.Rows.Count}");

            return DtResult;
        }

        private void CreateColumnsDataTable()
        {
            DtResult.Columns.Add("CommentId");
            DtResult.Columns.Add("PublishedAtUTC");
            DtResult.Columns.Add("UpdatedAtUTC");
            DtResult.Columns.Add("LikeCount", typeof(long));
            DtResult.Columns.Add("TotalReplyCount", typeof(long));
            DtResult.Columns.Add("TextDisplay");
            DtResult.Columns.Add("TextOriginal");
            DtResult.Columns.Add("AuthorDisplayName");
            DtResult.Columns.Add("AuthorChannelId");
            DtResult.Columns.Add("CommentParent");
            DtResult.Columns.Add("IsReply", typeof(bool));
        }

        /// <summary>
        /// Выгрузить все комментарии без ответов на них
        /// </summary>
        private async Task ExtractVideoCommentThread()
        {
            CommentThreadListResponse response;
            string nextPageToken = null;

            do
            {
                response = await youTube.CallRequestCommentThread(VideoId, nextPageToken);

                AddCommentWithReplay(response?.Items);

                PutThreads(response?.Items);

                nextPageToken = response?.NextPageToken;


            } while (!string.IsNullOrEmpty(nextPageToken));

            logger.Info("ExtractVideoCommentThread success");
        }

        private void PutThreads(IList<CommentThread>? items)
        {
            if (items == null || items.Count == 0) return;

            if (Threads.Count() == 0)
            {
                Threads = items;
                return;
            }

            Threads = Threads.Concat(items);
        }

        private void AddCommentWithReplay(IEnumerable<CommentThread> comments)
        {
            if (comments == null)
                return;

            foreach (var comment in comments.Where(x => x.Snippet.TotalReplyCount != null && x.Snippet.TotalReplyCount > 0).ToList())
            {
                СommentsWithReplays.TryAdd(comment.Id, comment.Snippet.TotalReplyCount.Value);
            }
        }

        /// <summary>
        /// выгрузка ответов на комментарии
        /// </summary>
        private async Task ExtractReplays()
        {
            const int SPLIT_SIZE = 5;
            const int MAX_REPLAYS = 50;


            //все комменты у которых больше 50 ответов (что бы такие вытащить потребуется несколько запросов, т.к максимум в одном ответе будет 50 штук),
            //вытаскиваем в цикле, не распараллеливая дабы не схаватить 
            //ограничения кол-во запросов в секунду
            foreach (var comment_id in СommentsWithReplays.Where(x => x.Value > MAX_REPLAYS).Select(x => x.Key))
            {
                await GetTheadsByComment(comment_id);
            }

            //здесь распарралелим, на 5 комментов сразу полетит запрос на выгрузку к ним всех ответ (они прилетят одним ответом на каждый коммент. т.к меньше 50 штук)
            foreach (var list in СommentsWithReplays.Where(x => x.Value > 0 && x.Value <= MAX_REPLAYS).Select(x => x.Key).ToArray().Split(SPLIT_SIZE))
            {
                Parallel.ForEach(list, async comment_id =>
                {
                    await GetTheadsByComment(comment_id);
                });
            }

            logger.Info("ExtractReplays success");

        }

        private async Task GetTheadsByComment(string comment_id)
        {
            CommentListResponse response;
            string nextPageToken = null;

            do
            {
                response = await youTube.CallRequestTheadsByComment(comment_id, nextPageToken);
                nextPageToken = response.NextPageToken;
                PutReplays(response.Items);

            } while (!string.IsNullOrEmpty(nextPageToken));

        }

        private void PutReplays(IList<Comment> items)
        {
            if (items == null || items.Count == 0) return;

            if (Replays.Count() == 0)
            {
                Replays = items;
                return;
            }

            Replays = Replays.Concat(items);
        }

        public static string GetVideoIdFromUrl(string url)
        {
            if (url == null) throw new ArgumentNullException("url");

            const string pattern = @"^.*((youtu.be\/)|(v\/)|(\/u\/\w\/)|(embed\/)|(watch\?))\??v?=?([^#&?]*).*";

            var matches = Regex.Matches(url, pattern);

            if (matches.Count > 0 && matches[0].Groups.Count > 7 && matches[0].Groups[7].Value.Length == 11)
            {
                return matches[0].Groups[7].Value;
            }

            return null;
        }
    }
}
