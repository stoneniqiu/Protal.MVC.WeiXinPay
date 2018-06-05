namespace Portal.MVC5.Models
{
    public class TemplateModel2
    {
          public string touser { get; set; }
        public string template_id { get; set; }
        public string url { get; set; }

        public string topcolor { get; set; }

        public TemplateData2 data { get; set; } 
    
        public TemplateModel2(string all,string type,string username,string time,string content,string last)
        {
            data=new TemplateData2()
            {
                first = new TempItem(all),
                keyword1 = new TempItem(type),
                keyword2 = new TempItem(username),
                keyword3 = new TempItem(time),
                keyword4 = new TempItem(content),
                remark = new TempItem(last)
            };
            template_id = "EdO4A5HIOCMEmx4CqAON83dQ6Xv0OVsXC5RspBNF0ZI";
        }
    }

    public class TemplateData2
    {
        public TempItem first { get; set; }
        public TempItem keyword1 { get; set; }
        public TempItem keyword2 { get; set; }
        public TempItem keyword3 { get; set; }
        public TempItem keyword4 { get; set; }
        public TempItem remark { get; set; }
    }
}