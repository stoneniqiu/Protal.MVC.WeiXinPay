using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Niqiu.Core.Helpers
{
   public class ConfigHelper
    {
       private static double MaxSize = 2;//G
       private static int UploadNum = 10;

       public static double GetUploadMaxSize()
       {
           var maxSize = System.Configuration.ConfigurationManager.AppSettings["MaxUpload"];
           if (maxSize != null)
           {
               try
               {
                   var key = Convert.ToDouble(maxSize);
                   return key * 1024 * 1024*1024;
               }
               catch
               {
               }
           }
           return MaxSize * 1024 * 1024;
       }

       public static double GetUploadNum()
       {
           var maxSize = System.Configuration.ConfigurationManager.AppSettings["UploadNum"];
           if (maxSize != null)
           {
               try
               {
                   var key = Convert.ToInt16(maxSize);
                   return key;
                }
               catch
               {
               }
           }
           return UploadNum;
       }
    }
}
