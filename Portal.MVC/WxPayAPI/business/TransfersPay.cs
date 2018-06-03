using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using WxPayAPI;

namespace Portal.MVC.WxPayAPI.business
{
    public class TransfersPay
    {
        public string openid { get; set; }

        public int amount { get; set; }

        public string partner_trade_no { get; set; }

        public string re_user_name { get; set; }

        public string spbill_create_ip { get; set; }


        public WxPayData GetTransfersApiParameters()
        {
            WxPayData apiParam = new WxPayData();
            apiParam.SetValue("partner_trade_no", partner_trade_no);
            apiParam.SetValue("openid", openid);
            apiParam.SetValue("check_name", "NO_CHECK");
            apiParam.SetValue("amount", amount);
            apiParam.SetValue("desc", "提现");
            apiParam.SetValue("spbill_create_ip", spbill_create_ip);
            apiParam.SetValue("re_user_name", re_user_name);

            return apiParam;
        }
    }
}