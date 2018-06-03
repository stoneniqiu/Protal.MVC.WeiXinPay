var Admin = function() {
    var self = this;

    self.setActive = function(id) {
        $("#" + id).addClass("active");
        $("#" + id).find("ul").slideDown();
    };
    self.deleteItem = function ($ele, url, info, callBack) {
        var id = $ele.data("id");
        if (!id) {
            console.log("id 不能为空！");
            return;
        }
        info = info || "确定删除这条数据";
        var row = $ele.parent().parent();
        if (confirm(info)) {
            $.post(url, { id: id }, function (data) {
                if (data == 1) {
                    row.fadeOut();
                } else {
                    console.log("删除失败", data);
                }
                if (callBack) {
                    callBack(data);
                }
            });
        }


    };
    self.deleteItems = function($cks,url,info) {
        info = info || "确定删除这些数据";
        $cks = $cks || $("table input:checkbox:checked");
        if ($cks.length == 0) {
            alert("请选择要删除的数据");
        } else {
            if (confirm(info)) {
                $cks.each(function () {
                    $(this).parents("tr").fadeOut();
                    var id = $(this).data("id");
                    $.post(url, { id: id }, function () {
                    });
                });
            }
        }
    };
    self.loadImg = function(buttonId, callback) {
        var uploader = new plupload.Uploader({
            runtimes: 'html5,flash,silverlight,html4',
            browse_button: buttonId,
            url: '/Admin/Home/UploadImg',
            flash_swf_url: '/Content/plupload-2.1.8/js/Moxie.swf',
            silverlight_xap_url: '/Content/plupload-2.1.8/js/Moxie.xap',
            filters: {
                max_file_size: "3mb",
                mime_types: [
                    { title: "Image files", extensions: "jpg,gif,png,bmp,jpeg" },
                    //{ title: "Zip files", extensions: "zip" }
                ]
            },
            init: {
                PostInit: function() {
                },
                FilesAdded: function(up, files) {
                    plupload.each(files, function(file) {
                        uploader.start();
                    });
                },
                UploadProgress: function(up, file) {
                },
                Error: function(up, err) {
                    if (err.message) {
                        alert(data.message);
                    }
                }
            }
        });
        uploader.init();
        uploader.bind('FileUploaded', function(upldr, file, object) {
            var data = JSON.parse(object.response);
            console.log(data);
            
            callback(data);
        });
    };
    self.loadVideo = function (buttonId, callback) {
        var uploader = new plupload.Uploader({
            runtimes: 'html5,flash,silverlight,html4',
            browse_button: buttonId,
            url: '/Admin/Home/UploadVideo',
            flash_swf_url: '/Content/plupload-2.1.8/js/Moxie.swf',
            silverlight_xap_url: '/Content/plupload-2.1.8/js/Moxie.xap',
            filters: {
                max_file_size: '2048mb',
                chunk_size: '0',
                mime_types: [
                   // { title: "video files", extensions: ".mp4" },
                    //{ title: "Zip files", extensions: "zip" }
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
                    console.log(file.percent);
                    $("#percent").html(file.percent + '%');
                    $('#process').css('width', file.percent + '%');
                    if (file.percent == 100) {
                        setTimeout(function() {
                            $("#percent").html("");
                            $('#process').css('width', 0 + '%');
                        }, 3000);
                    }
                },
                Error: function (up, err) {
                    if (err.message) {
                        alert(err.message);
                    }
                }
            }
        });
        uploader.init();
        uploader.bind('FileUploaded', function (upldr, file, object) {
            var data = JSON.parse(object.response);
            console.log(data);

            callback(data);
        });
    };
    self.setState = function(url, $a) {
        var id = $a.data("id");
        var state = $a.data("state");
        var $td = $a.parents("td");
        $.post(url, { id: id, state: state }, function(data) {
            if (data == 1) {
                $td.html(" <label class='label label-success'>已通过</label>");
            }
            if (data == 2) {
                $td.html(" <label class='label label-default'>已拒绝</label>");
            }
        });
    };
    self.delayReload = function() {
        setTimeout(function() {
            location.reload();
        }, 2000);
    };
    self.info=function(txt) {
        toastr.info(txt);
    }
    self.lineChartByDay = function (json, title, tooltip, $ele) {
        var d1 = [];
        var dates = [];
        var fy = json.firstYear;
        //for (var i = 0; i <= 10; i += 1) d1.push([i, parseInt(Math.random() * 30)]);
        for (var i = 0; i < json.data.length; i++) {
            d1.push([i, json.data[i].value]);
            dates.push([i, json.data[i].Date]);
        }

        var data = new Array();
        data.push({
            data: d1,
            bars: {
                show: true,
                barWidth: 0.4,
                order: 1,
            }
        });

        // === Make chart === //
         $.plot($ele,
               [{ data: d1, label: title, color: "#ee7951" }], {
                   xaxis: {
                       ticks: dates
                   },
                   series: {
                       lines: { show: true },
                       points: { show: true }
                   },
                   grid: { hoverable: true, clickable: true },
               });
        // === Point hover in chart === //
        var previousPoint = null;
        $ele.bind("plothover", function (event, pos, item) {
            if (item) {
                if (previousPoint != item.dataIndex) {
                    previousPoint = item.dataIndex;

                    $('#tooltip').fadeOut(200, function () {
                        $(this).remove();
                    });
                    var x = item.datapoint[0],
                        y = item.datapoint[1].toFixed(2);

                    maruti.flot_tooltip(item.pageX, item.pageY, dates[x][1] + tooltip + y);
                }

            } else {
                $('#tooltip').fadeOut(200, function () {
                    $(this).remove();
                });
                previousPoint = null;
            }
        });
       

        function simpleDay(x) {
            var day = addDate(fy + "/1/1", x);
            return day;
        }

        function addDate(date, days) {
            var d = new Date(date);
            d.setDate(days);
            var month = d.getMonth() + 1;
            var day = d.getDate();
            if (month < 10) {
                month = "0" + month;
            }
            if (day < 10) {
                day = "0" + day;
            }
            var val = d.getFullYear() + "/" + month + "/" + day;
            return val;
        }


    }

};
maruti = {
    // === Tooltip for flot charts === //
    flot_tooltip: function (x, y, contents) {

        $('<div id="tooltip">' + contents + '</div>').css({
            top: y + 5,
            left: x + 5
        }).appendTo("body").fadeIn(200);
    }
}
window.admin = new Admin();
