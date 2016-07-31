$(document).ready(function () {
    InitZeroClip();
});

function InitZeroClip () {

    var client = new ZeroClipboard($('.copy-button'), { moviePath: "/Scripts/ZeroClipboard.swf" }, { forceHandCursor: true });

    client.on('datarequested', function () {
        client.setText($(this).parent().attr('copyText').replace(new RegExp("/", "gm"), "\\"));
    });

    client.on('complete', function () {
        $(this).parent().popover("show");
        setTimeout(function () { $('.popover').hide(); }, 1000);
    });

    client.on('wrongflash noflash', function () {
        //"当前浏览器不支持'复制'按钮的功能,可能因为未安装flash或者安装的flash版本不支持该功能"
        ZeroClipboard.destroy();
        $(".copy-button-wrap ").remove();
    });
}