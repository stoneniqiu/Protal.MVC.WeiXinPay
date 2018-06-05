using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Niqiu.Core.Domain.Common
{
   public class Result
    {
       public Result(string code="001",string msg="")
       {
           Date = DateTime.Now;
           Code = code;
           if (msg == "")
           {
               switch (code)
               {
                   case "000":
                       msg = "参数为空";
                       break;
                   case"001":
                       msg = "操作成功";
                       break;
                   case "002":
                       msg = "参数有误";
                       break;
                   case"003":
                       msg = "认证失败";
                       break;
                   case "004":
                       msg = "操作失败";
                       break;
                   case "005":
                       msg = "文件已存在";
                       break;
                   case "006":
                       msg = "token非法";
                       break;
                   case "007":
                       msg = "token过期";
                       break;
                   case "008":
                       msg = "服务器错误";
                       break;
               }
           }
           Message = msg;
       }
       /// <summary>
       /// 000 参数为空
       /// 001 操作成功
       /// 003 认证失败
       /// 004 操作失败
       /// 005 文件存在
       /// </summary>
       public string Code { get; set; }
       public string Message { get; set; }
       public object Data { get; set; }
       public DateTime Date { get; set; }
       public int Count { get; set; }
    }
}
