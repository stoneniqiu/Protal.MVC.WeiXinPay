
//初始事件绑定
//点击移除提示
$(document).on("click",".note",function() {
    $(this).remove();
})
//登录检查
$(document).on("keyup", "#uname,#pwd", function () {
    var len1 = $("#uname").val().length;
    var len2 = $("#pwd").val().length;
    if (len1 >= 2 && len2 >= 5) {
        $("#loginbt").removeClass("disabled");
    } else {
        $("#loginbt").addClass("disabled");
    }
})
window.tools = {};
var cleartime = null;
//警告提示
tools.warning = function (txt, $taget) {
    $taget = $taget || $(".page-current");
    $(".page-current .note").remove();
    if (typeof (txt) == "object") txt = txt.content;
    var div = $("<div>").addClass("note").addClass("warning").html(txt);
    $taget.append(div);

    clearTimeout(cleartime);
    cleartime = setTimeout(function () {
        if ($(".page-current .note").length) {
            $(".page-current .note").remove();
        }
    }, 5000);
}
tools.clear=function() {
    $(".page-current .note").remove();
}
tools.save=function(key,value) {
    if (!key) {
        tools.warning("key不能为空");
        return;
    }
    localStorage[key] = value;
}
tools.get=function(key) {
    return localStorage[key];
}
tools.lastchar=function(name) {
    return name.substr(name.length - 1, 1).toUpperCase();
}
tools.time=function() {
    var now = new Date();
    var month = now.getMonth() + 1; //月
    var day = now.getDate(); //日 var hh = now.getHours(); //时
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

    return month + "-" + day + " " + hh + ":" + mm;
}

tools.isEmpty=function (keys) {
    if (typeof keys === "string") {
        keys = keys.replace(/\"|&nbsp;|\\/g, '').replace(/(^\s*)|(\s*$)/g, "");
        if (keys == "" || keys == null || keys == "null" || keys === "undefined" || keys == "undefined") {
            return true;
        } else {
            return false;
        }
    } else if (typeof keys === "undefined") {
        return true;
    } else if (typeof keys === "number") {
        return false;
    } else {
        if (typeof keys == "object") {
            if ($.isEmptyObject(keys)) {
                return true;
            } else {
                return false;
            }
        }
        return true;
    }
}

tools.getExtension=function(fileName) {
    var index = fileName.lastIndexOf('.');
    var ext = fileName.substring(index, fileName.length).toUpperCase();
    return ext;
}
var imgTypes = [".PNG", ".JPEG", ".GIF", ".BMP", ".JPG", ".ICO"];
//支持文件类型
var fileTypes = { ".TXT": "txt.png", ".PDF": "pdf.png", ".DOC": "doc.png", ".DOCX": "docx.png", ".PPT": "ppt.png", ".PPTX": "ppt.png", ".XLS": "xls.png", ".XLSX": "xlsx.png" };

tools.converFile=function(item) {
    var ext = tools.getExtension(item.fileName);
    var root = "../";

    //先判断是图片
    var $img = $("<img>");
    $img[0].src = root + "/css/img/" + "unknownfile.png";
    if (~imgTypes.indexOf(ext)) {
        //是一张图片
        $img[0].src = root+item.url;
    } else if(fileTypes[ext]){
        $img[0].src = root + "/css/img/"+fileTypes[ext];
    }


    return $img;

}
//{name:xx,kk:toto}
tools.parseObj = function (str) {
    var obj = {};
    var list = str.replace("{", "").replace("}", "").split(",");
    for (var i = 0; i < list.length; i++) {
        var kv = list[i].split(':');
        obj[kv[0]] = kv[1];
    }
    return obj;
}
//--------------页面交互-----------------------

//登录页面
$(document).on("click", "#loginbt", function () {
    //检测手机号
    var phone = $("#uname").val();
    var pwd = $("#pwd").val();
    var myreg = /^[1][3,4,5,7,8][0-9]{9}$/;
    isinit = false;
    if (myreg.test(phone)) {
        //正确
        tools.clear();
        $(this).addClass("disabled").val("正在登录...");
        sdk.login(phone,pwd,function(data) {
            console.log(data);
            if (data.Code != "001") {
                //登录有误 可以重新登录
                $("#loginbt").removeClass("disabled").val("登录");
                tools.warning(data.Message, $(".loginbox"));
            } else {
                //登录成功
                $("#loginbt").val("登录成功!");
                init();
                $.router.load("#messagepage");
                $("#uname").val("");
                $("#pwd").val("");
                $("#loginbt").val("登录成功!");
            }
        })
    } else {
        tools.warning("手机号格式不正确!", $(".loginbox"));
    }
})
$("#loginoutbt").click(function() {
    localStorage.token = null;
    sdk.close();
    $.router.load("#loginpage");
});

//对话人id
var currentToId, $box, viewkey = "chatview", $box, warp, word,
    msgscache = {},allfriends= {};

//自己说
function rightsay(content,msgid,type) {
    $box = $(".messages.active");
    var rawmsg = selfgroupmsg[msgid];
    var txt = "未读";
    var gclass = "";
    if (rawmsg) {
        var ginfo = allfriends[rawmsg.receiverid];
        //总的人数
        var total = ginfo.Users.length - 1;
        txt = total + "人未读";
        gclass = "unlist";//表示显示未读人名单的
    }
    var clasname = "msgcontent";
    if (type == "004") {
        //说明是个文件，那么这里就要开始转化
        var file = tools.parseObj(content);
        console.log("文件", file);
        content = tools.converFile(file);
        clasname += " imgbox";
    }
    word = $("<div class='" + clasname + "'>").html(content);
    var span = $("<div class='unread'>").html(txt).addClass(gclass).attr("data-gid", currentToId).attr("data-msgid", msgid);
    warp = $("<div class='rightsay'>").attr("id", msgid).append(word).append(span);
    //显示上去，
    $box.append(warp);
}
//他人说
function leftsay(boxid, content, msgid) {
    //这个view不一定打开了。
    $box = $("#" + boxid);
    word = $("<div class='msgcontent'>").html(content);
    warp = $("<div class='leftsay'>").attr("id", msgid).append(word);
    if ($box.length) {
        $box.append(warp);
    } else {
        $box = $("<div class='messages' id=" + boxid + ">");
        $box.append(warp);
        $("#messagesbox").append($box);
    }
}


//用来展示发送者的消息 
function messageItem(friendId,msg,name,imgurl) {
    var key = friendId + "item",item;

    var $item = $("#" + key);

    item = allfriends[friendId];
    if (!name) {
        name = item.Name;
      
        imgurl = item.Image;
        console.log("messageitem",item);
    }

    var now = tools.time();
    if (!$item.length) {
        //没有就创建一个
        $item = $("<div>").addClass("messages-item");
        var $avatar = $("<div>").addClass("avatar");
        if (imgurl) {
            var img = $("<img>").attr("src", imgurl);
            $avatar.append(img);
        } else {

            if (item.IsGroup) {
                //这个地方还是要判断是不是群
                //可以用其他名字组合
                $avatar.html("群");
            } else {
                $avatar.html(tools.lastchar(name));
            }
        }
        $item.append($avatar);

        var $content = $("<div>").addClass("item-content");
        var $title = $("<div>").addClass("title-warp");
        var $name = $("<div>").addClass("name-warp").html(name);
        var $time = $("<span>").addClass("time").html(now);
        $title.append($name).append($time);
        $content.append($title);
        var $msg = $("<div>").addClass("last-message").html(msg);
        $content.append($msg);
        $item.append($content);
        $item.attr("id", key).attr("data-key", friendId);
        $("#messagepage .content").append($item);
    } else {
        $item.find(".name-warp").html(name);
        $item.find(".last-message").html(msg);
        $item.find(".time").html(now);
    }
  }
//未读标记
function unreadmark(friendId, count) {
    var item = friendId + "item";
    $("#" + item).find(".unreadnum").remove();
    if (count === 0) {
        return;
    }
    if (count > 99) count = "99+";
    //需要创建一个消息列表
    //然后加载
    var span = $("<span class='unreadnum' >").html(count);
    $("#" + item).find(".item-content").append(span);
}

//初始化部分
//1是连接websocket。
//2是拉取联系人列表
//3拉取消息
//然后可以进行聊天
var isinit = false;
//自己发的群消息 用来处理群回执
var selfgroupmsg = {};

function init() {
    if (isinit) return;

    isinit = true;
    sdk.on("messages", function(data) {
        console.log("收到", data);
        if (sdk.isSelf(data.senderid)) {

            //每条群消息 用list来存储有几人发了回执
            data.list = [];
            if (data.isgroup)
                selfgroupmsg[data.msgid] = data;

            rightsay(data.content, data.msgid,data.messagetype);
            messageItem(currentToId, data.content);

        } else {
            //别人说的
            //不一定是当前对话，就要从ReceiverId判断。
            var _toid = data.senderid;
            if (!sdk.isSelf(data.receiverid)) {
                //接受者不是自己 说明是群消息
                _toid = data.receiverid;
            }
            var boxid = _toid + viewkey;

            messageItem(_toid, data.content);

            if (!msgscache[_toid]) {
                msgscache[_toid] = [];
            }

            //如果是当前会话就发送已读回执
            if (_toid == currentToId && $(".page-current")[0].id == "chatpage") {
                //但是可能当前页面不是聊天页面 也要处理
                sdk.sendReceipt(data.senderid, data.msgid);
            } else {
                //存入未读列表
                msgscache[_toid].push(data);
                unreadmark(_toid, msgscache[_toid].length);
            }
            leftsay(boxid, data.content, data.msgid, data.messagetype);
        }
    });
    function readmsg(data) {
        //区分是单聊还是群聊
        //单聊就直接是已读
        var msgid = data.msgid;
        var rawmsg = selfgroupmsg[msgid];
        if (!rawmsg) {
            $("#" + msgid).find(".unread").html("已读").addClass("ed");
        }
        else {
            rawmsg.list.push(data);
            //得到了这个群的信息
            var ginfo = allfriends[rawmsg.receiverid];
            //总的人数
            var total = ginfo.Users.length;
            //找到原始的消息
            //已读的人数
            var readcount = rawmsg.list.length;
            //未读人数
            var unread = total - readcount - 1;//除去自己
            var txt = "已读";
            if (unread != 0) {
                txt = unread + "人未读";
                $("#" + msgid).find(".unread").html(txt);
            } else {
                $("#" + msgid).find(".unread").html(txt).addClass("ed");
            }
        }
    }
    sdk.on("receipts",function(data) {
        console.log("回执", data);
        readmsg(data);
    })
    sdk.on("error", function(data) {
        if (data.code) {
            tools.warning("websocket 关闭");
            return;
        }
        tools.warning(data);

        if (data.type == "0011") {
            //token过期 重新登录;
            $.router.load("#loginpage");
        }
    });

    //eventManger.trigger('uploadSuccess', item);
    sdk.on("uploadSuccess", function (data) {
        sdk.sendFile(currentToId,data);
    });
    sdk.on("uploadError", function (data) {
        tools.warning("上传失败!");
    });
    sdk.init();


    activeUser(tools.get("toname"), tools.get("touserkey"));


   // $.showPreloader('正在加载联系人');
    sdk.getUserInfo(localStorage.token, initFriends);
   
    //还要加载之前的消息
}
//如果已经登录了 就早点加载用户信息
if (localStorage.token) {
    init();
}

function initFriends(list) {
    $("#minepage .card-header").html(list[1]);
    //这个时候加载上传插件，确保token是可用的
    sdk.loadinit();
    sdk.getFriends(function(data) {
        var $list = $("#list");
        $list.html("");
        for (var i = 0; i < data.Data.length; i++) {
            var item = data.Data[i];
            var chat = tools.lastchar(item.Name);

            allfriends[item.Id]=item;
            if (item.IsGroup) {chat = "群";  }  
          
            ////默认选中第一个
            //if (i == 0) activeUser(item.Name, item.Id);
            var $avatar = $("<div>").addClass("avatar").html(chat);
            var $name = $("<span>").html(item.Name);
            var div = $("<div class='firend'>").attr("id", item.Id).append($avatar).append($name);
            $list.append(div);
            
        }
    })
}

// 联系人页面
$(document).on("click", "#list .firend", function () {
    $.router.load("#chatpage");
    var name = $(this).find("span").html();
    activeUser(name, $(this).attr("id"));
});
//消息列表点击
$(document).on("click", "#messagepage .messages-item", function () {
    $.router.load("#chatpage");
    var userid = $(this).attr("data-key");
    var name = allfriends[userid].Name;
    activeUser(name, userid);
});
//点击未读列表
$(document).on("click", ".unlist", function () {
    //拿到群信息
    //群id
    var gid = $(this).attr("data-gid");
    var msgid = $(this).data("msgid");
    var ginfo = allfriends[gid];
    var rawmsg = selfgroupmsg[msgid];
    var list = [];
    for (var i = 0; i < ginfo.Users.length; i++) {
        var user = ginfo.Users[i];
        if (!sdk.isSelf(user.UserGuid)) {
            user.isRead = false;
            for (var j = 0; j < rawmsg.list.length; j++) {
                var item = rawmsg.list[j];
                if (item.userid == user.UserGuid) {
                    user.isRead = true;
                    user.readTime = item.createtime;
                }
            }
            list.push(user);
        }
    }
    console.log(list);
    //拿到已读的信息
});

$("#content").focus(function (e) {
            e.stopPropagation();
            e.preventDefault();
           // imClient.scroll();
        $("#content").css("color", "#222222").removeAttr("placeholder");
});
$("#content").blur(function () {
    if (tools.isEmpty($("#content").html())) {
        $("#content").attr("placeholder", "请输入内容");
    }  
    //var iosVersion = imClient.getIosVersion();
    //if (iosVersion && iosVersion >= 10 && imClient.isPhone() || imClient.isIpad()) {
    //    $(".footer,.messageList").css({ 'bottom': '0' });
    //}
});

$("#sendbt").click(function() {
    send();
});

$("#content").keydown(function (event) {
    if (event.keyCode === 13) {
        send();
        event.preventDefault();
    }
});
function send(txt) {
    txt = txt || $("#content").html();
    if (tools.isEmpty(txt)) {
        $.toast("内容不能为空!");
        return;
    }
    //还要进行过滤的 先不管
    //字数限制
    if (!currentToId) {
        tools.warning("当前对话ID为空!");
        return;
    }
    $("#content").html("");
    console.log("发送",txt);
    sdk.sendTo(currentToId, txt);
}

$(document).on("pageInit", "#chatpage", function(e, pageId, $page) {
   // console.log("pageinit", pageId);
});
//为用户准备页面
function activeUser(userName, userId) {
    //先查找有无对应的聊天页面
    currentToId = userId;
    $("#toName").html(userName);
    tools.save("toname", userName);
    tools.save("touserkey", userId);

    console.log("activeUser", userName, userId);

    var divId = userId + viewkey;
    var $div = $("#" + divId);
    if (!$div.length) {
        $(".messages").removeClass("active");
        //没有就再创建一个
        $div = $("<div class='messages active' id=" + divId + ">");
        $("#messagesbox").append($div);
    } else {
        $div.siblings().removeClass("active");
        $div.addClass("active");
    }
    //还要做一个工作 就是需要把未读的消息 发送已读回执到服务器
    var unreads = msgscache[userId];
    if (unreads && unreads.length > 0) {
        for (var i = 0; i < unreads.length; i++) {
            var item = unreads[i];
            sdk.sendReceipt(item.senderid, item.msgid);
        }
        msgscache[userId] = [];
    }

    //还要做一个工作 就是需要把未读的消息 发送已读回执到服务器
    unreadmark(userId, 0);
}