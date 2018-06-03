using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Niqiu.Core.Domain.User;

namespace Portal.MVC.Models.Services
{
    public class FriendDbService
    {
        private PortalDb db = new PortalDb();
        public Firend GetFirendByUserId(int myid, int userId)
        {
            return db.Firends.FirstOrDefault(n => n.UserId == myid && n.FirendId == userId);
        }
    }
}