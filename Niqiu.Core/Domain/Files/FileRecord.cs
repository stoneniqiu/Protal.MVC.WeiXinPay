using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Niqiu.Core.Domain.Files
{
   public class FileRecord:BaseEntity
    {
       public FileRecord()
       {
           GuId = Guid.NewGuid().ToString();
       }
       public string RawName { get; set; }
       public string SavePath { get; set; }
       public string WebPath { get; set; }
       public int Size { get; set; }
       public string MD5 { get; set; }
       public string UserGuid{ get; set; }
       public int DownloadTimes { get; set; }
       public int VisitTimes { get; set; }
       public string Ip { get; set; }

       public string GuId { get; set; }

    }
}
