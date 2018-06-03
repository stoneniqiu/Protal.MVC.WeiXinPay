; (function ($) {
    $.extend($, {
        fileUpload: function (options) {
            var para = {
                multiple: false,
                filebutton: ".filePicker",
                uploadButton: null,
                url: "/Question/MUploadImg",
                base64strUrl: "/Question/MUploadImgBase64Str",
                filebase: "mfile",//mvc后台需要对应的名称
                auto: true,
                previewZoom: null,
                uploadComplete: function (res) {
                    //console.log("uploadComplete", res);
                    //uploadCount++;
                    //core.checkComplete();
                },
                uploadError: function (err) {
                    console.log("uploadError", err);
                },
                onProgress: function (percent) {  // 提供给外部获取单个文件的上传进度，供外部实现上传进度效果
                    console.log(percent);
                },
            };
            para = $.extend(para, options);

            var $self = $(para.filebutton);
            //先加入一个file元素
            init();
            var multiple = "";  // 设置多选的参数
            para.multiple ? multiple = "multiple" : multiple = "";
            $self.css('position', 'relative');
            var inputstr = '<input id="fileImage"  style="opacity:0;position:absolute;top: 0;left: 0;width:100%;height:100%" accept="image/jpeg,.jpg,image/gif,.gif,image/png,.png,image/bmp,.bmp,.jpeg"  type="file" size="30" name="fileselect[]" ' + multiple + '>';
            $self.append(inputstr);

            var doms = {
                "fileToUpload": $self.find("#fileImage").last(),
                // "thumb": $self.find(".thumb"),
                // "progress": $self.find(".upload-progress")
            };

            function init() {
                $self.find("#fileImage").remove();
                $(document).off("change", "#fileImage");
                $(document).off("click", para.filebutton);
               //$(document).off("click", para.uploadButton);
            }
            function simpleSize(size) {
                if (!size) return "0";
                if (size < 1024) {
                    return size;
                }
                var kb = size / 1024;
                if (kb < 1024) {
                    return kb.toFixed(2) + "K";
                }
                var mb = kb / 1024;
                if (mb < 1024) {
                    return mb.toFixed(2) + "M";

                }
                var gb = mb / 1024;
                return gb.toFixed(2) + "G";
            };

            var uploadCount = 0;
            var core = {
                fileSelected: function () {
                    var files = $("#fileImage").last()[0].files;
                    var count = files.length;
                    console.log("共有" + count + "个文件");
                    for (var i = 0; i < count; i++) {
                        if (i >= para.limitCount) {
                            console.log("最多只能选择" + para.limitCount + "张图片!");
                            break;
                        }
                        var item = files[i];
                        console.log("原图片大小", simpleSize(item.size));
                        var isAndroid = navigator.userAgent.toLowerCase().match(/android/i) == "android";
                        if (para.auto) {
                            if (isAndroid) {
                                var FR = new FileReader();
                                FR.onload = function (e) {
                                    var imageString = e.target.result;
                                    console.log("安卓上传");
                                    core.uploadBase64str(imageString);
                                    //这个ImageString 就是图片转成的base64字符串  
                                };
                                FR.readAsDataURL(item);

                            } else {
                                core.uploadFile(item);
                            }

                        }
                        core.previewImage(item);
                    }
                },
                uploadBase64str: function (base64Str) {

                    //var blob = dataURItoBlob(base64Str);
                    //console.log("压缩后的文件大小", blob.size);
                    //core.uploadFile(blob);
                    var formdata = new FormData();
                    formdata.append("base64str", base64Str);
                    var xhr = new XMLHttpRequest();
                    xhr.upload.addEventListener("progress", function (e) {
                        var percentComplete = Math.round(e.loaded * 100 / e.total);
                        para.onProgress(percentComplete.toString() + '%');
                    });
                    xhr.addEventListener("load", function (e) {
                        para.uploadComplete(xhr.responseText);
                    });
                    xhr.addEventListener("error", function (e) {
                        para.uploadError(e);
                    });

                    xhr.open("post", para.base64strUrl, true);
                    xhr.send(formdata);
                },
                uploadFile: function (file) {
                    console.log("开始上传");
                    var formdata = new FormData();

                    formdata.append(para.filebase, file);//这个名字要和mvc后台配合

                    var xhr = new XMLHttpRequest();
                    xhr.upload.addEventListener("progress", function (e) {

                        var percentComplete = Math.round(e.loaded * 100 / e.total);
                        para.onProgress(percentComplete.toString() + '%');
                    });
                    xhr.addEventListener("load", function (e) {
                        para.uploadComplete(xhr.responseText);
                    });
                    xhr.addEventListener("error", function (e) {
                        para.uploadError(e);
                    });

                    xhr.open("post", para.url, true);
                    xhr.send(formdata);
                },
                checkComplete: function () {
                    var all = (doms.fileToUpload)[0].files.length;
                    if (all == uploadCount) {
                        console.log(all + "个文件上传完毕");
                        $("#fileImage").remove();
                        $self.append(inputstr);
                    }
                },
                uploadFiles: function () {
                    var files = (doms.fileToUpload)[0].files;
                    for (var i = 0; i < files.length; i++) {
                        core.uploadFile(files[i]);
                    }
                },
                previewImage: function (file) {
                    if (!para.previewZoom) return;
                    var img = document.createElement("img");
                    img.file = file;
                    $(para.previewZoom).append(img);
                    // 使用FileReader方法显示图片内容
                    var reader = new FileReader();
                    reader.onload = (function (aImg) {
                        return function (e) {
                            aImg.src = e.target.result;
                        };
                    })(img);
                    reader.readAsDataURL(file);
                }
            }
            $(document).on("change", "#fileImage", function () {
                core.fileSelected();
            });

            $(document).on("click", para.filebutton, function () {
                console.log("clicked");
            });
            if (para.uploadButton) {
                $(document).on("click", para.uploadButton, function () {
                    core.uploadFiles();
                });
            }
        }
    });
})(Zepto);
