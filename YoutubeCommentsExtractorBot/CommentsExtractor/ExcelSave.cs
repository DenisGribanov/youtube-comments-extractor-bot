using MiniExcelLibs;
using System.Data;

namespace CommentsExtractor
{
    public class ExcelSave : IExcelSave
    {
        private readonly DataTable DtResult;

        public ExcelSave(DataTable dtResult)
        {
            DtResult = dtResult;
        }

        public FileStream Save(string filename)
        {
            SaveFile(filename);

            if(!System.IO.File.Exists(filename)) 
            {
                throw new Exception("иксель файл не найден");
            }

            var fs =  new FileStream(filename, FileMode.Open);

            return fs;
        }

        private void SaveFile(string fileName)
        {
            MiniExcel.SaveAs(fileName, DtResult, overwriteFile: true);

        }
    }
}
