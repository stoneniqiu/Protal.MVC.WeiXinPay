(function() {
    var $ele = $(".datebox");
    var $valueEle = $("#VerifyTime");
    Date.prototype.format = function(format) {
        var o = {
            "M+": this.getMonth() + 1, //month
            "d+": this.getDate(), //day
            "h+": this.getHours(), //hour
            "m+": this.getMinutes(), //minute
            "s+": this.getSeconds(), //second
            "q+": Math.floor((this.getMonth() + 3) / 3), //quarter
            "S": this.getMilliseconds() //millisecond
        }
        if (/(y+)/.test(format))
            format = format.replace(RegExp.$1,
                (this.getFullYear() + "").substr(4 - RegExp.$1.length));
        for (var k in o)
            if (new RegExp("(" + k + ")").test(format))
                format = format.replace(RegExp.$1,
                    RegExp.$1.length == 1 ? o[k] :
                        ("00" + o[k]).substr(("" + o[k]).length));
        return format;
    };
    function is_leap(year) {
        return (year % 100 == 0 ? res = (year % 400 == 0 ? 1 : 0) : res = (year % 4 == 0 ? 1 : 0));
    } //是否为闰年
    var nstr = new Date(); //当前Date资讯
    var ynow = nstr.getFullYear(); //年份
    var mnow = nstr.getMonth(); //月份
    var dnow = nstr.getDate(); //今日日期
    var n1str = new Date(ynow, mnow, 1); //当月第一天Date资讯
    var firstday = n1str.getDay(); //当月第一天星期几
    var m_days = new Array(31, 28 + is_leap(ynow), 31, 30, 31, 30, 31, 31, 30, 31, 30, 31); //各月份的总天数
    var tr_str = Math.ceil((m_days[mnow] + firstday) / 7); //表格所需要行数

    function pain() {
        //打印表格第一行（有星期标志）
        $("#datetb").remove();
        var str = "<table id='datetb' cellspacing='0'><tr><td>周日</td><td>周一</td><td>周二</td><td>周三</td><td>周四</td><td>周五</td><td>周六</td></tr>";
        for (i = 0; i < tr_str; i++) { //表格的行
            str += "<tr>";
            for (k = 0; k < 7; k++) { //表格每行的单元格
                idx = i * 7 + k; //单元格自然序列号
                date_str = idx - firstday + 1; //计算日期
                (date_str <= 0 || date_str > m_days[mnow]) ? date_str = "&nbsp;" : date_str = idx - firstday + 1; //过滤无效日期（小于等于零的、大于月总天数的）
                //打印日期：今天底色样式
                date_str == dnow ? str += "<td class='ab' data-day=" + date_str + ">" + "<div>" + date_str + "</div>" + "<div class='subscribe'>预约</div>" + "</td>" : str += "<td  data-day=" + date_str + ">" + date_str + "</td>";
            }
            str += "</tr>"; //表格的行结束
        }
        str += "<tfoot><tr><td colspan='7'>" + ynow + "年" + (mnow+1) + "月</td></tr></tfoot>";
        str += "</table>"; //表格结束
        $ele.html(str);
    }

    function setDate(y,m,d) {
        var current = (new Date(y, m, d,10,0,0)).format("yyyy-MM-dd");;
        console.log(y, m, d, current);
        $valueEle.val(current);
    }

    $ele.on("click", "table td", function() {
        if (!$(this).hasClass('ab')) {
            var day = $(this).data("day");
            var now = new Date();
            if (now.getMonth() == mnow && day < now.getDate()) {
                return;
            }
            $(".datebox table td").removeClass('ab').children('.subscribe').remove();
            $(this).addClass('ab');
            $(this).html('<div>' + $(this).html() + '</div><div class="subscribe">预约</div>');
            setDate(ynow, mnow, day);
        }
    });
    $(".databox #up").click(function () {
        var temp = mnow - 1;
        if (temp < 0) {
            mnow = 11;
            ynow--;
        } else {
            //过期无效时间 处理
            if (temp < (new Date()).getMonth()) {
                return;
            }
            mnow--;
        }
        console.log(mnow);
        pain();
    });
    $(".databox #down").click(function () {
        var temp = mnow + 1;
        if (temp > 11) {
            mnow = 0;
            ynow++;
        } else {
            //最多提前三个月
            if (temp > (new Date()).getMonth()+3) {
                return;
            }
            mnow++;
        }
        console.log(mnow);
        pain();
    });

    pain();
})()