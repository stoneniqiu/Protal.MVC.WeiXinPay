using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using Niqiu.Core.Domain.Common;
using Niqiu.Core.Domain.Config;
using Niqiu.Core.Domain.Payments;
using Niqiu.Core.Domain.Questions;
using Niqiu.Core.Domain.User;
using Niqiu.Core.Helpers;
using Niqiu.Core.Services;
using Portal.MVC.Attributes;
using Portal.MVC.Models;
using Portal.MVC.Models.Services;
using Portal.MVC.WXDevice;
using Portal.MVC.WxPayAPI.lib;
using Senparc.Weixin;
using Senparc.Weixin.CommonAPIs;
using WxPayAPI;

namespace Portal.MVC.Controllers
{
    [LoginValid]
    [MenuStatistics]
    public class QuestionController : Controller
    {
        private readonly IWorkContext _workContext;
        private readonly ICacheManager _cacheManager;
        private string _questionCache = "create_question_{0}";
        public QuestionController(IWorkContext workContext,
           ICacheManager cacheManager)
        {
            _workContext = workContext;
            _cacheManager = cacheManager;
        }

        private WxDeviceService wxDeviceService = new WxDeviceService();
        private QuestionDbService questionDbService = new QuestionDbService();
        private MessageDbService messageDbService = new MessageDbService();
        private PayMeentDbService _payMeentDbService = new PayMeentDbService();
        public ActionResult CreateStep1()
        {
            //if (string.IsNullOrEmpty(CurrentUser.PaymentPassword))
            //{
            //    return RedirectToAction("SetPayPassword", "UserCenter",
            //        new { returnUrl = Url.Action("CreateStep1", "Question") });
            //}

            var question = _cacheManager.Get<Question>(getKey()) ?? new Question();
            //检查是否有钱包对象
            //没有就创建一个
            var user = CurrentUser;
            _payMeentDbService.CheckAndCreate(user.Id);


            var model = new WXShareModel();
            model.appId = WxPayConfig.APPID;
            model.nonceStr = WxPayApi.GenerateNonceStr();
            model.timestamp = Util.CreateTimestamp();
            model.ticket = GetTicket();
            model.url = "http://www.haoyoucai888.com" + Request.Url.PathAndQuery;
            model.MakeSign();

            ViewBag.Share = model;

            return View(question);
        }

         public ActionResult DownWxImage(string serverId)
        {
            var token = getToken();
             var url = string.Format("http://file.api.weixin.qq.com/cgi-bin/media/get?access_token={0}&media_id={1}",
                 token, serverId);

             HttpWebRequest req = (HttpWebRequest)HttpWebRequest.Create(url);

             req.Method = "GET";
             using (WebResponse wr = req.GetResponse())
             {
                 HttpWebResponse myResponse = (HttpWebResponse)req.GetResponse();

                var strpath = myResponse.ResponseUri.ToString();
                 WebClient mywebclient = new WebClient();

                 var path = "/Content/UploadFiles/mobile/";
                 var uploadpath = Server.MapPath(path);
                 if (!Directory.Exists(uploadpath))
                 {
                     Directory.CreateDirectory(uploadpath);
                 }
                 string saveName = Encrypt.GenerateOrderNumber() + ".jpg";
                 var savePath = uploadpath + saveName;

                 try
                 {
                     mywebclient.DownloadFile(strpath, savePath);
                     return Json(new { Success = true, SaveName = path + saveName });
                 }
                 catch (Exception ex)
                 {
                     savePath = ex.ToString();
                 }

             }
             return Json(new {Success = false, Message = "上传失败！"});
        }

        private string ticketKey = "jsapi_ticket";
        public string GetTicket()
        {
            var cticket = getCacheTicket();
            var token = getToken();
            if (string.IsNullOrEmpty(cticket))
            {
                cticket = wxDeviceService.GetJsApiTicket(token).ticket;
                setCacheTicket(cticket);
            }

            return cticket;
        }
        private string getCacheTicket()
        {
            return _cacheManager.Get<string>(ticketKey);
        }
        private void setCacheTicket(string cache)
        {
            _cacheManager.Set(ticketKey, cache, 7000);
        }
        private string getToken()
        {
            var token = getcacheToken();
            if (token == null || string.IsNullOrEmpty(token.access_token))//|| checkExpire(token.expires_in)
            {
                token = wxDeviceService.GetAccessToken();
                setToken(token);
            }
            return token.access_token;
        }
        private TokenResult getcacheToken()
        {
            return _cacheManager.Get<TokenResult>(tokenKey);
        }
        private string tokenKey = "access_token";
        private void setToken(TokenResult token)
        {
            _cacheManager.Set(tokenKey, token, 7000);
        }

        [HttpPost]
        public ActionResult CreateStep1(Question model)
        {
            //Session[getKey()] = model;
            // _cacheManager.Set(getKey(), model, 3600);
            return PartialView(model);
        }

        private string getKey()
        {
            var user = CurrentUser;
            return string.Format(_questionCache, user.Id);
        }
        public ActionResult CreateStep2()
        {
            //var question = Session[getKey()] as Question;
            var question = _cacheManager.Get<Question>(getKey());
            if (question == null) return RedirectToAction("CreateStep1");

            return View(question);
        }

        public ActionResult RecoderView(int id)
        {
            var q = questionDbService.GetById(id);
            return PartialView(q);
        }

        [OutputCache(Duration = 0)]
        public ActionResult CreateStep2Url(string imgUrl)
        {
            if (string.IsNullOrWhiteSpace(imgUrl)) return RedirectToAction("CreateStep1");

            var question = _cacheManager.Get<Question>(getKey());
            if (question != null)
            {
                if (question.ImageUrl != imgUrl)
                {
                    question = new Question() { ImageUrl = imgUrl };
                }

            }
            else
            {
                question = new Question() { ImageUrl = imgUrl };
            }

            return View("CreateStep2", question);
        }

        public JsonResult GetCacheQuestion(string imgstr)
        {
            var question = _cacheManager.Get<Question>(getKey());
            if (question == null||question.ImageUrl!=imgstr) return Json(false);

            return Json(new
            {
                title = question.Title,
                tip = question.Tip,
                answer = question.Answer,
                reward = question.Reward,
                num = question.RewardPeopleNum,
                rtype = question.RewardType,
                time = question.ExpireTime,
                img=question.ImageUrl
            });
        }

        public ActionResult Edit()
        {
            var model = _cacheManager.Get<Question>(getKey());
            if (model == null) return RedirectToAction("CreateStep1");

            return View("CreateStep2", model);
        }

        //false就显示

        public JsonResult HasShowShare(int id)
        {
            using (var db = new PortalDb())
            {
                var q = db.Questions.Find(id);
                if (q != null)
                {
                    var user = _workContext.CurrentUser;
                    if (user != null && user.Id == q.UserId)
                    {
                        var res = q.HasShowShare;
                        q.HasShowShare = true;
                        db.SaveChanges();

                        return Json(res);
                    }
                }
            }

            return Json(true);
        }

        public ActionResult CheckMoney(decimal reward)
        {
            var user = CurrentUser;
            var wallet = _payMeentDbService.CheckAndCreate(user.Id);
            var res = wallet.Balance >= reward;
            return Json(res, JsonRequestBehavior.AllowGet);
        }

        const int MaxPeople=1000;
        [HttpPost]
        public ActionResult CreateStep2(Question model)
        {
            //创建并删除缓存
            var result = new PortalResult();
            if (model.RewardPeopleNum > MaxPeople) return Json(new PortalResult(string.Format("悬赏人数不能大于{0}人",MaxPeople)));
            if (model.RewardPeopleNum < 1) return Json(new PortalResult("悬赏人数不能小于1人"));
            if (model.ClearAnwser().Length > 30) return Json(new PortalResult("答案不能大于30个字"));

            var res = model.ValidReward();
            if (!res.IsSuccess) return Json(res);

            Logger.Debug("谜题" + model.Title + ",提示" + model.Tip);

            // if (model.Reward > 200) return Json(new PortalResult("悬赏金额不能大于200"));
            if (string.IsNullOrEmpty(model.ImageUrl)) return Json(new PortalResult("图片不能为空"));

            if (model.ExpireHours != 720)
            {
                model.ExpireTime = DateTime.Now.AddHours(model.ExpireHours);
            }
            else
            {
                model.ExpireTime = DateTime.Now.AddMonths(1);
            }
            if (model.RewardType == RewardType.Decline)
            {
                model.MinReward = model.Reward / model.RewardPeopleNum / 5;
            }

            if (ModelState.IsValid)
            {
                var user = CurrentUser;
                model.UserId = user.Id;
                _setModel(model);

                result.IsSuccess = true;

                return Json(result, JsonRequestBehavior.AllowGet);


                //还要修正钱数
            }
            var key = ModelState.Values.First().Errors.FirstOrDefault() ?? new ModelError("验证失败");
            result.Message = key.ErrorMessage;

            return Json(result, JsonRequestBehavior.AllowGet);
        }



        //重新发布走到这儿
        public ActionResult CreateStep3(int id = 0)
        {
            Question model;
            ViewBag.ReSend = "";
            var wallet = _payMeentDbService.GetByUserId(CurrentUser.Id);
            ViewBag.B = wallet.Balance;
            if (id == 0)
            {
                //有可能也是重新发布
                model = _cacheManager.Get<Question>(getKey());
                if (model == null) return RedirectToAction("CreateStep1");
            }
            else
            {
                //重新发布
                var caChemodel = _cacheManager.Get<Question>(getKey());
                if (caChemodel == null)
                {
                    //别人重发 处理谜题
                    var raw = questionDbService.GetById(id);
                    if (raw == null) return RedirectToAction("CreateStep1");
                    model = raw.Clone();
                    model.UserId = CurrentUser.Id;
                    model.User = null;
                    model.IsFinished = false;
                    model.RemanidReward = model.Reward;
                    model.Id = 0;
                    model.IsResend = true;
                    _cacheManager.Set(getKey(), model, 60 * 24);
                }
                else
                {
                    model = caChemodel;
                }

            }

            //没有设置密码，且金额满足钱包支付
            if (string.IsNullOrEmpty(CurrentUser.PaymentPassword) && wallet.Balance>=model.Reward)
            {
                return RedirectToAction("SetPayPassword", "UserCenter",
                    new { returnUrl = Url.Action("CreateStep3", "Question",new{id=id}) });
            }
            //是否是重新发布
            return View(model);
        }

        private void _setModel(object model)
        {
            _cacheManager.Remove(getKey());
            _cacheManager.Set(getKey(), model, 60 * 24);
        }

        [HttpPost]
        public ActionResult SumbitQuestion()
        {
            var model = _cacheManager.Get<Question>(getKey());
            var user = CurrentUser;

            if (model == null) return Json(new PortalResult("谜题不存在!"));

            if (model.Id != 0)
            {
                var raw = questionDbService.GetById(model.Id);

                //重新发布的金额大于剩余的金额
                if (model.Reward > raw.RemanidReward)
                {
                    //还需要多锁住一些钱
                    var gap = model.Reward - raw.RemanidReward;
                    if (_payMeentDbService.LockMoney(user.Id, gap))
                    {
                        return _submitQuestion(model.Clone(), model.Id);
                    }
                    return Json(new PortalResult("余额不足!"));
                }
                //如果相等 钱包不需要变化
                if (model.Reward == raw.RemanidReward)
                {
                    return _submitQuestion(model.Clone(), model.Id);
                }
                //如果修改了还有多的
                var gap1 = raw.RemanidReward - model.Reward;
                //钱退回到用户的余额
                //lock减少，余额增加
                //todo 等于这个地方零钱支付没有创建订单
                if (_payMeentDbService.UnLockMoney(user.Id, gap1))
                {
                    return _submitQuestion(model.Clone(), model.Id);
                }
                return Json(new PortalResult("金额有误!"));
                //清空原来谜题的金额
                //返回一个克隆的谜题 以供添加

            }

            return _submitQuestion(model);
        }

        public ActionResult FiniQuestion(int id)
        {
            var question = questionDbService.GetById(id);
            if (_payMeentDbService.UnLockMoney(question.UserId, question.RemanidReward))
            {
                questionDbService.FinishQuestion(id);
                return Json(true);
            };
            return Json(false);
        }


        private ActionResult _submitQuestion(Question model, int id = 0)
        {
            var result = new PortalResult();

            if (id == 0)
            {
                if (_payMeentDbService.LockMoney(model.UserId, model.Reward))
                {
                    var res = _insertModel(model);
                    if (res != null)
                    {
                        _cacheManager.Remove(getKey());
                        var q1 = questionDbService.GetByGuid(model.Guid);
                        result.IsSuccess = true;
                        result.Num = q1.Id;
                        return Json(result);
                    }
                    result.Message = "新增谜题失败!";
                    return Json(result);
                }
                result.Message = "余额不足,请充值";
                return Json(result);
            }
            //id不等于0的时候是不需要再锁钱的
            //关闭原来的谜题
            questionDbService.FinishQuestion(id);
            _insertModel(model);
            _cacheManager.Remove(getKey());
            var q = questionDbService.GetByGuid(model.Guid);
            result.IsSuccess = true;
            result.Num = q.Id;
            return Json(result);

        }

        private Question _insertModel(Question model)
        {
            try
            {
                var user = CurrentUser;
                //处理答案
                questionDbService.InsertQuestion(user, model);
                _cacheManager.Remove(getKey());
                messageDbService.SendToAttentioned(user.Id, string.Format("你的好友{0}发布了谜题", user.Username), model.Guid);
                //创建日志
                var mywallert = _payMeentDbService.GetByUserId(user.Id);
                //这个时候并未支付给系统账户，只是钱锁住了，充值的时候就是支付系统账户
                var paylog = new PaymentLog()
                {
                    Amount = model.Reward,
                    Remarks = model.Title,
                    OrderType = OrderType.Reward,
                    FromWalletId = mywallert.Id,
                    PayType = PayType.Wallet,
                };
                messageDbService.InsertPaymentLog(paylog);
                return questionDbService.GetByGuid(model.Guid);
            }
            catch (Exception e)
            {
                Logger.Error(string.Format("用户创建谜题错误{0}", e.Message));
                return null;

            }


        }

        [LoginValid]
        [ExceptionLog]
        public ActionResult Answer(int quesitonId, string answer)
        {
            var user = CurrentUser;
            if (string.IsNullOrWhiteSpace(answer)) return Json(new PortalResult("答案不能为空"));
            _payMeentDbService.CheckAndCreate(user.Id);
            //检查是否可以作答
            var check = questionDbService.CanAnswerQuestion(user.Id, quesitonId);
            if (!check.IsSuccess) return Json(check);
            var question = questionDbService.GetById(quesitonId);
            var result = question.ValidAnswer(answer);

            //回答正确，但没有支付成功，可以再回答一次。
            var anwserNotpay =
                questionDbService.GetAllAnswers()
                    .FirstOrDefault(n => n.UserId == user.Id && n.QuestioId == quesitonId && !n.IsPay && n.IsRight);
            try
            {

                questionDbService.AnswerQuestion(anwserNotpay == null, quesitonId, user.Id, answer, result);


                return Json(result ? new PortalResult(true, "恭喜你，回答正确!") : new PortalResult("对不起，您的回答错误"));
            }
            catch (Exception e)
            {
                Logger.Error(string.Format("id{0}用户{1}回答问题,发生错误{2}", user.Id, user.Username, e.Message));
                return Json(new PortalResult("回答失败，请重试"));
            }

            //开始作答，并判断正误。
            //存入数据库
        }



        [LoginValid]
        public ActionResult Comment(int questionId, string content)
        {
            var user = CurrentUser;

            using (var db = new PortalDb())
            {
                if (string.IsNullOrWhiteSpace(content)) return Json(new PortalResult("评论内容不能为空"));
                var question = db.Questions.Find(questionId);
                if (question == null) return Json(new PortalResult("谜题不存在"));

                var comment = new Comment()
                {
                    CommentUserId = user.Id,
                    QuestionId = questionId,
                    Content = content,
                };
                db.Comments.Add(comment);
                db.SaveChanges();
            }


            return Json(new PortalResult(true));
        }

        public ActionResult CommentsView(int quesitonId)
        {
            using (var db = new PortalDb())
            {
                var q = db.Questions.Find(quesitonId).Commnets.ToList();
                foreach (var comment in q)
                {
                    comment.User = db.Users.Find(comment.CommentUserId);
                }
                if (q.Count == 0) return Content("");
                return PartialView(q);
            }
        }


        public ActionResult UploadImg()
        {

            return View();
        }
        [HttpPost]
        public ActionResult UploadImg(HttpPostedFileBase file, string dir = "UserImg")
        {
            if (CheckImg(file) != "ok") return Json(new { Success = false, Message = "文件格式不对！" }, JsonRequestBehavior.AllowGet);

            if (file != null)
            {
                var path = "~/Content/UploadFiles/" + dir + "/";
                var uploadpath = Server.MapPath(path);
                if (!Directory.Exists(uploadpath))
                {
                    Directory.CreateDirectory(uploadpath);
                }
                string fileName = Path.GetFileName(file.FileName);// 原始文件名称
                string fileExtension = Path.GetExtension(fileName); // 文件扩展名
                //string saveName = Guid.NewGuid() + fileExtension; // 保存文件名称 这是个好方法。
                string saveName = Encrypt.GenerateOrderNumber() + fileExtension; // 保存文件名称 这是个好方法。
                var saveUrl = uploadpath + saveName;
                // file.SaveAs(saveUrl);
                System.Drawing.Image image = ImageManageHelper.RotateImage(file.InputStream);
                image.Save(saveUrl);
                if (file.ContentLength / 1024 > 500)//大于0.5M
                {
                    var _saveName = Encrypt.GenerateOrderNumber() + "_thumbnailUrl" + fileExtension;
                    var thumbnailUrl = uploadpath + _saveName;
                    var maxh = 400;
                    var maxw = 400;

                    if (image.Width > maxw)
                    {
                        maxh = 400 * image.Height / image.Width;
                    }

                    ImageManageHelper.GetPicThumbnail(saveUrl, thumbnailUrl, maxh, maxw, 90);
                    return Json(new { Success = true, SaveName = "/Content/UploadFiles/Mobile/" + _saveName });
                }
                return Json(new { Success = true, SaveName = "/Content/UploadFiles/Mobile/" + saveName });
            }
            return Json(new { Success = false, Message = "请选择要上传的文件！" }, JsonRequestBehavior.AllowGet);

        }
        [HttpPost]
        public ActionResult MUploadImg(HttpPostedFileBase mfile)
        {
            return UploadImg(mfile, "Mobile");
        }

        public byte[] picFile2bytes(string picFilePath)
        {
            FileStream fs = new FileStream(picFilePath, FileMode.Open, FileAccess.Read);
            byte[] bytePhoto = new byte[fs.Length];
            fs.Read(bytePhoto, 0, (int)fs.Length);
            fs.Close();
            return bytePhoto;
        }


        [HttpPost]
        public ActionResult MUploadImgBase64Str(string base64str)
        {
            try
            {
                var imgData = base64str.Split(',')[1];
                //过滤特殊字符即可   
                string dummyData = imgData.Trim().Replace("%", "").Replace(",", "").Replace(" ", "+");
                if (dummyData.Length % 4 > 0)
                {
                    dummyData = dummyData.PadRight(dummyData.Length + 4 - dummyData.Length % 4, '=');
                }
                byte[] byteArray = Convert.FromBase64String(dummyData);
                using (System.IO.MemoryStream ms = new System.IO.MemoryStream(byteArray))
                {
                    var img = System.Drawing.Image.FromStream(ms);

                    var path = "~/Content/UploadFiles/mobile/";
                    var uploadpath = Server.MapPath(path);
                    if (!Directory.Exists(uploadpath))
                    {
                        Directory.CreateDirectory(uploadpath);
                    }
                    string saveName = Encrypt.GenerateOrderNumber() + ".jpg";
                    var savePath = uploadpath + saveName;
                    img.Save(savePath);
                    var bits = picFile2bytes(savePath);
                    if (bits.Length / 1024 > 500)//大于0.5M
                    {
                        var _saveName = Encrypt.GenerateOrderNumber() + "_thumbnailUrl" + ".jpg";
                        var thumbnailUrl = uploadpath + _saveName;
                        var maxh = 400;
                        var maxw = 400;

                        if (img.Width > maxw)
                        {
                            maxh = 400 * img.Height / img.Width;
                        }

                        ImageManageHelper.GetPicThumbnail(savePath, thumbnailUrl, maxh, maxw, 90);
                        return Json(new { Success = true, SaveName = "/Content/UploadFiles/Mobile/" + _saveName });
                    }

                    return Json(new { Success = true, SaveName = "/Content/UploadFiles/Mobile/" + saveName });
                }
            }
            catch (Exception e)
            {
                return Json(e.Message);

            }
        }

        private string[] imgtypes = { ".bmp", ".png", ".gif", ".jpg", ".jpeg" };
        /// <summary>
        /// 核对图片
        /// </summary>
        /// <param name="file"></param>
        /// <returns></returns>
        private string CheckImg(HttpPostedFileBase file)
        {
            if (file == null) return "图片不能空！";
            var extension = Path.GetExtension(file.FileName);
            if (extension != null)
            {
                var image = extension.ToLower();
                return imgtypes.Contains(image) ? "ok" : "文件格式不对";
            }
            return "ok";
        }
        public ActionResult PraiseComment(int commentid)
        {
            var user = CurrentUser;
            var result = questionDbService.AddCommentPraise(user.Id, commentid);
            return Json(result);
        }

        public ActionResult PraiseQuestion(int questionid)
        {
            var user = CurrentUser;
            // var result = _questionService.AddQuestionPraise(user.Id, questionid);
            var exist = questionDbService.GetById(questionid);
            if (exist != null)
            {
                if (user.Id == exist.UserId && !PortalConfig.CanPraiseSelf)
                {
                    return Json(getaPortalResult(false, "不能赞自己啦", exist.PraisesNum));
                }
                var res = questionDbService.PraiseQuestion(user, exist);
                return Json(res);
            }
            return Json(new PortalResult("问题不存在"));
        }
        private PortalResult getaPortalResult(bool res, string meg, int mum)
        {
            return new PortalResult(res, meg) { Num = mum };
        }
        private User _currentUser;
        public User CurrentUser
        {
            get
            {
                return _currentUser ?? (_currentUser = _workContext.CurrentUser);
            }
            set { _currentUser = value; }
        }
    }
}
