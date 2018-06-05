using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using Portal.MVC5.Services;
using Ninject;
using Niqiu.Core.Domain.Common;
using Niqiu.Core.Domain.User;
using Niqiu.Core.Helpers;
using Niqiu.Core.Services;
using Portal.MVC5.Attributes;
using Portal.MVC5.Models;

namespace Portal.MVC5.Controllers.API
{
    public class IdentyController : ApiController
    {
        private UserDbService _userService = new UserDbService();
        [HttpPost]
        public Result Register(RegistDto model)
        {
            if (string.IsNullOrEmpty(model.UserName) || string.IsNullOrEmpty(model.PassWord))
                return new Result("000");
            //中文名字就两个
            if (model.UserName.Length < 2) return new Result("002", "用户名太短");
            if (model.PassWord.Length < 6) return new Result("002", "密码太短");
            //检查是否重复
            var raw = _userService.GetUserByUsername(model.UserName);
            if(raw!=null) return new Result("005", "用户已经存在");

            var user = new User() { Username = model.UserName, Password = Encrypt.GetMd5Code(model.PassWord) ,Active = true};
            //插入
            _userService.InsertUser(user);
            //返回guid，name，时间 加密的token
            raw = _userService.GetUserByUsername(model.UserName);
            return new Result() { Data = _userService.CreateToken(raw) };

           // AuthenticationService.SignIn(user, true);

        }

        [HttpPost]
        public Result Login(LoginDto model)
        {
            if (model==null||string.IsNullOrEmpty(model.uname) || string.IsNullOrEmpty(model.pwd))
                return new Result("000");
            
            var user = _userService.GetUserByUsername(model.uname);
            //支持手机号登陆
            if (user == null)
            {
                user = _userService.GetUserByMobile(model.uname);
            }

            if (user == null)
            { return new Result("000", "用户不存在"); 
            }

            if (Encrypt.GetMd5Code(model.pwd) == user.Password)
            {
                return new Result() { Data = _userService.CreateToken(user) };
            }
            return new Result("003","密码错误");
        }

        [HttpPost]
        public Result Authen(string token)
        {
            return _userService.validToken(token);
        }

        /// <summary>
        /// 对外
        /// </summary>
        /// <returns></returns>
        [ValidToken]
        [HttpPost]
        public Result DecryptToken()
        {
            return new Result(){Data = _userService.DecryptToken()};
        }

        public List<string> GetOtherUsers(string userid)
        {
            return _userService.GetUserTable().Where(n => n.UserGuid.ToString() != userid).Select(n=>n.UserGuid.ToString()).ToList();
        }
        [Inject]
        public IAuthenticationService AuthenticationService { get; set; }
    }
}
