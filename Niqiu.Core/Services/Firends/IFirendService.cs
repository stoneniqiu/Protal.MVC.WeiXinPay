using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Niqiu.Core.Domain;
using Niqiu.Core.Domain.Common;
using Niqiu.Core.Domain.User;

namespace Niqiu.Core.Services.Firends
{
   public interface IFirendService
   {
       void InsertFirend(int userid,User user);
       void InsertFirend(Firend user);

       Firend GetFirendById(int id);
       Firend GetFirendByUserId(int myid, int userId);
       void UpdateFirend(Firend firend);
       Firend GetFirendByFirendname(string username);
       void DeleteFirend(int id);
       User GetUserByUserId(int userid);
       IPagedList<Firend> GetAllFirends(int userid=0,string username = "", bool hasblacklist = false, int pageIndex = 0, int pageSize = 2147483647);
       PortalResult AddOrCanelFirend(int myid, int userid);
       PortalResult AddOrCanelFirend(int myid, User user);

       IEnumerable<Firend> GetAttentioned(int userid);
       int GetNewFirendCount(int userid);

       bool HasNewFirends(int userid);
   }
}
