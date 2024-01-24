using System;
using System.Collections.Generic;

namespace BotApi.Database
{
    public partial class TgUser
    {
        public TgUser()
        {
            DownloadFromVideoTasks = new HashSet<DownloadFromVideoTask>();
        }

        public long ChatId { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? UserName { get; set; }
        public string? LanguageCode { get; set; }
        public DateTime CreateData { get; set; }

        public virtual ICollection<DownloadFromVideoTask> DownloadFromVideoTasks { get; set; }
    }
}
