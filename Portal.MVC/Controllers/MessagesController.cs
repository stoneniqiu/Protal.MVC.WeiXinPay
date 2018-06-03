using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Niqiu.Core.Domain.Common;
using Niqiu.Core.Domain.Messages;
using Niqiu.Core.Domain.User;
using Niqiu.Core.Services;
using Niqiu.Core.Services.Firends;
using Niqiu.Core.Services.Messages;
using Niqiu.Core.Services.Questiones;
using Portal.MVC.Attributes;
using Portal.MVC.Models;
using Portal.MVC.ViewModel;

namespace Portal.MVC.Controllers
{
    [LoginValid]
    [MenuStatistics]
    public class MessagesController : Controller
    {
        private readonly IMessageService _messageService;
        private readonly IWorkContext _workContext;
        private readonly IUserService _userService;
        private string _messageCache = "message{0}";
        private readonly ICacheManager _cacheManager;
        private User _currentUser;
        private readonly IFirendService _firendService;
        private readonly IQuestionService _questionService;

        public MessagesController(IMessageService messageService, IQuestionService questionService, IFirendService firendService, ICacheManager cacheManager, IWorkContext workContext, IUserService userService)
        {
            _messageService = messageService;
            _workContext = workContext;
            _userService = userService;
            _firendService = firendService;
            _cacheManager = cacheManager;
            _questionService = questionService;
        }

        //
        // GET: /Messages/

        public User CurrentUser
        {
            get { return _currentUser ?? (_currentUser = _workContext.CurrentUser); }
            set { _currentUser = value; }
        }

        public ActionResult Index()
        {
            var user = CurrentUser;
            var messages = _messageService.GetUserMessages(user.Id, 0, MessageType.All).ToList();
            var mcm = new MessageCenterModel
            {
                InfoMessages = messages.Where(n => n.MessageType == MessageType.Weibo && n.FromUserId != CurrentUser.Id).ToList(),
                User = user
            };

            foreach (var infoMessage in mcm.InfoMessages)
            {
                infoMessage.Question = _questionService.GetByGuid(infoMessage.RelateGuid);
            }


            //我的好友
            var fs = _firendService.GetAllFirends(user.Id);
            IList<Message> pm = new List<Message>();
            foreach (var f in fs)
            {
                var mes = _messageService.GetLastMessage(user.Id, f.FirendId);
                if (mes != null) pm.Add(mes);
            }
            mcm.PrivateMessage = pm;
            //获取 未读的总条数

            var rewards = _messageService.GetAllMessages(0, user.Id, MessageType.Reward);
            var rewarded = _messageService.GetAllMessages(0, user.Id, MessageType.SystemInfo);
            mcm.SystemMessage.AddRange(rewarded);
            mcm.SystemMessage.AddRange(rewards);
            mcm.SystemMessage.AddRange(_messageService.GetAllMessages(0, user.Id, MessageType.ReChange));
            mcm.SystemMessage.AddRange(_messageService.GetAllMessages(0, user.Id, MessageType.BuyInfo));

            return View(mcm);
        }

        public ActionResult SetAllInfoMessageReaded()
        {
            var user = CurrentUser;
            var messages = _messageService.GetUserMessages(user.Id, 0, MessageType.All).ToList();
            var infoMessages =
                messages.Where(n => n.MessageType == MessageType.Weibo && n.FromUserId != CurrentUser.Id).ToList();
            foreach (var item in infoMessages)
            {
                if (!item.IsRead)
                {
                    SetReadMessage(item.Id);
                    item.IsRead = true;
                }
            }



            return Content("1");
        }

        public ActionResult DeleteMessage(int id)
        {
            var msg = _messageService.GetById(id);
            if (msg != null)
            {
                msg.Deleted = true;
                _messageService.UpdateMessage(msg);
            }
            return Json(1);
        }

        public ActionResult SetAllSystemMessageReaded()
        {
            var user = CurrentUser;
            var rewards = _messageService.GetAllMessages(0, user.Id, MessageType.Reward);
            var rewarded = _messageService.GetAllMessages(0, user.Id, MessageType.SystemInfo);
            var syss = new List<Message>();
            syss.AddRange(rewarded);
            syss.AddRange(rewards);
            syss.AddRange(_messageService.GetAllMessages(0, user.Id, MessageType.ReChange));
            syss.AddRange(_messageService.GetAllMessages(0, user.Id, MessageType.BuyInfo));

            foreach (var item in syss)
            {
                if (!item.IsRead)
                {
                    SetReadMessage(item.Id);
                    item.IsRead = true;
                }
            }
            return Content("1");
        }

        public void SetReadMessage(int id)
        {
            using (var db = new PortalDb())
            {
                var msg = db.Messages.Find(id);
                msg.IsRead = true;
                db.SaveChanges();
            }

        }


        public ActionResult Chat(int touserid)
        {
            var touser = _userService.GetUserById(touserid);
            if (touser == null) return new HttpStatusCodeResult(404);
            ViewBag.Title = touser.Username;
            var user = CurrentUser;
            var chamodel = new ChatModel
            {
                Me = user,
                ToUser = touser
            };
            return View(chamodel);
        }
        public ActionResult GetMessage(int toid, int lastId)
        {
            var user = CurrentUser;
            var touser = _userService.GetUserById(toid);
            var newmsgs = _messageService.GetUserMessages(user.Id, toid, MessageType.Chat, lastId);
            foreach (var message in newmsgs)
            {
                if (!message.IsRead) _messageService.ReadedMessage(message.Id);
            }
            var chamodel = new ChatJsonModel(user, touser, newmsgs);
            return Json(chamodel);
        }


        public ActionResult Delete(int id)
        {
            var user = _workContext.CurrentUser;
            var mesg = _messageService.GetById(id);
            if (user.Id == mesg.FromUserId)
            {
                mesg.FromUserHide = true;
            }
            if (user.Id == mesg.ToUserId)
            {
                mesg.ToUserHide = true;
            }
            _messageService.UpdateMessage(mesg);
            //_messageService.DeleteMessage(id);
            return Json(id);
        }

        [ExceptionLog]
        public ActionResult SendTo(int userid, string msg)
        {
            var user = CurrentUser;
            try
            {
                var model = _messageService.SendTo(user.Id, userid, msg);
                return Json(model.Id);
            }
            catch (Exception e)
            {
                return Json(0);
            }
        }
        public ActionResult SendToTest(int userid, int touserid, string msg)
        {
            var model = _messageService.SendTo(userid, touserid, msg);
            return Json(model.Id);
        }



    }
}
