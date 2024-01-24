using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommentsExtractor
{
    public class ApiKeyWasBannedEventArgs : EventArgs
    {
        public string ApiKey { get; set; }

        public ApiKeyWasBannedEventArgs(string apiKey)
        {
            ApiKey = apiKey;
        }
    }
}
