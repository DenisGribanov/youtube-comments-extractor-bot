using System;
using System.Collections.Generic;

namespace BotApi.Database
{
    public partial class DownloadFromVideoTask
    {
        public int Id { get; set; }
        public Guid UidTask { get; set; }
        public string VideoUrl { get; set; } = null!;
        public long ChatId { get; set; }
        public DateTime CreateDate { get; set; }
        public DateTime? BeginDate { get; set; }
        public bool Completed { get; set; }
        public bool Failed { get; set; }
        public DateTime? CompleteDate { get; set; }
        public int? TotalCoast { get; set; }
        public int? TotalDownloaded { get; set; }
        public string? Description { get; set; }
        public string? ErrorText { get; set; }
        public string? VideoId { get; set; }
        public string? VideoTitle { get; set; }
        public string? ChannelTitle { get; set; }
        public int? TotalComments { get; set; }

        public virtual TgUser Chat { get; set; } = null!;
    }
}
