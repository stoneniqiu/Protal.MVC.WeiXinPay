namespace Portal.MVC5.Models
{
    /// <summary>
    /// 发送消息
    /// </summary>
    public class MessageDto
    {
        public string sender { get; set; }
        public string msg { get; set; }
        public string toId { get; set; }
    }
    /// <summary>
    /// 用于处理消息体
    /// </summary>
    public class MsgDto
    {
        public string userId { get; set; }
        public string msgId { get; set; }

        public string senderId { get; set; }
        
    }
}