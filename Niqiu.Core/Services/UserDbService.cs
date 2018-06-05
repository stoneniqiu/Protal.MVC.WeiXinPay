using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Globalization;
using System.Linq;
using Niqiu.Core;
using Niqiu.Core.Domain;
using Niqiu.Core.Domain.Common;
using Niqiu.Core.Domain.IM;
using Niqiu.Core.Domain.User;
using Niqiu.Core.Helpers;

namespace Niqiu.Core.Services
{
    public class UserDbService
    {
        private int ExpireTime = 2 * 60;//min
        private PortalDb _db = new PortalDb();
        public void InsertUser(User model)
        {

            if (model != null)
            {
                using (var db=new PortalDb())
                {
                    db.Users.Add(model);
                    db.SaveChanges();
                }
            }

        }

        public User GetUserByOpenId(string openId)
        {
            return string.IsNullOrWhiteSpace(openId) ? null : _db.Users.FirstOrDefault(n => n.OpenId == openId);
        }
        public User GetUserBySystemName(string systemName)
        {
            return string.IsNullOrWhiteSpace(systemName) ? null : _db.Users.FirstOrDefault(n => n.SystemName == systemName);
        }

        public IPagedList<User> GetAllUsers(string username = null, string email = "", int pageIndex = 0, int pageSize = 2147483647)
        {
            var query =_db.Users.Where(n => !n.Deleted);
            if (!String.IsNullOrWhiteSpace(email))
                query = query.Where(c => c.Email.Contains(email));
            if (!String.IsNullOrWhiteSpace(username))
                query = query.Where(c => c.Username.Contains(username));//c.RealName.Contains(username)||
            query = query.OrderByDescending(c => c.Id);
            var users = new PagedList<User>(query, pageIndex, pageSize);
            return users;
        }
        public User GetUserByGuid(Guid userGuid)
        {
            return userGuid == Guid.Empty ? null :
              _db.Users.FirstOrDefault(n => n.UserGuid == userGuid);
        }
        public User GetUserByGuid(string userGuid)
        {
            var guid = Guid.Parse(userGuid);
            return userGuid == string.Empty ? null :
              _db.Users.FirstOrDefault(n => n.UserGuid == guid);
        }
        public User GetUserById(int userId)
        {
            if (userId == 0)
                return null;
            return _db.Users.Find(userId);
        }
        public void UpdateUser(User user)
        {
            if (user == null)
                throw new ArgumentNullException("user");
            //_useRepository.Update(user);
            using (var db=new PortalDb())
            {
                var raw = db.Users.Find(user.Id);
                if (raw != null)
                {
                    raw.UserGuid = user.UserGuid;
                    raw.Gender = user.Gender;
                    raw.Username = user.Username;
                    raw.Email = user.Email;
                    raw.Mobile = user.Mobile;
                    raw.Password = user.Password;
                    raw.RealName = user.RealName;
                    raw.Description = user.Description;
                    raw.ImgUrl = user.ImgUrl;
                    raw.Address = user.Address;
                    raw.Active = user.Active;
                    raw.Deleted = user.Deleted;
                    raw.IsSystemAccount = user.IsSystemAccount;
                    raw.SystemName = user.SystemName;
                    raw.LastIpAddress = user.LastIpAddress;
                    raw.LastLoginDateUtc = user.LastLoginDateUtc;
                    raw.LastActivityDateUtc = user.LastActivityDateUtc;
                    raw.OpenId = user.OpenId;
                    raw.Province = user.Province;
                    raw.Country = user.Country;
                    raw.City = user.City;
                    db.SaveChanges();
                }
            }

            //还触发了事件通知！
            //_eventPublisher.EntityUpdated(customer);
        }
        public User GetUserByUsername(string username)
        {
            return string.IsNullOrWhiteSpace(username) ? null : _db.Users.FirstOrDefault(n => n.Username == username);
        }
        public User GetUserByMobile(string mobile)
        {
            return string.IsNullOrWhiteSpace(mobile) ? null : _db.Users.FirstOrDefault(n => n.Mobile == mobile);
        }

        public Result validToken(string token)
        {
            if (string.IsNullOrEmpty(token)) return new Result("006");
            try
            {
                var raw = Encrypt.MD5Decrypt(token);
                var strs = raw.Split(',');
                //检查时间是否过期
                var tokenTime = Convert.ToDateTime(strs[2]);
                if (DateTime.Now > tokenTime.AddMinutes(ExpireTime))
                {
                    return new Result("007");
                }
                //检查用户是否存在
                var user = GetUserByGuid(strs[0]);
                if (user == null || user.UserGuid.ToString() != strs[0]) return new Result("006");
                strs[1] = user.Username;//对名称进行修正，现有加密导致中文乱码
                return new Result() { Message = "验证成功", Data = strs };
            }
            catch
            {
                return new Result("008");
            }
      
        }

        /// <summary>
        /// 默认是已经验证过的 
        /// 配合ValidTokenAttribute
        /// </summary>
        /// <returns></returns>
        public string[] DecryptToken(string token="")
        {
            if(string.IsNullOrEmpty(token))
            token = GetToken();
            var raw = Encrypt.MD5Decrypt(token);
            var strs = raw.Split(',');
            return strs;
        }

        public string CreateToken(User user)
        {
            //名称里面不能出现特殊符号
            var key = user.UserGuid + "," + user.Username + "," + DateTime.Now.ToString(CultureInfo.InvariantCulture);
            var token = Encrypt.MD5Encrypt(key);
            return token;
        }

        public string GetToken()
        {
            //先从header中检测是否有token
            var request = System.Web.HttpContext.Current.Request;
            var token = request.Headers["Token"];
            //如果没有就从form中寻找
            if (string.IsNullOrEmpty(token))

                token = request.Form["token"];
            //还没有从参数中寻找
            if (string.IsNullOrEmpty(token))
            {
                //IDictionary<string, object> parmdatas = filterContext
                //token = parmdatas["token"].ToString();
            }
            return token;
        }

        public IEnumerable<User> GetUserTable()
        {
            return _db.Users;
        }

        /// <summary>
        /// 创建群，群主在群里
        /// </summary>
        /// <param name="createUserId"></param>
        /// <param name="groupName"></param>
        /// <param name="des"></param>
        /// <returns></returns>
        public Result CreateGroup(int createUserId, string groupName, string des = "")
        {
            if (string.IsNullOrEmpty(groupName))
            {
                return  new Result("002");
            }

            using (var db = new PortalDb())
            {
                var owner =db.Users.Find(createUserId);
                if(owner==null) return new Result("002","用户不存在!");
                var group=new Group()
                {
                    CreateUserId = createUserId,
                    GroupName = groupName,
                    Description = des,
                };
                group.Users.Add(owner);
                db.Groups.Add(group);
                db.SaveChanges();
             return  new Result(){Data = group.Id};
            }
        }

        public Result AddUsersToGroup(List<int> userlist, string groupId)
        {
            if(userlist==null||userlist.Count==0) return  new Result("002");
            if(string.IsNullOrEmpty(groupId)||groupId.Length!=20) return new Result("002");
            using (var db = new PortalDb())
            {
                var group = db.Groups.Include(n=>n.Users).FirstOrDefault(n => n.Id == groupId && !n.IsDeleteD);
                if(group==null) return new Result("002","群不存在!");
                foreach (var userid in userlist)
                {
                    var user =db.Users.Find(userid);
                    //如果已经在群里了呢，不再重复添加
                    if(user==null) return new Result("002","用户"+userid+"不存在!");
                    if(group.Users.All(n=>n.UserGuid!=user.UserGuid))
                    group.Users.Add(user);
                }
                db.SaveChanges();
                return new Result(){Data = group};
            }
        }

        /// <summary>
        /// 直接拉用户创建群，
        /// 像微信一样用
        /// list的成员 算上群主至少有三个人
        /// </summary>
        /// <param name="createUserId"></param>
        /// <param name="userlist">list中不用保护createUserId</param>
        /// <param name="groupName"></param>
        /// <returns></returns>
        public Result AddUserAndCreateGroup(int createUserId,List<int> userlist, string groupName = "")
        {
            if (createUserId<=0)
            {
                return new Result("002");
            }
            if (userlist == null || userlist.Count == 0 || userlist.Count < 2) return new Result("002", "群用户至少有2个人");
            userlist.Add(createUserId);

            using (var db = new PortalDb())
            {
               //创建群
                var owner = db.Users.Find(createUserId);
                if (owner == null) return new Result("002", "用户不存在!");
                var group = new Group()
                {
                    CreateUserId = createUserId,
                    GroupName = groupName,
                };
                //为什么这里没有添加进去呢？
                //难道还需要加一遍user中的group? 
                db.Groups.Add(group);
                //添加人
                foreach (var userid in userlist)
                {
                    var user = db.Users.Find(userid);
                    //如果已经在群里了呢，不再重复添加
                    if (user == null) return new Result("002", "用户" + userid + "不存在!");
                    if (group.Users.All(n => n.Id != user.Id))
                        group.Users.Add(user);
                }
                //修改群名 
                if (string.IsNullOrEmpty(groupName))
                {

                    foreach (var user in group.Users.Take(5))
                    {
                        //这个逗号可以再处理下
                        groupName += user.Username + ",";
                    }
                    group.GroupName = groupName;

                }
                db.SaveChanges();
                return new Result(){Data = group};
            }
        }


        /// <summary>
        /// 从群里面删除用户
        /// </summary>
        /// <param name="userList"></param>
        /// <param name="groupId"></param>
        /// <returns></returns>
        public Result RemoveUsersFromGroup(List<int> userList, string groupId)
        {
            //不能移除群主!
            using (var db=new PortalDb())
            {
                var group = db.Groups.FirstOrDefault(n => n.Id == groupId && !n.IsDeleteD);
                if(group==null) return  new Result("002","群不存在!");

                if(userList.Any(n=>n==group.CreateUserId)) return new Result("002","不能移除群主!");

                foreach (var userId in userList)
                {
                    var u = db.Users.Find(userId);
                    if (u != null)
                        group.Users.Remove(u);
                }
                group.ModifyTime=DateTime.Now;
                db.SaveChanges();
                return new Result() { Data = group };
            }
        }

        public Group GetGroup(string groupId)
        {
            return _db.Groups.Include(n=>n.Users).FirstOrDefault(n => n.Id == groupId && !n.IsDeleteD);
        }
        public Result RemoveGroup(string groupId)
        {
         //权限问题，但是api暂时不显示
            if (string.IsNullOrEmpty(groupId) || groupId.Length != 20) return new Result("002");
            //权限由上层控制
            using (var db = new PortalDb())
            {
                var group = db.Groups.FirstOrDefault(n => n.Id == groupId && !n.IsDeleteD);
                group.IsDeleteD = true;
                db.SaveChanges();
                return new Result();
            }
        }

        /// <summary>
        /// 暂时测试用的api
        /// </summary>
        /// <param name="guid"></param>
        /// <returns></returns>
        public List<Friend> GetFriends(string guid)
        {
            var guid1 = Guid.Parse(guid);
            var friends = _db.Users.Where(n => n.UserGuid != guid1).ToList().Select(user => new Friend(user)).ToList();
            var group = _db.Groups.Include(n=>n.Users).Where(n=>n.Users.Any(m=>m.UserGuid==guid1)).ToList().Select(g => new Friend(g));
            friends.AddRange(group);
            return friends;
        } 
 
    }
}