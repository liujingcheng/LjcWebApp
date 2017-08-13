//----------------------------------用js控制只能输入整型数据----------------------------
//给class为onlyInt的文本框绑定keypress(键盘按下)事件
$(function () {
    $(document).on("keypress", ".onlyInt", filterInt);
});
//给class为onlyInt的文本框绑定blur(失焦)事件
$(function () {
    $(document).on("blur", ".onlyInt", checkInt);
});
//控制从键盘只能输入整型数据(int型),但此函数无法阻止用户粘贴和拖放
function filterInt(evt) {
    evt = evt || window.event;
    var b = evt.keyCode || evt.which;
    return (b >= 48 && b <= 57) || b == 8;   //0到9的键值为48到57,BackSpace的键值为8
}
//防止用户粘贴和拖放非整型数据(当文本框失焦时检测数据,如果数据为非整型就将其清掉)
function checkInt(event) {
    var patrn = /[^0123456789]/;
    if (patrn.exec(this.value)) {
        alert("输入不合法，请重新输入！");
        this.value = "";
        this.focus();
    }
}
//-----------------------------------------------------------------------------------

//回车时新建任务
$(function () {
    $(document).on("keypress", "#txtNewEventName,.txtNewEventName", function (evt) {
        evt = evt || window.event;
        var keyCode = evt.keyCode || evt.which;

        if (keyCode == 13 && document.activeElement == this) {
            var prevLine = $(this).parent().parent().prev();//保留上一行，用于后面的焦点定位
            var btnNew = $(this).parent().parent().find("#btnNew,.btnNew").first();
            btnNew.click();//新建任务

            //新建了任务后焦点定位在新建任务的文本框
            var focusNextTxtNewEvent = setInterval(function (obj) {
                if (obj.length > 0) {//如果原先的上一行还能找到的话
                    var nextChildren = $(obj).next().next().find("#txtNewEventName,.txtNewEventName");
                    if (nextChildren.length > 0) {
                        $(nextChildren).first().focus();
                    }
                }
                clearInterval(focusNextTxtNewEvent);
            }, 400, prevLine);
        }
    });
});

//已存在任务文本框内回车跳到下一个任务文本框
$(function () {
    $(document).on("keypress", "#txtEventName,.txtEventName,.txtPlanMinutes", function (evt) {
        evt = evt || window.event;
        var keyCode = evt.keyCode || evt.which;

        if (keyCode == 13 && document.activeElement == this) {
            var nextLine = $(this).parent().parent().next();//取下一行
            var nextTxt = nextLine.find(".txtEventName,.txtNewEventName,#txtNewEventName");
            if (nextTxt.length > 0) {
                $(nextTxt).first().focus();
            }
        }
    });
});

//把总秒数转换成时分秒显示
function transferToDateTime(seconds) {
    var hour = 0;
    var minu = 0;
    var sec = 0;
    if (seconds > 3600) {
        hour = parseInt(seconds / 3600);
        seconds = seconds % 3600;
    }
    if (seconds > 60) {
        minu = parseInt(seconds / 60);
        seconds = seconds % 60;
    }
    sec = seconds;
    return hour + ":" + minu + ":" + sec;
}


//----------------------------------给任务添加备注----------------------------
$(function () {
    $(document).on('click', ".btnRemark", function () {
        if (!$("#remarkLine").hasClass("hide")) {
            $("#remarkLine").addClass("hide");
            return false;
        }
        $("#remarkLine").removeClass("hide");
        var currentLine = $(this).parent().parent();
        var remarkLine = $("#txtRemark").parent().parent();
        $(currentLine).after(remarkLine);

        var hddRemark = $(currentLine).find(".hddRemark").first().val();
        $("#txtRemark").val(hddRemark);
        $("#txtRemark").focus();
        return false;
    });

    $(document).on('click', "#btnSaveRemark", function () {
        if ($("#txtRemark").val() == "") {
            $("#txtRemark").focus();
            return;
        }
        var currentLine = $(this).parent().parent().prev();
        var hddEventId = $(currentLine).find(".hddEventId").first();
        var remark = $("#txtRemark").val();

        var ajaxResult;
        $.ajax({
            url: '/TimeStatistic/UpdateRemark',
            cache: false,
            async: false,
            type: 'Post',
            data: { eventId: $(hddEventId).val(), remark: remark },

            success: function (data, status, xhr) {
                ajaxResult = data;
            },
            error: function (data, status, e) {
                alert("ajax error");
            }
        });

        if (ajaxResult == 0) {
            $("#remarkLine").addClass("hide");
        }
        else {
            alert("UpdateRemark failed");
        }
    });

    //回车时点保存按钮
    $(document).on("keypress", "#txtRemark", function (evt) {
        evt = evt || window.event;
        var keyCode = evt.keyCode || evt.which;

        if (keyCode == 13 && document.activeElement == this) {
            $("#btnSaveRemark").click();
        }
    });
});
//-----------------------------------------------------------------------------------
