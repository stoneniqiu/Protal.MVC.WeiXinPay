using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Websoket.Server
{
    class JsonDto
    {
        public JsonDto()
        {
            //001 验证token
            //默认是正常对话
            //003 回执
            //004 图片
            type = "002";
            sendTime=DateTime.Now;
        }
        public string content { get; set; }
        //消息类型
        public string type { get; set; }
        public string sender { get; set; }
        public string toId { get; set; }
        public DateTime sendTime { get; set; }

    }
  
}
