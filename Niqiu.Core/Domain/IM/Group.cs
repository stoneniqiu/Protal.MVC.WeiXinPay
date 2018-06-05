using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Niqiu.Core.Helpers;

namespace Niqiu.Core.Domain.IM
{
    /// <summary>
    /// 群组 必须有个群主
    /// </summary>
   public class Group
    {
        private ICollection<User.User> _users;

        public Group()
        {
            Id = Encrypt.GenerateOrderNumber();
            CreateTime=DateTime.Now;
            ModifyTime=DateTime.Now;
        }

        [Key]
        public string Id { get; set; }
        public DateTime CreateTime { get; set; }
        public DateTime ModifyTime { get; set; }
 
       public string GroupName { get; set; }
       public string Image { get; set; }

       [Required]
       //群主
       public int CreateUserId { get; set; }
   
        [NotMapped]
        public virtual User.User Owner { get; set; }

        public ICollection<User.User> Users
        {
            get { return _users??(_users=new List<User.User>()); }
            set { _users = value; }
        }

        public string Description { get; set; }
       public bool IsDeleteD { get; set; }
    }

}
