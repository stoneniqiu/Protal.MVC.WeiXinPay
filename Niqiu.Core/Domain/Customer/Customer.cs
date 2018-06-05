using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Niqiu.Core.Domain.Customer
{
  public  class Customer:BaseEntity
    {
      public string Name { get; set; }
      public string Mobile { get; set; }
      public string Ip { get; set; }

      public string Title { get; set; }
      public string Url { get; set; }
      public string Referrer { get; set; }
      public string SearchEngine { get; set; }
      public string Keyword { get; set; }

    }

    public class ClientDto
    {
        public string Name { get; set; }
        public string phone { get; set; }
        public string Url { get; set; }
        public string sourceUrl { get; set; }
        public string keyword { get; set; }
        public string engin { get; set; }

        public string title { get; set; }
        
    }
}
