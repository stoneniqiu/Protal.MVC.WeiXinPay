using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Websoket.Server
{
   public class ClientInfo
    {
       public ClientInfo()
       {
           IsValid = false;
           UserGuid = "";
       }
       public string UserGuid { get; set; }
       public string UserName { get; set; }
       public DateTime ValidTime { get; set; }
       public bool IsValid { get; set; }
       public string Token { get; set; }
    }
}
