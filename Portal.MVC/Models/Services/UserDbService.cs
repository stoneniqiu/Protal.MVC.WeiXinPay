using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using Niqiu.Core.Domain;
using Niqiu.Core.Domain.User;
using Niqiu.Core.Helpers;
using Senparc.Weixin.Open.OAuthAPIs;

namespace Portal.MVC.Models.Services
{
    public class UserDbService
    {
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

        public void UpdateUserForWx(string openid, OAuthUserInfo userInfo)
        {

            using (var db = new PortalDb())
            {
                var user = db.Users.FirstOrDefault(n => n.OpenId == openid);
                if (user == null) return;
                user.ImgUrl = userInfo.headimgurl;
                user.Gender = userInfo.sex.ToString();
                user.Province = userInfo.province;
                user.Country = userInfo.country;
                user.City = userInfo.city;
                user.AuthType = AuthType.WeiXin;
                user.OpenId = userInfo.openid;
                user.Active = true;
                user.ModifyTime = DateTime.Now;
                user.LastLoginDateUtc = DateTime.Now;
                db.SaveChanges();
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
                    raw.PaymentPassword = user.PaymentPassword;
                    raw.RealName = user.RealName;
                    raw.Description = user.Description;
                    raw.ImgUrl = user.ImgUrl;
                    raw.Address = user.Address;
                    raw.IdCared = user.IdCared;
                    raw.Active = user.Active;
                    raw.Deleted = user.Deleted;
                    raw.IsIllegal = user.IsIllegal;
                    raw.IsSystemAccount = user.IsSystemAccount;
                    raw.SystemName = user.SystemName;
                    raw.LastIpAddress = user.LastIpAddress;
                    raw.LastLoginDateUtc = user.LastLoginDateUtc;
                    raw.LastActivityDateUtc = user.LastActivityDateUtc;
                    raw.GradeType = user.GradeType;
                    raw.WeiXinId = user.WeiXinId;
                    raw.AuthType = user.AuthType;
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
        public bool SetPaymentPassword(int userid, string password)
        {
            var user = _users.Find(userid);
            if (user != null && !string.IsNullOrEmpty(password) && password.Length >= 6)
            {
                user.PaymentPassword = Encrypt.EncryptString(password);
                UpdateUser(user);
                return true;
            }
            return false;
        }
        public bool ValidPaymentPassword(int userid, string password)
        {
            var user = _users.Find(userid);
            var x = Encrypt.EncryptString(password);
            return user.PaymentPassword == x;
        }

        public void Feeback(User user, string content)
        {
            if (user != null && !string.IsNullOrWhiteSpace(content))
            {
                var fb = new Feeback()
                {
                    UserId = user.Id,
                    UserName = user.Username,
                    Content = content
                };
                using (var db=new PortalDb())
                {
                    db.Feebacks.Add(fb);
                    db.SaveChanges();
                }
            }

        }

        private DbSet<User> _users
        {
            get { return _db.Users; }
        } 
    }
}