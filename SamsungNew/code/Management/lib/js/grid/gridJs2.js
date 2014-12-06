/* 
 * To change this template, choose Tools | Templates
 * and open the template in the editor.
 */
(function ($) {
    $.grid2 = {
        f_ajaxSubmit: function (url, params, tips, flag) {
            $.ajax({
                url: url,
                type: 'post',
                data: params,
                dataType: 'json',
                success: function () {
                    $.grid2.f_showTips(tips);
                    if (flag) {
                        $("#pager2 div.l-bar-group .l-bar-btnload").click();
                    }
                },
                error: function () {
                    $.grid2.f_showTips("系统出错！");
                }
            });
        },

        f_showTips: function (text) {
            var dialog = $.ligerDialog.tip({
                title: '提示信息',
                content: text
            });
            setTimeout(function () {
                dialog.close();
            }, 4000);
        },

        f_getData: function (grid, url, params) {
            $.ajax({
                url: url,
                type: 'post',
                data: params,
                dataType: 'json',
                success: function (data) {
                    var gdata = {};
                    gdata.Rows = data.gdata;
                    grid.set({
                        data: gdata
                    });
                    $("#titleTime").text(data.showTime);
                    $.grid2.f_setPagerClass(data.page, data.totalPage, data.pageBtn, data.pageMsg);
                    $.grid2.f_setPageParam(data.page, data.pageSize, data.totalPage);
                    $("#pageloading2").hide();
                },
                error: function () {
                    $.grid2.f_showTips("系统出错！数据加载失败！");
                    $("#pageloading2").hide();
                }
            });
        },

        f_setPagerClass: function (page, totalpage, pageBtn, pageMsg) {
            var style = pageBtn.split("#");
            $("#l-bar-text2").html(pageMsg);
            $("#btnFirst2").attr("class", style[0]);
            $("#btnprev2").attr("class", style[0]);
            $("#btnnext2").attr("class", style[1]);
            $("#btnlast2").attr("class", style[1]);
            $("#showPage2").val(page);
            $("#showTotalPage2").html(totalpage);
            $("div.l-grid1 .l-grid-hd-row").removeClass("l-checked");
        },

        f_setOverClass: function () {
            $("div.a .l-bar-button").mouseover(function () {
                $(this).addClass("l-bar-button-over");
            });
            $("div.a .l-bar-button").mouseout(function () {
                $(this).removeClass("l-bar-button-over");
            });
        },

        f_setPageParam: function (page, pageSize, totalPage) {
            $("#page2").val(page);
            $("#pageSize2").val(pageSize);
            $("#totalPage2").val(totalPage);
        },

        f_pagerOnclick: function (grid, url, params) {
            $("#pager2 div.a .l-bar-button").bind("click", function () {
                var style = $(this).children("span").attr("class");
                if (style == "l-disabled") {
                    return false;
                } else if (style == "l-abled") {
                    //$("#pageloading2").show();
                    var page, pageSize;
                    if ($(this).hasClass("l-bar-btnfirst")) {
                        page = 1;
                        pageSize = $("#pageSize2").val();
                    } else if ($(this).hasClass("l-bar-btnprev")) {
                        page = parseInt($("#page2").val(), 10) - 1;
                        pageSize = $("#pageSize2").val();
                    } else if ($(this).hasClass("l-bar-btnnext")) {
                        page = parseInt($("#page2").val(), 10) + 1;
                        pageSize = $("#pageSize2").val();
                    } else if ($(this).hasClass("l-bar-btnlast")) {
                        page = $("#totalPage2").val();
                        pageSize = $("#pageSize2").val();
                    } else if ($(this).hasClass("l-bar-btnload")) {
                        page = $("#page2").val();
                        pageSize = $("#pageSize2").val();
                    }
                    params["page"] = page;
                    params["pageSize"] = pageSize;
                    params["content"] = $("#content2").val();
                    params["actionTime"] = $("#actionTime").val();
                    $.grid2.f_getData(grid, url, params);
                }
                return true;
            });

        },

        f_pageEnterDown: function (grid, url, params) {
            $("div.a .pcontrol input").change(function () {
                //$("#pageloading2").show();
                var page = $(this).val();
                var pageSize = $("#pageSize2").val();
                params["page"] = page;
                params["pageSize"] = pageSize;
                params["content"] = $("#content2").val();
                params["actionTime"] = $("#actionTime").val();
                $.grid2.f_getData(grid, url, params);
            });
        },

        f_pageSizeSelect: function (grid, url, params) {
            $("#rp2").change(function () {
                //$("#pageloading2").show();
                var page = $("#page2").val();
                var pageSize = $(this).val();
                params["page"] = page;
                params["pageSize"] = pageSize;
                params["content"] = $("#content2").val();
                params["actionTime"] = $("#actionTime").val();
                $.grid2.f_getData(grid, url, params);
            });
        }
    };
})(jQuery);
