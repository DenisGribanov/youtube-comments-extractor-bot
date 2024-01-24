using CommentsExtractor;
using System.Data;

namespace BotApi.Services
{
    public class ExtractorAdapter
    {
        private readonly IYouTube youTubeProxy;
        public DataTable result { get; private set; }

        private string videoId;

        private string ExcelFileSaveFolder => Variables.GetInstance().SAVE_RESULT_FOLDER;

        public ExtractorAdapter(IYouTube youTubeProxy)
        {
            this.youTubeProxy = youTubeProxy;
        }

        public async Task Run(string videoId)
        {
            this.videoId = videoId;

            YouTubeCommentsExtract extract = new YouTubeCommentsExtract(youTubeProxy, videoId);

            result = await extract.Extract();
        }

        public FileStream GetExcelFile()
        {
            if (result == null) return null;

            IExcelSave excelSave = new ExcelSave(result);

            if (string.IsNullOrWhiteSpace(ExcelFileSaveFolder))
            {
                return excelSave.Save($"{videoId}.xlsx");
            }
            else
            {
                if (!System.IO.Directory.Exists(ExcelFileSaveFolder))
                {
                    System.IO.Directory.CreateDirectory(ExcelFileSaveFolder);
                }

                return excelSave.Save(Path.Combine(ExcelFileSaveFolder, $"{videoId}.xlsx"));

            }
        }

        public List<string> GetBlockedApiKeys()
        {
            return this.youTubeProxy.GetBlockedApiKeys();
        }
    }
}
