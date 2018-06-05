using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Niqiu.Core.Domain.IM
{
    /// <summary>
    /// 联系人
    /// 被加入的群也算，这里存在一种转换
    /// </summary>
    public class Friend
    {
        public Friend(User.User user)
        {
            Id = user.UserGuid.ToString();
            Name = user.Username;
            Image = user.ImgUrl;
            CreateTime=DateTime.Now;
            IsGroup = false;
        }

        public Friend(Group group)
        {
            Id = group.Id;
            Name = group.GroupName;
            Image = group.Image;
            CreateTime=DateTime.Now;
            IsGroup = true;
            Users = new List<UserDto>();
            foreach (var user in group.Users)
            {
                Users.Add(new UserDto()
                {
                    Id = user.Id,
                    UserGuid = user.UserGuid.ToString(),
                    ImgUrl = user.ImgUrl,
                    Username = user.Username,
                });
            }
        }
        public string Id { get; set; }
        public string Name { get; set; }
        public string Image { get; set; }
        public bool IsGroup { get; set; }
        
        [NotMapped]
        public List<UserDto> Users { get; set; }
        public DateTime CreateTime { get; set; }
    }

    public class UserDto
    {
        public int Id { get; set; }
        public string UserGuid { get; set; }
        public string ImgUrl { get; set; }
        public string Username { get; set; }
    }
}
