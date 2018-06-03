using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Niqiu.Core.Domain.Messages;
using Niqiu.Core.Domain.User;
using Niqiu.Core.Helpers;
using Niqiu.Core.Services;
using Niqiu.Core.Services.Messages;
using Portal.MVC.Attributes;
using Portal.MVC.Models;
using Portal.MVC.ViewModel;

namespace Portal.MVC.Areas.Admin.Controllers
{

    public class MsgController : AdminBaseController
    {
        private readonly IMessageService _messageService;
        private readonly IRepository<User> _repository;
        private readonly IUserService _userRepository;
        public MsgController(IMessageService messageService, IRepository<User> repository, IUserService useRepository)
        {
            _messageService = messageService;
            _userRepository = useRepository;
            _repository = repository;
        }

        public ActionResult Index()
        {
            return View();
        }

        public ActionResult ListPage(int timeType=0)
        {
                var ms = _messageService.AllChatMessages();
                switch (timeType)
                {
                    case 1:
                        ms = ms.Where(n => n.CreateTime >= DateTime.Now.AddDays(-5));
                        break;
                    case 2:
                        ms = ms.Where(n => n.CreateTime >= DateTime.Now.AddMonths(-1));
                        break;
                    case 3:
                    case 0:
                        ms = ms.Where(n => n.CreateTime >= DateTime.Now.AddMonths(-3));
                        break;
                }
                return PartialView(ms);
        }

        public ActionResult SendMessage()
        {
            return View();
        }

        public ActionResult UserPage(string name = "")
        {
            var users = _userRepository.GetAllUsers(name);
            return PartialView(users);
        }


        public ActionResult SendTo(int id,string content)
        {
            var sys = _userRepository.GetUserBySystemName(SystemUserNames.SystemWallet);
            var mes = new Message()
            {
                MessageType = MessageType.SystemInfo,
                Content = content,
                ToUserId = id,
                FromUserId = sys.Id
            };
            _messageService.InsertMessage(mes);

            return Json(1);
        }

        public ActionResult SendToUsers(string ids, string content)
        {
            if (string.IsNullOrEmpty(ids)) return Json(0);
            var sys = _userRepository.GetUserBySystemName(SystemUserNames.SystemWallet);
            var idArr = ids.Split(',');
            using (var db= new PortalDb())
            {
                var ms=new List<Message>();
                foreach (var s in idArr)
                {
                    var id = 0;
                    int.TryParse(s, out id);
                    if (id != 0)
                    {
                        var mes = new Message()
                        {
                            MessageType = MessageType.SystemInfo,
                            Content = content,
                            ToUserId = id,
                            FromUserId = sys.Id
                        };
                        ms.Add(mes);
                    }

                }
                db.Messages.AddRange(ms);
                db.SaveChanges();
            }
            return Json(1);
        }

        public ActionResult SendToAll(string content)
        {
            var sys = _userRepository.GetUserBySystemName(SystemUserNames.SystemWallet);
            using (var db = new PortalDb())
            {
                var ms = new List<Message>();
                var users = db.Users.ToList();
                foreach (var user in users)
                {
                    var mes = new Message()
                    {
                        MessageType = MessageType.SystemInfo,
                        Content = content,
                        ToUserId = user.Id,
                        FromUserId = sys.Id
                    };
                    ms.Add(mes);

                }
                db.Messages.AddRange(ms);
                db.SaveChanges();
            }
            return Json(1);
        }


        #region 添加管理员

        [AdminAuthorize("EditAdmin")]
        public ActionResult Admins()
        {
            var users = _repository.Table.Where(n => n.UserRoles.Any());
            return View(users);
        }

        [AdminAuthorize("CreateAdmin")]
        public ActionResult AddAdmin()
        {
            var roles = _userRepository.GetAllUserRoles();
            ViewBag.S = new SelectList(roles, "Id", "Name");
            var model = new AdminModel();
            return View(model);
        }

        [AdminAuthorize("CreateAdmin")]
        [HttpPost]
        public ActionResult AddAdmin(AdminModel model)
        {
            var key = Request.Form["RoleId"];

            var roles = _userRepository.GetAllUserRoles();
            ViewBag.S = new SelectList(roles, "Id", "Name");

            if (model.RoleId == 0)
            {
                Error("请选择角色");
                return View(model);
            }
            var user = new User
            {
                UserGuid = Guid.NewGuid(),
                Username = model.UserName,
                Email = model.Email,
                Mobile = model.Mobile,
                Active = true,
                //加密存储
                Password = Encrypt.GetMd5Code(model.Password),
            };
            _userRepository.InsertUser(user);
            var newuser = _userRepository.GetUserByGuid(user.UserGuid);
            var role = _userRepository.GetUserRoleById(model.RoleId);
            //先只有一个角色
            newuser.UserRoles.Clear();
            newuser.UserRoles.Add(role);
            _userRepository.UpdateUser(newuser);
            Success();

            return View(model);
        }

        [AdminAuthorize("EditAdmin")]
        public ActionResult EditAdmin(int id)
        {
            var roles = _userRepository.GetAllUserRoles();
            ViewBag.S = new SelectList(roles, "Id", "Name");
            var user = _repository.GetById(id);
            var model = new EditAdminModel()
            {
                Id = id,
                UserName = user.Username,
                RoleId = user.UserRoles.First().Id,
                Email = user.Email,
                Mobile = user.Mobile
            };

            return View(model);
        }
        [AdminAuthorize("EditAdmin")]
        [HttpPost]
        public ActionResult EditAdmin(EditAdminModel model)
        {
            var roles = _userRepository.GetAllUserRoles();
            ViewBag.S = new SelectList(roles, "Id", "Name");
            var user = _repository.GetById(model.Id);
            user.Username = model.UserName;
            user.Email = model.Email;
            user.Mobile = model.Mobile;
            user.Password = Encrypt.EncryptString(model.Password);
            user.UserRoles.Clear();
            var role = _userRepository.GetUserRoleById(model.RoleId);
            user.UserRoles.Add(role);
            _userRepository.UpdateUser(user);
            Success();
            return View(model);
        }
        /// <summary>
        /// 移除管理员
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpPost]
        [AdminAuthorize("EditAdmin")]
        public ActionResult DeleteAdmin(int id)
        {
            var user = _userRepository.GetUserById(id);
            user.UserRoles.Clear();
            _userRepository.UpdateUser(user);
            return Json(1);
        }

        #endregion

    }
}
