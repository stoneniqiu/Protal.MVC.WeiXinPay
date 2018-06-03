using System;
using System.Collections.Generic;
using System.Linq;
using Niqiu.Core.Domain.Payments;
using WxPayAPI;

namespace Portal.MVC.WxPayAPI.business
{
    public class NativePay
    {
        /**
        * 生成扫描支付模式一URL
        * @param productId 商品ID
        * @return 模式一URL
        */
        public string GetPrePayUrl(string productId)
        {
            Log.Info(this.GetType().ToString(), "Native pay mode 1 url is producing...");

            WxPayData data = new WxPayData();
            data.SetValue("appid", WxPayConfig.APPID);//公众帐号id
            data.SetValue("mch_id", WxPayConfig.MCHID);//商户号
            data.SetValue("time_stamp", WxPayApi.GenerateTimeStamp());//时间戳
            data.SetValue("nonce_str", WxPayApi.GenerateNonceStr());//随机字符串
            data.SetValue("product_id", productId);//商品ID
            data.SetValue("sign", data.MakeSign());//签名
            string str = ToUrlParams(data.GetValues());//转换为URL串
            string url = "weixin://wxpay/bizpayurl?" + str;

            Log.Info(this.GetType().ToString(), "Get native pay mode 1 url : " + url);
            return url;
        }

        /**
        * 生成直接支付url，支付url有效期为2小时,模式二
        * @param productId 商品ID
        * @return 模式二URL
         * g公众账号id，商户号，随机字符串，签名，商品描述，商户订单号，总金额，终端ip，通知地址，交易类型
        */
        public string GetPayUrl(string productId)
        {
            Log.Info(this.GetType().ToString(), "Native pay mode 2 url is producing...");

            WxPayData data = new WxPayData();
            data.SetValue("appid", WxPayConfig.APPID);
            data.SetValue("mch_id", WxPayConfig.MCHID);
            data.SetValue("device_info", "iphone4s");
            data.SetValue("nonce_str", WxPayApi.GenerateNonceStr());
            data.SetValue("body", "好油菜充值");//商品描述
            data.SetValue("detail", "好油菜充值");//商品描述
            data.SetValue("attach", "北京好油菜");//附加数据
            data.SetValue("out_trade_no", WxPayApi.GenerateOutTradeNo());//随机字符串
            data.SetValue("total_fee", 1);//总金额
            data.SetValue("spbill_create_ip", "123.12.12.123");//总金额
            data.SetValue("time_start", DateTime.Now.ToString("yyyyMMddHHmmss"));//交易起始时间
            data.SetValue("time_expire", DateTime.Now.AddMinutes(10).ToString("yyyyMMddHHmmss"));//交易结束时间
            data.SetValue("goods_tag", "jjj");//商品标记
            data.SetValue("notify_url", "http://www.warmwood.com/");//通知地址
            data.SetValue("trade_type", "NATIVE");//交易类型
            data.SetValue("product_id", productId);//商品ID
          //  data.SetValue("limit_pay", productId);//指定支付方式
           // data.SetValue("openid", productId);//用户标识
            data.SetValue("sign", data.MakeSign());//签名

            WxPayData result = WxPayApi.UnifiedOrder(data);//调用统一下单接口
            string url = result.GetValue("code_url").ToString();//获得统一下单接口返回的二维码链接

            Log.Info(this.GetType().ToString(), "Get native pay mode 2 url : " + url);
            return url;
        }

        public string GetPayUrl(Order order,string ip)
        {
            if (order == null)
            {
                throw new ArgumentNullException("order");
            }
           
            WxPayData data = new WxPayData();
            data.SetValue("appid", WxPayConfig.APPID);
            data.SetValue("mch_id", WxPayConfig.MCHID);
            // data.SetValue("device_info", "iphone4s");
            data.SetValue("nonce_str", WxPayApi.GenerateNonceStr());
            data.SetValue("body", "好油菜充值");//商品描述
            data.SetValue("detail", "好油菜充值");//商品描述
            data.SetValue("attach", "北京好油菜");//附加数据
            data.SetValue("out_trade_no", order.TradeNumber);//随机字符串
           // data.SetValue("total_fee", Convert.ToInt32(order.OrderTotal * 100));//总金额
            data.SetValue("total_fee", (int)order.Amount*100);//总金额
           // data.SetValue("spbill_create_ip",ip);//总金额
            data.SetValue("time_start", DateTime.Now.ToString("yyyyMMddHHmmss"));//交易起始时间
            data.SetValue("time_expire", DateTime.Now.AddMinutes(30).ToString("yyyyMMddHHmmss"));//交易结束时间
            data.SetValue("goods_tag", "好油菜充值");//商品标记
            data.SetValue("notify_url", "http://www.yoursite.com/Checkout/ResultNotify");//通知地址
            data.SetValue("trade_type", "NATIVE");//交易类型
            data.SetValue("product_id", 121);//商品ID  
            data.SetValue("sign", data.MakeSign());//签名
            //Logger.Info("获得签名" + data.GetValue("sign"));

            WxPayData result = WxPayApi.UnifiedOrder(data);//调用统一下单接口
            //Logger.Info(result.ToJson());
            string url = result.GetValue("code_url").ToString();//获得统一下单接口返回的二维码链接
            //Logger.Info("pay url:" + url);
            return url;
        }

        /**
        * 参数数组转换为url格式
        * @param map 参数名与参数值的映射表
        * @return URL字符串
        */
        private string ToUrlParams(SortedDictionary<string, object> map)
        {
            string buff = "";
            foreach (KeyValuePair<string, object> pair in map)
            {
                buff += pair.Key + "=" + pair.Value + "&";
            }
            buff = buff.Trim('&');
            return buff;
        }
    }
}