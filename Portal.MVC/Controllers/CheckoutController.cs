using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Text;
using System.Web;
using System.Web.Mvc;
using Niqiu.Core.Domain;
using Niqiu.Core.Domain.Common;
using Niqiu.Core.Domain.Payments;
using Niqiu.Core.Domain.User;
using Niqiu.Core.Services;
using Portal.MVC.Attributes;
using Portal.MVC.Models.Services;
using Portal.MVC.WxPayAPI.business;
using ThoughtWorks.QRCode.Codec;
using WxPayAPI;

namespace Portal.MVC.Controllers
{
    
    [MenuStatistics]
    public class CheckoutController : Controller
    {
        private readonly IWorkContext _workContext;
        private readonly IWebHelper _webHelper;

        public CheckoutController(IWorkContext workContext, IWebHelper webHelper)
        {
            _workContext = workContext;
            _webHelper = webHelper;
        }

        private PayMeentDbService _payMeentDbService = new PayMeentDbService();
        private UserDbService _userDbService = new UserDbService();

        public ActionResult Index()
        {
            return RedirectToAction("Payment");
        }


        //公众号支付
        [LoginValid]
        public ActionResult H5Pay(string orederId)
        {
            var user = _workContext.CurrentUser;
            var order = _payMeentDbService.GetOrderByOrderNumber(orederId);
            //判断订单是否存在
            //订单是否已经支付了
            var openid = user.OpenId;
            var jsApipay = new JsApiPayMvc(this.ControllerContext.HttpContext);
            jsApipay.openid = openid;
            jsApipay.total_fee = (int)(order.Amount * 100);
            WxPayData unifiedOrderResult = jsApipay.GetUnifiedOrderResult("谜题红包");
            ViewBag.wxJsApiParam = jsApipay.GetJsApiParameters();//获取H5调起JS API参数     
            ViewBag.unifiedOrder = unifiedOrderResult.ToPrintStr();
            ViewBag.OrderNumber = order.OrderNumber;
            return View();
        }

        [LoginValid]
        public ActionResult H5PayJson(string orederId)
        {
            var stop = new Stopwatch();
            stop.Start();
            var user = _workContext.CurrentUser;
            var order = _payMeentDbService.GetOrderByOrderNumber(orederId);
            Logger.Debug("获取订单耗时"+stop.Elapsed);
            stop.Restart();
            var strkey = "谜题红包";
            if (order.OrderType == OrderType.QuestionStrategy)
            {
                strkey = "购买提示";
            }
            //判断订单是否存在
            //订单是否已经支付了
            var openid = user.OpenId;
            var jsApipay = new JsApiPayMvc(ControllerContext.HttpContext)
            {
                openid = openid,
                total_fee = (int)(order.Amount*100)
            };
            try
            {
                jsApipay.GetUnifiedOrderResult(strkey);
               // order.TradeNumber = jsApipay.TradeNumber;
                //_payMeentDbService.UpdateOrder(order);
                Logger.Debug("获取参数耗时" + stop.Elapsed);
                return Json(jsApipay.GetJsApiParameters());
            }
            catch (Exception e)
            {
                //统一下单失败
                return Json(new PortalResult(false, e.Message));
            }
        }

        [LoginValid]
        public ActionResult CashTransfers(string orderNumber)
        {
            //var order = new Order(){Amount = 1};
           // var openid = "oBSBmwQjqwjfzQlKsFNjxFLSixxx";

            var user = _workContext.CurrentUser;
            var order = _payMeentDbService.GetOrderByOrderNumber(orderNumber);
            if (string.IsNullOrEmpty(user.OpenId))
            {
                return Json(new PortalResult("请用微信登录!"));
            }
            if (order == null || order.OrderState != OrderState.Padding)
            {
                return Json(new PortalResult("订单有误!"));
            }
            
            var transfer = new TransfersPay
            {
                openid = user.OpenId,
                amount = (int) (order.Amount*100),
                partner_trade_no = order.OrderNumber,
                re_user_name = user.Username,
                spbill_create_ip = _webHelper.GetCurrentIpAddress()
            };
            var data = transfer.GetTransfersApiParameters();
            var result = WxPayApi.Transfers(data);
            if (result.GetValue("result_code").ToString() == "SUCCESS")
            {
                return Json(new PortalResult(true, "提现成功"));
            }
            return Json(new PortalResult(false, result.GetValue("return_msg").ToString()));

            return Content(result.ToPrintStr());
        }

        private  ActionResult PayBackAllMoney()
        {
            var users = _userDbService.GetAllUsers();
            foreach (var user in users)
            {
                var wallet = _payMeentDbService.GetByUserId(user.Id);
                if (wallet != null)
                {
                    var money = wallet.Balance + wallet.LockMoney;
                    paymoney(user, wallet, money);

                }
            }
            return Content("处理完毕");
        }



        private void paymoney(User user,Wallet wallet,decimal money)
        {
            if(money<1) return;
            var order = new Order();
            order.Amount = money;
            var transfer = new TransfersPay
            {
                openid = user.OpenId,
                amount = (int)(order.Amount * 100),
                partner_trade_no = order.OrderNumber,
                re_user_name = user.Username,
                spbill_create_ip = _webHelper.GetCurrentIpAddress()
            };
            var data = transfer.GetTransfersApiParameters();
            var result = WxPayApi.Transfers(data);
            if (result.GetValue("result_code").ToString() == "SUCCESS")
            {
                wallet.Balance = 0;
                wallet.LockMoney = 0;
                _payMeentDbService.UpdateWallet(wallet);
            }
        }


        public ActionResult Redpack()
        {
            var order = new Order(WxPayConfig.MCHID) { Amount = 4 };
            var openid = "oBSBmwQjqwjfzQlKsFNjxFLSiIQM";
            var paymodel = new RedpackPay()
            {
                total_amount = (int) order.Amount*100,
                send_name = "stoneniqiu",
                re_openid = openid,
                total_num = 1,
                client_ip = _webHelper.GetCurrentIpAddress(),
                wishing = "2017 新春大吉!",
                remark = "猜越多得越多，快来抢！",
                act_name = "猜灯谜抢红包活动",
                mch_billno = order.OrderNumber
            };
            var data = paymodel.GetParameters();
            var result = WxPayApi.Sendredpack(data);
            return Content(result.ToPrintStr());
        }

      

        //扫码支付
        public ActionResult Payment(int orederId)
        {
            var order = _payMeentDbService.GetOrderById(orederId);
            if (string.IsNullOrEmpty(order.TradeNumber))
            {
                order.TradeNumber = WxPayApi.GenerateOutTradeNo();
            }
            if (order.OrderState == OrderState.Success)
            {
                //Success("已经支付");
                return RedirectToAction("PaidSuccess", new { tradeno = order.TradeNumber });
            }
            var user = new User(){UserGuid = Guid.NewGuid(),Username = "stoneniqiu"};
            NativePay nativePay = new NativePay();
            string url2 = nativePay.GetPayUrl(order, user.LastIpAddress);
            ViewBag.QRCode = "/Checkout/MakeQRCode?data=" + HttpUtility.UrlEncode(url2);
            ViewBag.Order = order;

            return View();
        }

        public WxPayData unifiedOrderResult { get; set; }
        public JsonResult GetUnifiedOrderJson()
        {
            //统一下单
            WxPayData data = new WxPayData();
            data.SetValue("body", "test");
            data.SetValue("attach", "test");
            data.SetValue("out_trade_no", WxPayApi.GenerateOutTradeNo());
            data.SetValue("total_fee", 1);
            data.SetValue("time_start", DateTime.Now.ToString("yyyyMMddHHmmss"));
            data.SetValue("time_expire", DateTime.Now.AddMinutes(10).ToString("yyyyMMddHHmmss"));
            data.SetValue("goods_tag", "test");
            data.SetValue("trade_type", "JSAPI");
           // data.SetValue("openid", openid);

            WxPayData result = WxPayApi.UnifiedOrder(data);
            if (!result.IsSet("appid") || !result.IsSet("prepay_id") || result.GetValue("prepay_id").ToString() == "")
            {
                Log.Error(this.GetType().ToString(), "UnifiedOrder response error!");
                throw new WxPayException("UnifiedOrder response error!");
            }

            unifiedOrderResult = result;
            return Json(result,JsonRequestBehavior.AllowGet);
        }

      

        public ActionResult PaidSuccess(string tradeno)
        {
           // var order = _orderService.GetOrderByTradeNumber(tradeno);
            var order = new Order() {TradeNumber = tradeno};
            return View(order);
        }

        public FileResult MakeQRCode(string data)
        {
            if (string.IsNullOrEmpty(data))
                throw new ArgumentException("data");

            //初始化二维码生成工具
            QRCodeEncoder qrCodeEncoder = new QRCodeEncoder();
            qrCodeEncoder.QRCodeEncodeMode = QRCodeEncoder.ENCODE_MODE.BYTE;
            qrCodeEncoder.QRCodeErrorCorrect = QRCodeEncoder.ERROR_CORRECTION.M;
            qrCodeEncoder.QRCodeVersion = 0;
            qrCodeEncoder.QRCodeScale = 4;

            //将字符串生成二维码图片
            Bitmap image = qrCodeEncoder.Encode(data, Encoding.Default);

            //保存为PNG到内存流  
            MemoryStream ms = new MemoryStream();
            image.Save(ms, ImageFormat.Jpeg);

            return File(ms.ToArray(), "image/jpeg");
        }

        public ActionResult ResultNotify()
        {
            //接收从微信后台POST过来的数据
            Stream s = Request.InputStream;
            int count = 0;
            byte[] buffer = new byte[1024];
            StringBuilder builder = new StringBuilder();
            while ((count = s.Read(buffer, 0, 1024)) > 0)
            {
                builder.Append(Encoding.UTF8.GetString(buffer, 0, count));
            }
            s.Flush();
            s.Close();
            s.Dispose();

            //Logger.Info(this.GetType() + "Receive data from WeChat : " + builder);
            //转换数据格式并验证签名
            WxPayData data = new WxPayData();
            try
            {
                data.FromXml(builder.ToString());
            }
            catch (WxPayException ex)
            {
                //若签名错误，则立即返回结果给微信支付后台
                WxPayData res = new WxPayData();
                res.SetValue("return_code", "FAIL");
                res.SetValue("return_msg", ex.Message);
                Log.Error(this.GetType().ToString(), "Sign check error : " + res.ToXml());
                Response.Write(res.ToXml());
                Response.End();
            }
            //Logger.Info(this.GetType() + "Check sign success");


            ProcessNotify(data);

            return View();
        }

        public void ProcessNotify(WxPayData data)
        {
            WxPayData notifyData = data;

            //检查支付结果中transaction_id是否存在
            if (!notifyData.IsSet("transaction_id"))
            {
                //若transaction_id不存在，则立即返回结果给微信支付后台
                WxPayData res = new WxPayData();
                res.SetValue("return_code", "FAIL");
                res.SetValue("return_msg", "支付结果中微信订单号不存在");
                //Logger.Error(this.GetType() + "The Pay result is error : " + res.ToXml());
                Response.Write(res.ToXml());
                Response.End();
            }

            string transaction_id = notifyData.GetValue("transaction_id").ToString();

            //查询订单，判断订单真实性
            if (!QueryOrder(transaction_id))
            {
                //若订单查询失败，则立即返回结果给微信支付后台
                WxPayData res = new WxPayData();
                res.SetValue("return_code", "FAIL");
                res.SetValue("return_msg", "订单查询失败");
                //Logger.Error(this.GetType() + "Order query failure : " + res.ToXml());
                SetPaymentResult(data.GetValue("out_trade_no").ToString(), OrderState.Canceled);
                Response.Write(res.ToXml());
                Response.End();
            }
            //查询订单成功
            else
            {
                WxPayData res = new WxPayData();
                res.SetValue("return_code", "SUCCESS");
                res.SetValue("return_msg", "OK");
                //Logger.Info(this.GetType() + "order query success : " + res.ToXml());
                SetPaymentResult(data.GetValue("out_trade_no").ToString(), OrderState.Success);
                Response.Write(res.ToXml());
                Response.End();
            }
        }
        //查询订单
        private bool QueryOrder(string transaction_id)
        {
            WxPayData req = new WxPayData();
            req.SetValue("transaction_id", transaction_id);
            WxPayData res = WxPayApi.OrderQuery(req);
            if (res.GetValue("return_code").ToString() == "SUCCESS" &&
                res.GetValue("result_code").ToString() == "SUCCESS")
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// 设置支付状态
        /// </summary>
        /// <param name="tradeno"></param>
        /// <param name="status"></param>
        public void SetPaymentResult(string tradeno, OrderState status)
        {
            Logger.Info("订单号:" + tradeno);
            //var order = _orderService.GetOrderByTradeNumber(tradeno);
            //if (order != null)
            //{
            //    order.PaymentStatus = status;
            //    if (status == PaymentStatus.Paid)
            //    {
            //        order.PaidDate = DateTime.Now;
            //    }
            //    _orderService.UpdateOrder(order);
            //    //Logger.Info("订单：" + tradeno + "成功更新状态为" + status);
            //}
        }

    }
}
