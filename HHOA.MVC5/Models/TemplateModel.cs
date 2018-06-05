namespace Portal.MVC5.Models
{
    public class TemplateModel
    {
        public string touser { get; set; }
        public string template_id { get; set; }
        public string url { get; set; }

        public string topcolor { get; set; }

        public TemplateData data { get; set; } 
    
        public TemplateModel(string hello,string state,string reason,string last)
        {
            data=new TemplateData()
            {
                first = new TempItem(hello),
                keyword1 = new TempItem(state),
                keyword2 = new TempItem(reason),
                remark = new TempItem(last)
            };
            template_id = "rXMjcyoq8aMKfmXTBN3X-KBTjfBTfq6bSuQTxOcbEPo";
        }
    }

    public class TemplateData
    {
        public TempItem first { get; set; }
        public TempItem keyword1 { get; set; }
        public TempItem keyword2 { get; set; }
        public TempItem remark { get; set; }
    }
    public class TempItem
    {
        public TempItem(string v,string c = "#173177")
        {
            value = v;
            color = c;
        }
        public string value { get; set; }
        public string color { get; set; }
    }
}