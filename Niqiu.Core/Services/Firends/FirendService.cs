using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using Niqiu.Core.Domain;
using Niqiu.Core.Domain.Common;
using Niqiu.Core.Domain.Config;
using Niqiu.Core.Domain.User;

namespace Niqiu.Core.Services.Firends
{
    public class FirendService : IFirendService
    {
        private readonly IRepository<Firend> _firendRepository;
        private readonly IRepository<User> _userRepository;
        public FirendService(IRepository<Firend> firendRepository, IRepository<User> userRepository)
        {
            _firendRepository = firendRepository;
            _userRepository = userRepository;
        }
        public void InsertFirend(int userid, User user)
        {
            if (user == null) throw new ArgumentNullException("user");
            var f = new Firend()
            {
                UserId = userid,
                FirendId = user.Id,
                FirendImg = user.ImgUrl,
                FirendName = user.Username,
            };
            InsertFirend(f);
        }

        public void InsertFirend(Firend user)
        {
            if (user == null) throw new ArgumentNullException("user");
            user.IsArgee = !PortalConfig.AddFirendNeedAgree;
            _firendRepository.Insert(user);
        }

        public Firend GetFirendById(int id)
        {
            if (id == 0)
                return null;

            return _firendRepository.GetById(id);
        }

        public User GetUserByUserId(int userid)
        {
           return _userRepository.GetById(userid);
        }
        public Firend GetFirendByUserId(int myid, int userId)
        {
            return _firendRepository.Table.FirstOrDefault(n => n.UserId == myid && n.FirendId == userId);
        }

        public void UpdateFirend(Firend firend)
        {
            if (firend == null)
                throw new ArgumentNullException("firend");
            _firendRepository.Update(firend);
        }

        public Firend GetFirendByFirendname(string username)
        {
            return string.IsNullOrWhiteSpace(username) ? null : _firendRepository.Table.FirstOrDefault(n => n.FirendName == username);

        }

        public int GetNewFirendCount(int userid)
        {
            var news = _firendRepository.Table.Where(n => n.FirendId == userid&&!n.Readed).ToList();
            var mys = _firendRepository.Table.Where(n => n.UserId == userid).ToList();
            var count = 0;
            foreach (var firend in news)
            {
                if (mys.All(n => n.FirendId != firend.UserId)) count++;
            }

            return count;
        }

        public bool HasNewFirends(int userid)
        {
            var news = _firendRepository.Table.Where(n => n.FirendId == userid).ToList();
            var mys = _firendRepository.Table.Where(n => n.UserId == userid).ToList();
            var count = 0;
            foreach (var firend in news)
            {
                if (mys.All(n => n.FirendId != firend.UserId)) count++;
            }

            return count>0;
        }

        public void DeleteFirend(int id)
        {
            var f = GetFirendById(id);
            _firendRepository.Delete(f);
        }
 

        public IPagedList<Firend> GetAllFirends(int userid = 0, string username = "", bool hasblacklist = false, int pageIndex = 0, int pageSize = 2147483647)
        {
            var query = _firendRepository.Table.Where(n => !n.Blacklisted);
            if (userid != 0)
            {
                query = query.Where(n => n.UserId == userid);
            }
            if (!string.IsNullOrWhiteSpace(username))
            {
                query = query.Where(c => c.FirendName.Contains(username));
            }
            query = query.OrderByDescending(c => c.Id);
            var users = new PagedList<Firend>(query, pageIndex, pageSize);
            return users;
        }

        public PortalResult AddOrCanelFirend(int myid, User user)
        {
            //不能关注自己！
            if (user == null || _userRepository.GetById(user.Id) == null) return new PortalResult("用户不存在！");
            if (myid == user.Id) return new PortalResult("不能关注自己");
            var friend = GetFirendByUserId(myid, user.Id);
            if (friend == null)
            {
                InsertFirend(myid, user);
                return new PortalResult(true) { Message = "关注成功！" };
            }
            DeleteFirend(friend.Id);
            return new PortalResult(true) { Message = "取消成功！" };
        }

        public PortalResult AddOrCanelFirend(int myid, int userid)
        {
            if (userid == 0) return new PortalResult("id错误");
            var model = _userRepository.GetById(userid);
            return AddOrCanelFirend(myid, model);
        }

        public IEnumerable<Firend> GetAttentioned(int userid)
        {
            return _firendRepository.Table.Where(n => n.FirendId == userid);
        }
    }
}
