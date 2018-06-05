using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Niqiu.Core.Domain.Common;
using Niqiu.Core.Domain.IM;
using Niqiu.Core.Services;

namespace Portal.MVC5.Controllers.API
{
    public class UserController : ApiController
    {
        private UserDbService _userDb=new UserDbService();
        
        [HttpPost]
        public Result Friends(string guid)
        {
            return new Result() { Data = _userDb.GetFriends(guid) };
        }
    }
}
