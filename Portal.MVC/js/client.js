function jsApiCall(json, success, fail) {
    WeixinJSBridge.invoke(
        'getBrandWCPayRequest',
        json,//josn串
        function (res) {
            WeixinJSBridge.log(res.err_msg);
            //alert(res.err_code + res.err_desc + res.err_msg);
            if (res.err_msg == "get_brand_wcpay_request:ok") {
                //充值进去 要区分是出题充值 还是购买悬赏 前者冲到他的钱包
                //后者直接冲到系统账户
                if (success) success();
            }
            if (res.err_msg == 'get_brand_wcpay_request:cancel') {
                // alert('取消支付');
                if (fail) fail();
            }
        }
    );
}

function callpay(json, success, fail) {
    if (typeof WeixinJSBridge == "undefined") {
        alert("请在微信中打开!");
        if (document.addEventListener) {
            document.addEventListener('WeixinJSBridgeReady', jsApiCall, false);
        }
        else if (document.attachEvent) {
            document.attachEvent('WeixinJSBridgeReady', jsApiCall);
            document.attachEvent('onWeixinJSBridgeReady', jsApiCall);
        }
    }
    else {
        $.hidePreloader();
        jsApiCall(json, success, fail);
    }
}
//下单之前要检测是否可以下单，不然支付了又不能创建
function createOrderAndPay(money, success, faill) {
    //微信支付  创建谜题
    $.showPreloader("正在唤起微信支付");
    $.post('/Payment/CreateRecharegOrder', { money: money }, function (orderdata) {
        console.log(orderdata);
        if (orderdata.IsSuccess === true) {
            var orderId = orderdata.OrderId;
            $.post("/Checkout/H5PayJson", { orederId: orderId }, function (jsondata) {
                if (jsondata.IsSuccess === false) {
                    $.toast(jsondata.Message);
                    return;
                }
                var jdata = JSON.parse(jsondata);
                if (jdata.appId) {
                    callpay(jdata, function () {
                        $.post("/payment/WeiXinPaySuccess", { ordernumber: orderId }, function (paymentdata) {
                            //确定回调执行
                            success();
                            //支付订单和日志总是有日志的，如果出错会中断掉谜题的创建
                            if (paymentdata.IsSuccess === true) {
                            } else {
                                $.alert(paymentdata.Message);
                            }
                        });
                    }, function () {
                        // $.alert("你已取消支付!");
                    });
                } else {
                    alert("统一下单失败!");
                }
            });

            return;
        }
        $.toast(orderdata.Error);
        $.hidePreloader();
    });
}

var client = {
    createQuestion: function () {
        //绑定事件：
        $(document).on("click", "#cqs1", function () {
            var question = $.trim($("#question").val());
            if (question == "") {
                $.alert('请输入你的问题');
                return;
            }
            if (question.length < 5) {
                $.alert('问题不能少于五个字');
                return;
            }
            var src = $(".loadimg img").attr("src");
            if (src == "/images/shadow.png") {
                $.alert('请上传图片!');
                return;
            }
            $.post("/Question/CreateStep1", { ImageUrl: src, Title: question }, function () {
                location.href = "/Question/CreateStep2";
            });
        });

        $(document).on("click", ".ctxqbox .ctxqxs", function () {
            var key = $(this).data("key");
            if (key == 1) {
                $("#peoplenum").hide();
                $("#renshu").html("悬赏1人");
            } else {
                $("#peoplenum").show();
                $("#renshu").html("悬赏人数");
            }
        });

        //选择悬赏金额
        $(document).on("click", ".ctjebl", function () {
            $(this).siblings().find("a").removeClass("ctjecur");
            $(this).find("a").addClass("ctjecur");
            $(".ctjeqt p").show();
            $(".ctqttxt").hide();
        });
        //金额问题
        $(document).on("click", ".ctjeqt p", function () {
            $(this).hide();
            $(".ctqttxt").show();
            $(".ctjebl a").removeClass("ctjecur");
        });
        //悬赏人数
        $(document).on("click", ".tsbll", function () {

            if ($("#onemoney").hasClass("ctjecur")) {
                var fb = $(".tsbll").eq(0);
                //$.toast("金额太少,人数超过限制");
                fb.find("a span").removeClass("none");
                fb.siblings().find("a span").addClass("none");
                fb.addClass("selected");
                fb.siblings().removeClass("selected");
                return;
            } else {
                if ($("#money").val() <= 1 && !$(".ctjebl a").hasClass("ctjecur")) {
                    $.toast("金额太少,人数超过限制");
                    var fb = $(".tsbll").eq(0);

                    fb.find("a span").removeClass("none");
                    fb.siblings().find("a span").addClass("none");
                    fb.addClass("selected");
                    fb.siblings().removeClass("selected");
                    return;
                }
            }

            $(this).find("a span").removeClass("none");
            $(this).siblings().find("a span").addClass("none");
            $(this).addClass("selected");
            $(this).siblings().removeClass("selected");
        });
        //悬赏方式
        $(document).on("click", ".ctxqxs", function () {

            $(this).addClass("xqxscur");
            $(this).siblings().removeClass("xqxscur");
        });
        //竞猜时间
        $(document).on("click", ".ctxqsj", function () {
            $(this).find("a span").removeClass("none");
            $(this).siblings().find("a span").addClass("none");
            $(this).addClass("selected");
            $(this).siblings().removeClass("selected");
        });



        $(document).on("click", "#cqs2", function () {

            //if (!$(this).hasClass("active")) {
            //    $.toast("请输入谜题和答案");
            //    return;
            //}
            var question = $.trim($("#question").val());
            if (question == "") {
                $.alert('请输入你的问题');
                return;
            }
            if (question.length < 5) {
                $.alert('问题不能少于五个字');
                return;
            }
            var anwser = $("#anwser").val();
            if (anwser == "") {
                $.toast('请输入你的答案');
                return;
            }
            var info = $("#info").html();
            //if (info == "") {
            //    $.toast('请输入提示');
            //    return;
            //}
            var button = $(".ctxqxs.xqxscur");
            if (button.length == 0) {
                $.toast('请选择悬赏方式');
                return;
            }
            var peoplenum = $(".tsbll.selected>span").html();
            if (button.data("key") == 1) {
                peoplenum = 1;
            }
            if ($("#ziinput").css("display") == "inline-block") {
                var inputNum = parseInt($("#ziinput").val());
                if (inputNum > 1000) {
                     $.toast('不能超过1000人!');
                    return;
                }

                if (inputNum < 10) {
                    $.toast('不能小于10人!');
                    return;
                }
                if (isNaN(inputNum)) {
                    $.toast('请输入人数!');
                    return;
                }
                peoplenum = inputNum;
            }

            var money = 0;
            if ($(".ctjebl .ctjecur").length > 0) {
                money = $(".ctjebl .ctjecur").html();
            } else {
                money = parseFloat($("#money").val());
            }
            if (parseFloat(money) < 1) {
                $.toast('悬赏金额不能小于1元钱');
                return;
            }
            var mstr = money.toString();
            if (~mstr.indexOf('.')) {
                $.toast('悬赏金额请输入整数!');
                return;
            }

            if (parseFloat(money) > 999) {
                $.toast('悬赏金额不能大于999元');
                return;
            }

            if (peoplenum * 0.1 > money) {
                $.toast('平均每人不能小于0.1元!');
                return;
            }

            var type = button.data("key");
            var img = $("#ImageUrl").val();
            var time = $(".ctxqsj.selected").data("key");
            var id = $("#Id").val();
            var rr = $("#RemanidReward").val();
            $.post("/Question/CreateStep2", { Id: id, RemanidReward: rr, Title: question, ImageUrl: img, Answer: anwser, Tip: info, RewardType: type, RewardPeopleNum: peoplenum, Reward: money, ExpireHours: time },
                function (res) {
                    if (res.IsSuccess === true) {
                        location.href = "/Question/CreateStep3";
                    } else {
                        $.toast(res.Message);
                    }
                });

        });

        $(document).on("click", ".zfbtn", function () {
            var money = $(this).data("money");
            var yue = $(this).data("yue");
            //// $(this).attr("disabled", "disabled");
            // var bt = $(this);
            // console.log("disabled");
            // $.post("/Question/CheckMoney", { reward: money }, function (data) {
            //     if (data === true) {
            //         console.log(data);
            //         submitQuestion();
            //         //余额够支付;
            //     } else {
            //         //微信支付  创建谜题
            //         createOrderAndPay(money, submitQuestion);
            //       // bt.removeAttr("disabled");
            //         console.log("remove disabled");
            //     }
            // });
            payType = 1;
            selectPay(yue, money);

        });
    },

    searchQuestion: function () {
        $(document).on("click", ".sitem li", function () {
            var key = $(this).data("key");
            $(this).addClass("cur").siblings().removeClass("cur");
            $.showPreloader();
            if (key == "isright") {
                $.post('/Home/QuestionesList', { isright: false }, function (view) {
                    $(".content").html(view);
                    $.hidePreloader();
                    $(".sitem ul").hide();
                });
            } else {
                $.post('/Home/QuestionesList', { type: key }, function (view) {
                    $(".content").html(view);
                    $.hidePreloader();
                    $(".sitem ul").hide();
                });
            }
        });
        //我的问题 查询
        //button
        $(document).on("click", "#mysul li", function () {
            var key = $(this).data("key");
            $(this).addClass("cur").siblings().removeClass("cur");
            $.showIndicator();
            $.post("/UserCenter/MyQuestionsPage", { hasrewared: key }, function (view) {
                $("#mypage").html(view);
                $.hideIndicator();
            });
        });
        $(document).on("click", "#joinul li", function () {
            var key = $(this).data("key");
            $(this).addClass("cur").siblings().removeClass("cur");
            $.showIndicator();
            $.post("/UserCenter/JoinQuestionPage", { isreward: key }, function (view) {
                $("#mypage").html(view);
                $.hideIndicator();
            });
        });
        $(document).on("click", ".wdbtm .wdbtn", function () {
            $(".wdbtmbl").toggle();
            $("#down,#up").toggle();
            $.showIndicator();
            $.post("/UserCenter/AllQuestions", function (view) {
                $("#mypage").html(view);
                $.hideIndicator();
            });
        });

        $(document).on("click", ".wdbtmbl a", function () {
            var txt = $(this).html();
            $(".wdbtn span").html(txt);
            $(".wdbtmbl").hide();
            $(this).addClass("cur").siblings("a").removeClass("cur");
            var id = $(this)[0].id;
            $("#up").show();
            $("#down").hide();
            if (id == "allq") {
                $("#myquestions .tab").hide();
            }
            if (id == "myq") {
                console.log(id);
                $("#myquestions #mysul").show();
                $("#myquestions #joinul").hide();
                $.showIndicator();
                $.post("/UserCenter/MyQuestionsPage", function (view) {
                    $("#mypage").html(view);
                    $.hideIndicator();
                });
            }
            if (id == "joinq") {
                $("#myquestions #mysul").hide();
                $("#myquestions #joinul").show();
                $.showIndicator();
                $.post("/UserCenter/JoinQuestionPage", function (view) {
                    $("#mypage").html(view);
                    $.hideIndicator();
                });
            }
        });

    },
    //购买策略
    buystrategy: function () {
        $(document).on("click", ".tsbl", function () {
            $(this).find("a span.none").removeClass("none");
            $(this).find(".tsblr .num").addClass("tscur");
            $(this).addClass("selected").siblings().removeClass("selected");
            $(this).siblings().find("a span").addClass("none");
            $(this).siblings().find(".tsblr .num").removeClass("tscur");
        });
        $(document).on("click", ".tszfbl", function () {
            $(this).find("a span.none").removeClass("none");
            $(this).addClass("selected").siblings().removeClass("selected");
            $(this).siblings().find("a span").addClass("none");
        });
        $(document).on("click", ".gltop a", function () {
            $(".glbox").hide();
        });
        $(document).on("click", ".tszfbl", function () {
            $(".tszffs").hide();
            $(".tszh").show();
        });

        //确认购买
        $(document).on("click", "#strategybt", function () {
            //$(".glbox.tszffs").show();
            //检查余额
            var money = $(".tsbl.selected").data("price");
            var yue = $(".tsbl.selected").data("b");
            var empty = $(this).data("e");
            var id = $(this).data("q");
            if (empty == "True" && yue > money) {
                location.href = "/UserCenter/SetPayPassword?returnUrl=/Home/BuyInfo?questionId=" + id;
            }

            selectPay(yue, money);
            payType = 0;
            //$.post("/Question/CheckMoney", { reward: money }, function (data) {
            //    if (data === true) {
            //        //余额够支付;弹出支付密码
            //        console.log(data);
            //        $(".tszffs").hide();
            //        $(".tszh").show();
            //    } else {
            //        //微信支付  创建谜题
            //        createOrderAndPay(money, _buystargety);
            //    }
            //});

        });

        //确定支付
        $(document).on("click", "#makeuserpay", function () {
            var password = $(".glnumbl input").val();
            if (!password || password.length < 6) {
                $.toast("请输入完整的密码");
                return;
            }
            console.log(password);
            $.showIndicator();
            $.post('/UserCenter/CheckPaymentPassword', { pwd: password }, function (data) {
                $.hideIndicator();
                if (data === true) {
                    var ckradio = $(".tsbl.selected");
                    //输入密码后，触发以下的内容
                    var blance = ckradio.data("b");
                    var price = ckradio.data("price");
                    console.log('blance', blance, 'price', price);
                    if (blance < price) {
                        $.confirm('余额不足请充值!', function () {
                            $.router.load("/UserCenter/Recharge");
                        }, function () {
                            console.log("取消了");
                        });
                        return;
                    }
                    $.modal({
                        title: "支付",
                        text: "购买提示 <h3>" + price + "￥</h3>",
                        afterText: "账户余额" + blance + "￥",
                        buttons: [
                            {
                                text: '确定支付',
                                onClick: function () {
                                    _buystargety();
                                }
                            }
                        ]
                    });
                } else {
                    $(".glnumbl input").val("");
                    $.alert("密码错误");
                }
            });
        });

    },
    isWeiXin: function () {
        return /micromessenger/i.test(navigator.userAgent);
    },

    recharge: function () {
        function _recharge(m) {
            $.confirm('确定充值"' + m + '"元?', function () {
                ///Payment/RechargeAction
                $.post('/Payment/CreateRecharegOrder', { money: m }, function (data) {
                    console.log(data);
                    if (data.IsSuccess === true) {
                        var orderId = data.OrderId;
                        if (client.isWeiXin()) {
                            //微信里面
                            location.href = "/Checkout/H5Pay?orederId=" + orderId;
                        } else {
                            //非微信浏览器
                            location.href = "/Checkout/H5Pay?orederId=" + orderId;
                        }
                        return;
                    }
                    $.toast(data.Error);
                });
            });
        }

        $(document).on("click", ".moneybox", function () {
            var money = $(this).data("key");
            if (money != 0) {
                _recharge(money);
            }
        });
        $(document).on("click", "#rechargebt,#othermoney", function () {
            var money = $("#rechargeval").val();
            if ($(this)[0].id == "othermoney") {
                money = 0;
            }
            if (money == 0) {
                $.prompt('请输入你要充值的金额?', function (value) {

                    var reg = /^[1-9]{1}\d*(\.\d{1,2})?$/;
                    var result = reg.test(value);
                    if (result) {
                        _recharge(value);
                    } else {
                        var min = parseFloat(value);
                        if (min < 1) {
                            $.toast("充值金额最小不少于1元!");
                        } else {
                            $.toast("请输入正确的金额!");
                        }
                    }
                });
            } else {
                _recharge(money);
            }
        });


    },
    islogin: function () {
        return $("#islogin").val() == "True";
    },
    _loginAlert: function (url) {
        $.alert("请先登录", function () {
            location.href = url || "/Account/Loadding?returnUrl=" + document.location.href;
        });
    },
    loginAlert: function (url) {
        //$.alert("请先登录", function () {
        //    location.href = url || "/Account/Loadding?returnUrl=" + document.location.href;
        //});
    },
    answerQuestion: function () {
        function answerSuccess(name, money) {
            name = name || "stoneniqiu";
            money = money || 1;
            $(".reward-info .from span").html(name);
            $(".reward-info .amount span").html(money);
            $.popup(".popup-turn");
            tools.editeWxTitle("好油菜·回答正确");
        }
        function answerError() {
            $.popup(".popup-errorAnwser");
            tools.editeWxTitle("好油菜·答案错误");
        }

        function getRecord(qid) {
            $.post("/Question/RecoderView", { id: qid }, function (v) {
                $("#tab3").html(v);
                var count = $("#tab3 .hjbox").length;
                $(".mytabs a").eq(0).html("记录<span>" + count + "</span>");
            });
        }

        $(document).on("click", ".resendThisQuestion", function () {
            if ($(this).hasClass("die")) return;
            location.href = $(this).data("href");

        });
        $(document).on("click", ".zhye-green.jixu", function () {
            $.closeModal(".popup-errorAnwser");
        });
        $(document).on("click", ".doanswer", function () {
            if ($(this).hasClass("die")) return;
            $(".anwserquestion").removeClass("none").show();
            $("#anwsertxt").focus();
        });
        $(document).on("click", ".buyTag", function () {
            if ($(this).hasClass("die")) return;
            location.href = $(this).data("href");
        });
        //questione card
        $(document).on("click", ".questione.card", function () {
           $(".anwserquestion").hide();
        });

        $(document).on("click", "#answerQuestionbt", function () {
            // 是否登录？
            var qid = $(this).data("key");
            var url = "/Account/Loadding?returnUrl=/Home/Detail/" + qid;
            var name = $(this).data("name");
            var bt = $(this);
            $.post('/Home/HasLogin', function (res) {
                if (res === true) {
                    var answer = $("#anwsertxt").val();
                    //禁止button
                    bt.attr("disabled", "disabled");
                    $.post('/Question/Answer', { quesitonId: qid, answer: answer }, function (data) {
                        console.log(data);
                        setTimeout(function () {
                            bt.removeAttr("disabled");
                        }, 2000);
                        if (data.IsSuccess === undefined) {
                            client.loginAlert(url);
                        }
                        if (data.IsSuccess === true) {
                            //回答正确
                            $("#anwser-input").val("");
                            $.showPreloader(data.Message + " 正在领取悬赏。");
                            $.post('/Payment/PayReward', { questionId: qid }, function (res) {
                                $.hidePreloader();
                                if (res.IsSuccess === true) {
                                    answerSuccess(name, res.Money);
                                } else {
                                    $.toast(res.Error);
                                    //也要加载记录
                                    getRecord(qid);
                                }
                            });
                            //支付
                        } else {
                            // 不具有回答资格或者系统错误了
                            if (data.Message != "对不起，您的回答错误") {
                                $.toast(data.Message);
                            } else {
                                //正经回答错误
                                $("dtzq p").html(data.Message);
                                answerError();
                            }
                            getRecord(qid);
                        }
                        $(".anwserquestion").addClass("none");
                    });

                } else {
                    client._loginAlert(url);
                }
            });


            //if (!client.islogin()) {
            //    client.loginAlert(url);
            //}
        });

        $(document).on("click", ".open-report", function () {
            var bt = $(this);
            $.post('/Home/HasLogin', function (res) {
                if (res === true) {
                    $(".xqjbti span").html(bt.data("user"));
                    $(".xqjbti samp").html(bt.data("type"));
                    $(".popup-report").attr("data-id", bt.data("id"));
                    $(".popup-report").attr("data-relateType", bt.data("type"));
                    $.popup('.popup-report');
                } else {
                    client._loginAlert();
                }
            });
        });
        //举报框控制
        $(document).on('click', '.jbtabli', function () {
            $('.jbtabli').removeClass("selected");
            $(this).addClass("selected");
        });

        $(document).on("click", ".xqjbbtn", function () {
            var title = $(".xqjbti").html().replace(/<[^>]+>/g, "");
            var content = $(".qtitle").html() + $(".questione .card-content-inner>a").html();
            var type = $(".jbtabli.selected").data("key");
            var reid = $(".popup-report").data("id");
            var relateType = $(".popup-report").data("relatetype") == "谜题" ? 0 : 1;
            console.log(title, "内容", content, "举报类型", type, "相关id", reid, "相关类型", relateType);
            $.post("/Home/InsertReport", { title: title, content: content, type: type, id: reid, relateType: relateType }, function (json) {
                if (json.IsSuccess === true) {
                    $.toast(json.Message);
                } else {
                    $.toast(json.Message);
                }
                $.closeModal(".popup-report");
            });
        });
        //公布答案
        $(document).on("click", "#showAnswer", function () {
            var id = $(this).data("key");
            $.post("/Question/FiniQuestion", { id: id }, function (data) {
                if (data === true) {
                    $.toast("公布成功");
                    setTimeout(function () {
                        Location.reload();
                    }, 1000);
                } else {
                    $.toast("公布失败");
                }
            });
        });
    },
    activeTab: function (index) {
        $("#footertabs a").eq(index).addClass("active").siblings().removeClass("active");

    },// "miti": 0, "detail": 0, 
    footerTabs: {"chuti1": 0, "chuti2": 0, "usercenter": 4, "userdetail": 4, "portrait": 4, "nikename": 4, "mobile": 4, "classical": 3, "firends": 3, "firendsNew": 3, "messages": 4, "chat": 4 },
    attentionUser: function () {
        $(document).on("click", "#attentionbt", function () {
            var userid = $(this).data("key");
            $.post('/Home/HasLogin', function (res) {
                if (res === true) {
                    $.post("/Firends/Attention", { userid: userid }, function (data) {
                        $.toast(data.Message);
                        if (data.Message.indexOf("关注") > -1) {
                            $("#attentionbt").html("取消关注");
                        } else {
                            $("#attentionbt").html("<img src='/images/xqgz.png'> 关注");
                        }
                    });
                } else {
                    client._loginAlert();
                }
            });
        });
        $(document).on("click", ".hygz", function () {
            var userid = $(this).data("key");
            var self = $(this);
            //console.log("进来一次");
            $.post("/Firends/Attention", { userid: userid }, function (data) {
                $.toast(data.Message);
                self.hide();
                self.next().show();
            });
        });
    },

    praise: function () {
        $(document).on("click", ".qpraise,.xqlike", function () {
            var key = $(this).data("key");
            var $span = $(this).siblings("div");
            //没登录呢
            $.post('/Home/HasLogin', function (res) {
                if (res === true) {
                    $.post("/Question/PraiseQuestion", { questionid: key }, function (result) {
                        if (result.IsSuccess === true) {
                            $span.html(result.Num);
                            $(".praisedimg").show();
                            $(".praiseimg").hide();
                        }
                        if (result.Message) {
                            $.toast(result.Message);
                        }

                    });
                } else {
                    client._loginAlert();
                }
            });
        });
        $(document).on("click", ".cpraise", function () {
            var key = $(this).data("key");
            var $span = $(this).find("span");
            //没登录呢
            if (!client.islogin()) {
                client.loginAlert(url);
            }
            console.log(key);
            if (!key) return;
            $.post("/Question/PraiseComment", { commentid: key }, function (result) {
                if (result.IsSuccess === true) {
                    $span.html(result.Num);
                }
                if (result.Message) {
                    $.toast(result.Message);
                }

            });
        });
    },
    //让消息为已读
    readMessage: function () {
        var tabclicktimes = 0;
        $(document).on("click", "#systemmsgs", function () {
            console.log("系统消息");
            tabclicktimes++;
            $.post("/Messages/SetAllSystemMessageReaded", function (d) {
            });
            if (tabclicktimes > 1) {
                $("#tab3 .xximg span").remove();

            }
        });
        $(document).on("click", "#infomessageTab", function () {
            $("#tab1 .xximg span").remove();
        });
    },
    comment: function () {

        function hidecomment() {
            $(".comment.plbtm").hide();
            $("#comenttxt").blur();
            $("#comenttxt").attr("data-key", 0);
        }

        $(document).on("click", "#callcomment", function () {
            var key = $("#comenttxt").data("key");
            //$.toast(key);
            if (key == 0) {
                $(".comment.plbtm").show();
                $("#comenttxt").focus();
                $("#comenttxt").attr("data-key", 1);
            } else {
                hidecomment();
            }
        });

        $(document).on("click", "a,.tabs", function () {
            if ($(this)[0].id !== "callcomment") {
                hidecomment();
            }
        });

        $(document).on("click", "#commenta", function () {
            if ($(".comment.plbtm").hasClass("none")) {
                $(".comment.plbtm").removeClass("none");
            } else {
                $(".comment.plbtm").addClass("none");
            }
        });
        $(document).on("click", "#commentbt", function () {
            // 是否登录？
            var qid = $(this).data("key");
            var url = "/Account/Loadding?returnUrl=/Home/Detail/" + qid;
            var bt = $(this);
            $.post('/Home/HasLogin', function (res) {
                if (res === true) {
                    var answer = $("#comenttxt").val();
                    console.log(qid, answer);
                    bt.attr("disabled", "disabled");
                    $.post('/Question/Comment', { questionId: qid, content: answer }, function (data) {
                        console.log(data);
                        setTimeout(function () {
                            bt.removeAttr("disabled");
                        }, 2000);
                        if (data.IsSuccess === undefined) {
                            client.loginAlert(url);
                            return;
                        }
                        if (data.IsSuccess === true) {
                            $("#comenttxt").val("");
                            //加载评论
                            $.toast("评论成功,正在加载...");
                            hidecomment();
                            $.post("/Question/CommentsView", { quesitonId: qid }, function (view) {
                                $("#tab1").html(view);
                                var count = $("#tab1 .plbl").length;
                                $("#commenta span").html(count);
                                $(".mytabs .tab-link").eq(1).find("span").html(count);
                            });
                        } else {
                            // 回答错误 或其他
                            $.toast(data.Message);
                        }
                    });
                } else {

                    client._loginAlert(url);
                }
            });
        });
    },
    //去掉所有的html标记
    delHtmlTag: function (str) {
        return str.replace(/<[^>]+>/g, "").replace(/\r\s/g, '&nbsp;').replace(/\s/g, '&nbsp;'); //过滤换行符
    },
    //用户资料相关
    usercenter: function () {
        $(document).on("click", "#nicknamebt", function () {

            var name = $.trim($("#newname").val());
            if (name.length < 4) {
                $.toast("用户名不能少于4个字符");
                return;
            }
            $.post("/UserCenter/ChangeName", { name: name }, function (data) {
                if (data.IsSuccess === undefined) {
                    client.loginAlert(url);
                }
                if (data.IsSuccess === true) {
                    $.toast("修改成功");
                    setTimeout(function () {
                        location.href = "/UserCenter/Detail";
                    }, 1500);
                } else {
                    // 回答错误 或其他
                    $.toast(data.Message);
                }
            });

        });
        $(document).on("click", "#mobilebt", function () {
            var name = $.trim($("#newmobile").val());
            if (name.length < 11) {
                $.toast("请输入正确的手机号");
                return;
            }
            var code = $.trim($("#code").val());
            if (code.length < 4) {
                $.toast("请输入完整的验证码");
                return;
            }

            $.post("/UserCenter/ChangeMobile", { mobile: name, code: code }, function (data) {
                if (data.IsSuccess === undefined) {
                    client.loginAlert();
                }
                if (data.IsSuccess === true) {
                    $.toast("修改成功");
                    setTimeout(function () {
                        location.href = "/UserCenter/Detail";
                    }, 1500);
                } else {
                    // 回答错误 或其他
                    $.toast(data.Message);
                }
            });
        });
        $(document).on("click", "#passwordbt", function () {
            var old = $("#oldpwd").val() || "";
            var npwd = $("#newpwd").val() || "";
            var cpwd = $("#compwd").val() || "";
            if (old == "" || npwd == "" || cpwd == "") {
                $.toast("密码不能为空");
                return;
            }
            if (npwd != cpwd) {
                $.toast("两次密码输入不一致!");
                return;
            }
            if (npwd.length < 6) {
                $.toast("密码长度不能小于6!");
                return;
            }
            $.post("/UserCenter/ChangePassword", { oldpwd: old, pwd: npwd }, function (data) {
                if (data.IsSuccess === undefined) {
                    client.loginAlert();
                }
                if (data.IsSuccess === true) {
                    $.toast("修改成功");
                    setTimeout(function () {
                        location.href = "/UserCenter/Detail";
                    }, 1500);
                } else {
                    // 回答错误 或其他
                    $.toast(data.Message);
                }
            });

        });
        $(document).on("click", "#changepaypwdbt", function () {
            var old = $("#oldpwd").val() || "";
            var npwd = $("#newpwd").val() || "";
            var cpwd = $("#compwd").val() || "";
            if (old == "" || npwd == "" || cpwd == "") {
                $.toast("密码不能为空");
                return;
            }
            if (npwd != cpwd) {
                $.toast("两次密码输入不一致!");
                return;
            }
            if (npwd.length < 6) {
                $.toast("密码长度不能小于6!");
                return;
            }
            $.post("/UserCenter/ChangePayPassword", { oldpwd: old, pwd: npwd }, function (data) {
                if (data.IsSuccess === undefined) {
                    client.loginAlert();
                }
                if (data.IsSuccess === true) {
                    $.toast("修改成功");
                    setTimeout(function () {
                        location.href = "/UserCenter/Detail";
                    }, 1500);
                } else {
                    // 回答错误 或其他
                    $.toast(data.Message);
                }
            });

        });
        $(document).on("click", "#feebackbt", function () {
            var old = $("#feeback").val() || "";
            if (old.length < 10) {
                $.toast("内容不能少于10个字");
                return;
            }
            var phone = $("#feedbackphone").val();
            $.post("/UserCenter/Feeback", { content: old, phone: phone }, function (data) {
                if (data.IsSuccess === undefined) {
                    client.loginAlert();
                }
                if (data.IsSuccess === true) {
                    $.toast(data.Message);
                    setTimeout(function () {
                        location.href = "/UserCenter/Index";
                    }, 1500);
                } else {
                    // 回答错误 或其他
                    $.toast(data.Message);
                }
            });

        });
        $(document).on('click', '#logoff', function () {
            var buttons1 = [
              {
                  text: '确定退出登录',
                  onClick: function () {
                      location.href = "/Account/MLogOff";
                  }
              }
            ];
            var buttons2 = [
              {
                  text: '取消',
                  bg: 'danger'
              }
            ];
            var groups = [buttons1, buttons2];
            $.actions(groups);
        });

    },
    chat: {
        $text: $("#editor1"),
        $content: $("#output-content"),
        last: 0,
        showchatTime: null,
        showPastTime: false,
        showNowTime: false,
        appendTime: function (str) {
            $("#output-content").append("<div class='time'>" + str + "</p>");
        },
        currentTime: function (time, type) {
            var now;
            if (type == 2) {
                var start = time.indexOf('(');
                var end = time.lastIndexOf(')');
                time = time.substring(start + 1, end);
                now = new Date(Number(time));
            } else {
                now = new Date();
            }
            var month = now.getMonth() + 1; //月
            var day = now.getDate(); //日
            var hh = now.getHours(); //时
            var mm = now.getMinutes(); //分
            //var ms = now.getSeconds();//秒
            if (month < 10) {
                month = "0" + month;
            }
            if (day < 10) {
                day = "0" + day;
            }
            if (hh < 10) {
                hh = "0" + hh;
            }
            if (mm < 10) {
                mm = "0" + mm;
            }
            return month + "月" + day + "日 " + hh + ":" + mm;
        },
        isInterval: false,
        getmessagesInterval: function () {
            var self = this;
            if (self.isInterval) return null;
            console.log("启动了一次");

            self.showchatTime = setInterval(function () {
                var $time = $(".time").last();
                var key = $time.next();
                if (key.length) {
                    var t = self.currentTime();
                    self.appendTime(t);
                }
            }, 5 * 60 * 1000);

            return setInterval(function () {
                self.isInterval = true;
                var toid = $("#touserid").val();
                if (!toid) return;
                console.log(self.isInterval, "last", self.last, "toid", toid);


                $.post("/Messages/GetMessage", { toid: toid, lastId: self.last }, function (data) {
                    if (data) {
                        console.log(data);
                        if (data.lastId != 0) self.last = data.lastId;
                        for (var i = 0; i < data.Msgs.length; i++) {
                            var msg = data.Msgs[i];
                            if (self.messages.indexOf(msg.id) > -1) continue;
                            console.log('fromid', msg.fromid, 'msg.toId', msg.toid, "last", self.last);
                            if (!self.showPastTime) {
                                var t = self.currentTime(msg.time, 2);
                                self.showPastTime = true;
                                self.appendTime(t);
                            }
                            if (msg.toid == toid) {
                                self.userSay(msg.content, msg.id);
                            } else {
                                self.robotSay(msg.content, msg.id);
                            }
                            self.messages.push(msg.id);
                        }
                    }
                });
            }, 2000);
        },
        init: function () {
            var self = this;
            $(document).on("click", "#righticon", function () {
                self.sendMsg();
            });
            $(document).on("keypress", "#editor", function (event) {
                if (event.keyCode === 13) {
                    self.sendMsg();
                }
            });
            $(document).on("touchend", ".message", function () {
                console.log('touchend...');
                $(".msgtool").hide();
                var tool = $(this).siblings(".msgtool");
                tool.show();
                //其他隐藏
                setTimeout(function () {
                    tool.hide();
                }, 7000);
            });
            $(document).on("click", ".msgtool", function () {
                var p = $(this).parent();
                var id = p.attr("data-id");
                $.post("/Messages/Delete", { id: id }, function (d) {
                    console.log('删除...', d);
                });
                p.hide();
            });
        },
        scroll: function () {
            $('.content').scrollTop($('.content')[0].scrollHeight);
        },
        messages: [],
        sendMsg: function (msg) {
            var self = this;
            var txt = $("#editor1").val();
            console.log(txt, txt.length);
            if (!$.trim(txt)) return;
            var taget = self.userSay(txt);
            $("#editor1").val("");
            var toid = $("#touserid").val();
            $.post("/Messages/SendTo", { userid: toid, msg: txt }, function (data) {
                if (data !== 0) {
                    //发送成功
                    taget.attr("data-id", data);
                    self.messages.push(data);
                } else {
                    $.toast("发送失败");
                }
            });
            // self.ask(txt);
        },
        sendMsgTest: function (id, toid, msg) {
            var self = this;
            if (!$.trim(msg)) return;
            // var taget = self.robotSay(msg);
            $.post("/Messages/SendToTest", { userid: id, touserid: toid, msg: msg }, function (data) {
                if (data !== 0) {
                    //发送成功
                    // taget.attr("data-id", data);
                    //  self.messages.push(data);
                    $.toast("发送成功");
                } else {
                    $.toast("发送失败");
                }
            });
            // self.ask(txt);
        },
        getTime: function () {
            var myDate = new Date();
            var h = myDate.getHours();       //获取当前小时数(0-23)
            var m = myDate.getMinutes();     //获取当前分钟数(0-59)
            var s = myDate.getSeconds();     //获取当前秒数(0-59)
            return h + ":" + m + ":" + s;
        },
        robotSay: function (msg, id) {
            if (!msg) {
                return;
            }
            var self = this;
            var temp = $(".content>.to").clone().removeClass("hide").attr("data-id", id);
            temp.find(".ltkbl span").html(msg);
            $("#output-content").append(temp);
            self.scroll();
            return temp;
        },
        userSay: function (msg, id) {
            var self = this;
            var temp = $(".content>.me").clone().removeClass("hide").attr("data-id", id);
            temp.find(".ltkbl1 span").html(msg);
            $("#output-content").append(temp);
            self.scroll();
            return temp;
        }
    },

    init: function () {
        //让footer下面的页面选中
        $.modal.prototype.defaults.modalContainer = ".page-group";
        $.init();
        client.activeTab(client.footerTabs[$(".page-current").attr("id")]);
        $(document).on("pageInit", function (e, pageId, $page) {
            //console.log(pageId, "pageId");
            //if (pageId != "detail") {
            //    $(".doanswer,.resend").addClass("die");
            //}
            var index = client.footerTabs[pageId];
            //console.log("pageInit index:", index);
            client.activeTab(index);
            if (pageId == "chat") {
                client.chat.getmessagesInterval();
            } else {
                clearInterval(client.chat.getmessagesInterval);
            }
            if (pageId == "firends") {
                $.post('/Firends/GetNewFirends', function (n) {
                    console.log("GetNewFirendCount", n);
                    if (n === 0) {
                        $(".hymsg").remove();
                    }
                });
            }

            //detail页面启动popup

        });
        if ($(".page-current").attr("id") == "chat") {
            client.chat.getmessagesInterval();
        } else {
            clearInterval(client.chat.getmessagesInterval);
        }
        client.praise();
        client.comment();
        client.createQuestion();
        client.searchQuestion();
        client.buystrategy();
        client.recharge();
        client.answerQuestion();
        client.attentionUser();
        client.chat.init();
        client.usercenter();
        client.readMessage();
        // $.popup('.popup-reward');
        //获取验证码
        $(document).on("click", "#getvcode", function () {
            //先验证正确的手机号
            var tel = $("#regtel").val();
            var btn = $(this);
            var myreg = /^(((13[0-9]{1})|(15[0-9]{1})|(18[0-9]{1}))+\d{8})$/;
            if (!myreg.test(tel)) {
                $.alert("请输入正确的手机号码");
                return false;
            }
            //触发后台发送
            //post
            $.post("/AliMessage/SendRandomCodeToMobile", { phone: tel }, function (data) {
                if (data.success) {

                    $.toast("验证码发送成功，15分钟内有效哦!");
                    //开始倒计时
                    var count = 60;
                    //disabled
                    btn.css("font-size", "1rem").val(count).attr("disabled", "disabled");
                    var st = setInterval(function () {
                        count--;
                        btn.val(count);
                        if (count <= 1) {
                            //恢复点击
                            btn.css("font-size", ".5rem").val("获取验证码").removeAttr("disabled");
                            clearInterval(st);
                        }
                    }, 1000);

                } else {
                    $.alert(data.message);
                }
            });
        });
        //验证码输入
        //$(document).on("keyup", ".yzmbox input", function () {
        //    //onkeyup="this.value=this.value.replace(/[^0-9]/g,'')" onafterpaste="this.value=this.value.replace(/[^0-9]/g,'')"
        //    $(this).next().focus();

        //});
        $(document).on("click", "#setpaypasswordbt", function () {
            var p1 = $("#newpwd").val();
            var p2 = $("#compwd").val();
            if (p1.length != 6 || !/^\d{6}$/.test(p1)) {
                $.toast("密码为6位数字");
                return;
            }
            if (p1 !== p2) {
                $.toast("两次密码输入不一致!");
                return;
            }
            var backUrl = $("#rurl").val() || '/home/index';
            console.log(backUrl);
            $.post('/UserCenter/SetPayPwdJson', { pwd: p1 }, function (res) {
                if (res === true) {
                    $.alert("设置成功!", function () {
                        location.href = backUrl;
                    });
                }

            });
        });


        //提现
        $(document).on("click", ".cashbt", function () {
            //验证余额
            var money = $("#cashmoney").val();
            $.post("/Payment/CheckWalletAndFee", { money: money }, function (res) {
                if (res.IsSuccess === true) {
                    //创建订单
                    $.showIndicator();
                    $.post("/Payment/CreateToCashOrder", { money: money }, function (order) {
                        $.ajax({
                            url: "/Checkout/CashTransfers",
                            data: { orderNumber: order.OrderId },
                            type: "post",
                            success: function (result) {
                                if (result.IsSuccess === true) {
                                    //处理订单状态并扣除手续费
                                    $.post("/Payment/DealCashFee", { orderNumber: order.OrderId }, function (last) {
                                        //提示提现成功!
                                        if (last.IsSuccess === true) {
                                            $.alert("提现成功，请在查看您的微信零钱!", function () {
                                                location.href = "/UserCenter/Index";
                                            });
                                        } else {
                                            $.alert(last.Message);
                                        }
                                        $.hideIndicator();

                                    });

                                } else {
                                    $.hideIndicator();
                                    $.alert(result.Message);
                                }
                            },
                            error: function (err) {
                                $.hideIndicator();
                                alert(JSON.stringify(err));
                            }
                        });

                    });



                } else {
                    $.alert(res.Message);
                }
            });
        });
        $(document).on("click", "#zhilogon", function (e) {
            e.preventDefault();
            var cked = $("#iagree").is(":checked");
            if (!cked) {
                $.toast("请勾选下方协议，即可登录平台");
                return;
            }
            location.href = $(this).data("href");
        });
        //删除消息
        $(document).on("click", ".delete-block", function (e) {
            var db = $(this);
            var msgId = $(this).data("id");
            $.post("/Messages/DeleteMessage", { id: msgId }, function () {
                db.parents(".xxblock").remove();
            });
            e.stopPropagation();
        });
        //关闭模态框
        $(document).on("click", ".modal-title-inner>span,.modal-title-inner .close-modal", function () {
            $.closeModal();
        });
        $(document).on("click", ".payway.towallet", function () {
            var text = $(".title-content").html();
            if (text == "谜题红包支付") {
                //
                $(".title-content").html("选择支付方式");
                $(".modal-title-inner span,.payway.weixin").show();
                $(".close-modal,.payway.yue,.psdinput").hide();
                $(this).addClass("double");
                $(".payway.weixin,.payway.towallet").addClass("paypadding");

            } else {
                $(".title-content").html("谜题红包支付");
                $(".close-modal,.payway.yue,.psdinput").show();
                $(".modal-title-inner span,.payway.weixin").hide();
                $(this).removeClass("double");
                $(".payway.weixin,.payway.towallet").removeClass("paypadding");

            }
        });
        $(document).on("click", ".payway.weixin", function () {
            var yue = $(this).data("yue");
            var money = $(this).data("money");
            if (yue < money) return;//必须微信支付了

            var text = $(".title-content").html();
            if (text == "选择支付方式") {
                $(".title-content").html("谜题红包支付");
                $(".close-modal,.payway.yue,.paybutton").show();
                $(".modal-title-inner span").hide();
                $(".towallet").hide();
                $(".payway.weixin,.payway.towallet").removeClass("paypadding");
            } else {
                $(".title-content").html("选择支付方式");
                $(".modal-title-inner span,.payway.weixin,.towallet").show();
                $(".close-modal,.payway.yue,.psdinput,.paybutton").hide();
                $(".payway.weixin,.payway.towallet").addClass("paypadding");
            }
           
        });

        $(document).on("click", "#zidingyi", function() {
            $("#ziinput").toggle();
            $(".tsbll").toggle();
        });

    }
}
client.init();
initlogin();
checkopenid();
function checkopenid() {
    $.post("/Home/GetOpenId", function (openid) {
        if (openid) {
            localStorage["openid"] = openid;
            console.log("拿到openid", openid);
            // $.toast("拿到openid");
        } else {
           // setTimeout(checkopenid, 10000);
           // console.log("继续拿openid");
            // $.toast("继续拿openid");
        }

    });
}

function initlogin() {
    if (localStorage["openid"]) {
        var openid = localStorage["openid"];
        $.post("/home/LogoInit", { openid: openid }, function (data) {
            // console.log(data);
        });
    }
}
function selectPay(yue, money) {
    var paytxt = '<div class="payway towallet"><span>钱包余额:<span>' + yue + '</span>元</span><div class="right-cion"></div></div><div class="psdinput"><input type="password" placeholder="请输入密码" pattern="[0-9]*" autocomplete="off" value="" /></div>';
    var weixinpay = "";
    weixinpay = '<div data-money=' + money + ' data-yue=' + yue + ' class="payway weixin"><span>微信</span><div class="right-cion"></div></div><div data-money=' + money + ' class="payway paybutton"><a>立即支付</a></div>';
    if (money > yue) {
        clearInterval(interval);
        paytxt = weixinpay;
    } else {
        paytxt += weixinpay;
        $(".psdinput input[type='password']").val("");
        starinterval();
    }
    $.modal({
        title: '<div class="modal-title-inner"><span></span><div class="title-content">谜题红包支付</div><div class="close-modal"></div></div>',
        text: '<div class="payway yue">￥' + money + '</div>' + paytxt + '',
    });
    $(".modal-in").addClass("payModal");
    $(".modal").css({ 'width': '88% !important', 'margin-left': '6% !important', 'left': '0' });
    if (money <= yue) {
        $(".payway.weixin,.payway.paybutton").hide();
    }
}

var payType = 0;//0提示 1支付
var interval;
var ispay = false;
var starinterval = function () {
    //这个是需要输入密码的情况
    interval = setInterval(function () {
        //提示还是出题呢？
        if ($(".page-current")[0] && $(".page-current")[0].id != "buy-info" && $(".page-current")[0].id != "chuti3") return;
        var pwds = $(".psdinput input[type='password']").val();
        if (!pwds) return;
        if (pwds.length < 6) return;
        $.closeModal();
        if (ispay) return;
        ispay = true;
        $.post('/UserCenter/CheckPaymentPassword', { pwd: pwds }, function (data) {
            $.hideIndicator();
            ispay = false;
            if (data === true) {
                payType === 0 && _buystargety();
                payType === 1 && submitQuestion();
            } else {
                $(".psdinput input").val("");
                $.alert("密码错误");
            }
        });
    }, 300);
}

//这是直接微信支付
$(document).on("click", ".payway.paybutton", function () {
    var money = $(this).data("money");
    var yue = $(this).data("yue");
    //购买提示
    if (!money) {
        $.toast("金额不存在");
        return;
    }

    $.closeModal();
    if (payType === 0) {
        var ckradio = $(".tsbl.selected");
        var sid = ckradio.data("key");
        var qid = ckradio.data("qid");
        $.post("/payment/CanBuyStrategy", { strategyId: sid, questionId: qid }, function (data) {
            if (data.isSuccess === true) {
                createOrderAndPay(money, _buystargety);
            } else {
                $.toast(data.message);
            }
        });
    }
    //能走到这儿 默认钱包余额不够
    if (payType === 1) {
        createOrderAndPay(money, submitQuestion);
    }
    //购买谜题
});
//提交
function submitQuestion() {
    $.post('/Question/SumbitQuestion', function (createdata) {
        if (createdata.IsSuccess === true) {
            location.href = "/Home/Detail/" + createdata.Num;
        } else {
            $.alert(createdata.Message);
        }
    });
}
function paytishi(pwd) {
    if (!pwd || pwd.length < 6) {
        return;
    }
    $.showIndicator();
    $.post('/UserCenter/CheckPaymentPassword', { pwd: pwd }, function (data) {
        $.hideIndicator();
        if (data === true) {
            _buystargety();
        } else {
            $(".glnumbl input").val("");
            $.alert("密码错误");
        }
    });

}
function _buystargety() {
    var ckradio = $(".tsbl.selected");
    var sid = ckradio.data("key");
    var qid = ckradio.data("qid");
    var money = ckradio.data("price");
    $.post("/Payment/BuyQuestionStrategy", { strategyId: sid, questionId: qid, money: money }, function (data) {
        console.log(data);
        if (data.isSuccess === true) {
            location.href = "/Home/PaymentResult?orderNumber=" + data.order;
        } else {
            $.toast(data.message);
        }
    });
}
function loginTest(name) {
    name = name || "test2";
    var pwd = "111111";
    $.post("/Account/MLogon", { mobile: name, password: pwd, isreme: false }, function (res) {
        $.toast(name + "登录成功!");
        setTimeout(function () {
            location.reload();
        }, 1000);
    });
}
setInterval(function() {
    if (document.activeElement.id.indexOf('anwsertxt') >= 0) {
        document.activeElement.scrollIntoViewIfNeeded();
    }

}, 300);
