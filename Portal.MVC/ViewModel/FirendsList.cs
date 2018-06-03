using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Niqiu.Core.Domain.User;
using Niqiu.Core.Helpers;

namespace Portal.MVC.ViewModel
{
    public class FirendsList
    {
        private List<Firend> _firends;
        public string Title { get; set; }

        public List<Firend> Firends
        {
            get { return _firends??(_firends=new List<Firend>()); }
            set { _firends = value; }
        }
    }

    public class FirendsListGroup
    {
        private Dictionary<string, FirendsList> _groups;

        public FirendsListGroup(IEnumerable<Firend> items)
        {
            if(items==null||!items.Any()) return;

            foreach (var firend in items)
            {
                if(string.IsNullOrWhiteSpace(firend.FirendName)) continue;
                var key = CommonHelper.UtilIndexCode(firend.FirendName).Substring(0,1).ToUpper();
                if (Groups.ContainsKey(key))
                {
                    Groups[key].Firends.Add(firend);
                }
                else
                {
                    var fl = new FirendsList() {Title = key};
                    fl.Firends.Add(firend);
                    Groups.Add(key,fl);
                }
            }
            Groups = Groups.OrderBy(n => n.Key).ToDictionary(n => n.Key, n => n.Value);

        }

        public Dictionary<string, FirendsList> Groups
        {
            get { return _groups??(_groups=new Dictionary<string, FirendsList>()); }
            set { _groups = value; }
        }
    }
}