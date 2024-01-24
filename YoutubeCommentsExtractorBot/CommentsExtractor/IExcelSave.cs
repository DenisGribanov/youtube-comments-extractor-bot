using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommentsExtractor
{
    public interface IExcelSave
    {
        public FileStream Save(string filename);
    }
}
