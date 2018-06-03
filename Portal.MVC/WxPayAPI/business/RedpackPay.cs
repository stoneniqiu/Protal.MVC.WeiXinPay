using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using WxPayAPI;

namespace Portal.MVC.WxPayAPI.business
{
    public class RedpackPay
    {
        //商户订单号
        public string mch_billno { get; set; }

        //商户名称
        public string send_name { get; set; }
        //接受红包的用户 
        //用户在wxappid下的openid
        public string re_openid { get; set; }
        //红包发放总人数 
        public int total_num { get; set; }

        //感谢您参加猜灯谜活动，祝您元宵节快乐！
        public string wishing { get; set; }
        //ip
        public string client_ip { get; set; }

        //活动名称
        public string act_name { get; set; }

        public string remark { get; set; }
        //金额
        public int total_amount { get; set; }
        //场景id
        // 发放红包使用场景，红包金额大于200时必传
        //PRODUCT_1:商品促销 
        //PRODUCT_2:抽奖 
        //PRODUCT_3:虚拟物品兑奖  
        //PRODUCT_4:企业内部福利 
        //PRODUCT_5:渠道分润 
        //PRODUCT_6:保险回馈 
        //PRODUCT_7:彩票派奖 
        //PRODUCT_8:税务刮奖
        public string scene_id { get; set; }

        public WxPayData GetParameters()
        {
            WxPayData apiParam = new WxPayData();
            apiParam.SetValue("mch_billno", mch_billno);
            apiParam.SetValue("send_name", send_name);
            apiParam.SetValue("re_openid", re_openid);
            apiParam.SetValue("total_amount", total_amount);
            apiParam.SetValue("total_num", total_num);
            apiParam.SetValue("client_ip", client_ip);
            apiParam.SetValue("remark", remark);
            apiParam.SetValue("act_name", act_name);
            return apiParam;
        }
    }
}