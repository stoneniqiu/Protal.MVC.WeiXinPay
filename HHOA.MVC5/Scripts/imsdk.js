(function () {
    var eventManger = {
        handlers: {},
        //类型,绑定事件 
        addHandler: function (type, handler) {
            if (typeof this.handlers[type] == "undefined") {
                this.handlers[type] = [];//每个事件都可以绑定多次
            }
            this.handlers[type].push(handler);
        },
        removeHandler: function (type, handler) {
            var events = this.handlers[type];
            for (var i = 0, len = events.length; i < len; i++) {
                if (events[i] == handler) {
                    events.splice(i, 1);
                    break;
                }
            }
        },
        trigger: function (type) {
            if (this.handlers[type] instanceof Array) {
                var handlers = this.handlers[type];
                var args = Array.prototype.slice.call(arguments, 1);
                for (var i = 0, len = handlers.length; i < len; i++) {
                    handlers[i].apply(null, args);
                }
            }
        }
    };
    var socketSDK = {};
    var websocketUrl = "ws://127.0.0.1:8080/Chat";
    socketSDK.on = function (type, event) {
        eventManger.addHandler(type, event);
    };

    //移除事件
    socketSDK.off = function (type, event) {
        eventManger.removeHandler(type, event);
    };
    var ws, sender;

    var reconnetInterval = 2000;
    var retime = 1;
    function reconnect() {
        console.log("正在重连....",retime++);
        setTimeout(socketSDK.init, reconnetInterval);
    }

    socketSDK.init = function () {
        try {
            ws = new WebSocket(websocketUrl);
        } catch (e) {
            reconnect();
            return;
        }
        ws.onerror = function (err) {
            eventManger.trigger("error", "websocket错误,原因:" + JSON.stringify(err));
        }
        ws.onopen = function (e) {
            console.log("websocket 连接成功", e);
            //存在token 就自动连接一次  
            if (localStorage.token) {
                socketSDK.validToken();
            }

        }
        ws.onclose = function (e) {
            console.log("websocket 关闭", e);
            //eventManger.trigger('error', e);
            //非正常关闭 服务端的问题
            if (e.code == "1002") {
                //那么就3秒后重连
                setTimeout(socketSDK.init, 3000);
            }
        }
        ws.onmessage = function (e) {
            console.log("收到", e.data);
            var data = JSON.parse(e.data);
            if (data.type == "001" && !sender) {
                sender = data.sender;
            }
            //token验证失败
            if (data.type === "0011") {
                localStorage.token = null;
                eventManger.trigger('error', data);
                return;
            }

            if (data.hasOwnProperty("senderid")) {
                //消息
                eventManger.trigger('messages', data);
                if (data.senderid != sender) {
                    //别人的消息 显示上去

                } else {
                    //自己的消息 将消息的Id加上去 走到这儿说明发送到服务端是成功的

                }

            } else if (data.hasOwnProperty("receiptid")) {
                if (data.userid != sender) {
                    //别人的回执 用来改变界面效果
                    eventManger.trigger('receipts', data);

                } else {
                    //自己的回执  说明回执发送成功
                }
            }

        }
    }

      function send(obj) {
        //必须是对象，还有约定的类型
          if (ws && ws.readyState == WebSocket.OPEN) {
              ws.send(JSON.stringify(obj));
          } else {
              eventManger.trigger("error", "发送失败，websocket未连接");

          }

      }

      socketSDK.close= function () {
        ws && ws.close();
    }

    socketSDK.isSelf= function(id) {
        return id === sender;
    }
      socketSDK.sendTo = function (toId,msg) {
        var obj = {
            toId:toId,
            content: msg,
            type: "002"//聊天
        }
        send(obj);
      }
      socketSDK.sendFile = function (toId, item) {
        var obj = {
            toId: toId,
            content:JSON.stringify({
                url: item.WebPath,
                fileName: item.RawName,
                guid: item.GuId,
                size: item.Size
            }),
            md5:item.MD5,
            type: "004"//图片
        }
        send(obj);
    }
      socketSDK.validToken = function (token) {
          var obj = {
              content: token || localStorage.token,
              type: "001"//验证
          }
          send(obj);
      }
    //这个toid是应该可以省略的，因为msgId里面已经存在了
    //目前这么做的理由就是避免服务端进行一次查询。
    //toId必须是userId 也就是对应的sender
      socketSDK.sendReceipt = function (toId, msgId) {
          if (!toId || !msgId) {
              console.log("id不能为空");
              return;
          }
          var obj= {
              toId: toId,
              content: msgId,
              type:"003"
          }
          send(obj)
      }
    //001 验证token
    //002
    var root = "http://localhost:3615/api";


    socketSDK.login = function (username, pwd,callback) {
        $.post(root+"/Identy/login",
           { uname: username, pwd: pwd },
           function (data) {
               if (data.Code == "001") {
                   localStorage.token = data.Data;
                   console.log("验证token", localStorage.token);
                   socketSDK.validToken(data.Data)
               }
               callback&&callback(data);
           });
    }
    socketSDK.getUserInfo = function (token,callback) {
        token = token || localStorage.token;
        $.post(root + "/Identy/Authen?token=" + token,
           function (res) {
              // $.hidePreloader();
               if (res.Code == "001") {
                   var list = res.Data;
                   localStorage.userInfo = list;
                   sender = list[0];
                   callback && callback(list);
               } else {
                   localStorage.token = null;
               }
           });
    }
    socketSDK.getFriends = function (callback) {
        if (!sender) {
            console.log("请先登录!")
            return;
        }
        $.post(root + "/User/Friends?guid=" + sender, function (list) {
            callback && callback(list);
        });
    }

    socketSDK.loadinit = function (accessToken) {
        accessToken = accessToken || localStorage.token;
        console.log("init upload");
        var uploader = new plupload.Uploader({
            runtimes: 'flash,html5,silverlight,html4',
            browse_button: 'attach',
            multi_selection: true,
            //https://imonline.eastmoney.com//ImageService_fund_z/WebFormImageUpload
             url: root + '/FileService/Upload',
            // url: "https://imonline.eastmoney.com//ImageService_fund_z/WebFormImageUpload",
            flash_swf_url: '../Content/plupload-2.1.8/js/Moxie.swf',
            silverlight_xap_url: '../Content/plupload-2.1.8/js/Moxie.xap',
            headers: {
                "Token": accessToken
            },
            filters: {
                max_file_size: "10mb",
                mime_types: [
                ]
            },
            init: {
                PostInit: function () {
                },
                FilesAdded: function (up, files) {
                    plupload.each(files, function (file) {
                        uploader.start();

                    });
                },
                UploadProgress: function (up, file) {
                   // console.log(up,file);
                },
                Error: function (up, err) {
                    console.log(up, err);
                    eventManger.trigger('uploadError', err);
                    if (err.code === -601) {
                        // imClient.showerrorInfo("请选择图片上传!");
                        return;
                    }
                    if (err.message) {
                        // imClient.showerrorInfo(err.message);
                    }
                }
            }
        });
        uploader.init();
        uploader.bind('FileUploaded', function (upldr, file, object) {
            console.log(object);
            var data = JSON.parse(object.response);
            if (data.Code == "001" && data.Count > 0) {
                for (var i = 0; i < data.Count; i++) {
                    var item = data.Data[i];
                    console.log("上传结果：",item);
                    eventManger.trigger('uploadSuccess', item);
                    //sendMsg(JSON.stringify({
                    //    type: "002",
                    //    url: item.WebPath,
                    //    fileName: item.RawName,
                    //    guid: item.GuId
                    //}));
                    //显示在界面上

                }
            }

        });
    }

    //页面显示部分
    

    window.sdk = socketSDK;

 })()