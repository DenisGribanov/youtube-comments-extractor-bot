using System;
using System.Collections.Generic;

namespace BotApi.Database
{
    public partial class YoutubeApiKey
    {
        public int Id { get; set; }
        public string ApiKey { get; set; } = null!;
        public DateOnly DateAdd { get; set; }
        public DateOnly? UnblockingDate { get; set; }
        public string? Comments { get; set; }
        public bool? Deleted { get; set; }
    }
}
