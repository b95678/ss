/* 
 * To change this template, choose Tools | Templates
 * and open the template in the editor.
 */
(function($){
    $.grid = {
        f_ajaxSubmit:function(url,params,tips,flag){
            $.ajax({
                url:url,
                type:'post',
                data:params,
                dataType:'json',
                success:function(){
                    $.grid.f_showTips(tips);
                    if(flag){
                        $("#pager div.l-bar-group .l-bar-btnload").click();
                    }
                },
                error:function(){
                    $.grid.f_showTips("系统出错！");
                }
            });
        },
        
        f_showTips:function(text){
            var dialog = $.ligerDialog.tip({
                title: '提示信息',
                content: text
            });
            setTimeout(function(){
                dialog.close();
            },4000);
        },
        
        f_getData:function(grid,url,params){       
            $.ajax({
                url:url,
                type:'post',
                data:params,
                dataType:'json',
                success:function(data){
                    var gdata = {};
                    gdata.Rows = data.gdata;
                    grid.set({
                        data:gdata
                    });
                    $.grid.f_setPagerClass(data.page,data.totalPage,data.pageBtn,data.pageMsg);
                    $.grid.f_setPageParam(data.page,data.pageSize,data.totalPage);
                    $("#pageloading").hide();
                },
                error: function () {
                    $.grid.f_showTips("系统出错！数据加载失败！");
                    $("#pageloading").hide();
                }
            });
        },
        
        f_setPagerClass:function(page,totalpage,pageBtn,pageMsg){
            var style = pageBtn.split("#");
            $("div.l-bar-group span.l-bar-text").html(pageMsg);
            $("div.l-bar-group div.l-bar-btnfirst span").attr("class",style[0]);
            $("div.l-bar-group div.l-bar-btnprev span").attr("class",style[0]);
            $("div.l-bar-group div.l-bar-btnnext span").attr("class",style[1]);
            $("div.l-bar-group div.l-bar-btnlast span").attr("class",style[1]);
            $("div.l-bar-group span.pcontrol input").val(page);
            $("div.l-bar-group span.pcontrol span").html(totalpage);
            $("div.l-grid1 .l-grid-hd-row").removeClass("l-checked");
        },

        f_setOverClass:function(){
            $("div.l-bar-group .l-bar-button").mouseover(function(){
                $(this).addClass("l-bar-button-over");
            });
            $("div.l-bar-group .l-bar-button").mouseout(function(){
                $(this).removeClass("l-bar-button-over");
            });
        },

        f_setPageParam:function(page,pageSize,totalPage){
            $("#page").val(page);
            $("#pageSize").val(pageSize);
            $("#totalPage").val(totalPage);
        },

        f_pagerOnclick:function(grid,url,params){
            $("#pager div.l-bar-group .l-bar-button").bind("click",function(){               
                var style = $(this).children("span").attr("class");
                if(style == "l-disabled") {
                    return false;
                }else if(style == "l-abled"){
                    $("#pageloading").show();
                    var page,pageSize;
                    if($(this).hasClass("l-bar-btnfirst")){
                        page = 1;
                        pageSize = $("#pageSize").val();
                    }else if($(this).hasClass("l-bar-btnprev")){
                        page = parseInt($("#page").val(), 10) - 1;
                        pageSize = $("#pageSize").val();
                    }else if($(this).hasClass("l-bar-btnnext")){
                        page = parseInt($("#page").val(), 10) + 1;
                        pageSize = $("#pageSize").val();
                    }else if($(this).hasClass("l-bar-btnlast")){
                        page = $("#totalPage").val();
                        pageSize = $("#pageSize").val();
                    }else if($(this).hasClass("l-bar-btnload")){
                        page = $("#page").val();
                        pageSize = $("#pageSize").val();
                    }
                    params["key"] = $("#key").val();
                    params["content"] = $("#content").val();
                    params["begin"] = $("#begin").val();
                    params["end"] = $("#end").val();
                    params["office"] = $("#office").val();
                    params["work"] = $("#work").val();
                    params["level"] = $("#level").val();
                    params["page"] = page;
                    params["pageSize"] = pageSize;
                    $.grid.f_getData(grid,url,params);
                }
                return true;
            });

        },

        f_pageEnterDown:function(grid,url,params){
            $("div.l-bar-group .pcontrol input").change(function(){
                $("#pageloading").show();
                var page = $(this).val();
                var pageSize = $("#pageSize").val();
                params["key"] = $("#key").val();
                params["content"] = $("#content").val();
                params["begin"] = $("#begin").val();
                params["end"] = $("#end").val();
                params["office"] = $("#office").val();
                params["work"] = $("#work").val();
                params["level"] = $("#level").val();
                params["page"] = page;
                params["pageSize"] = pageSize;
                $.grid.f_getData(grid,url,params);
            });
        },

        f_pageSizeSelect:function(grid,url,params){
            $("div.l-bar-group select").change(function(){
                $("#pageloading").show();
                var page = $("#page").val();
                var pageSize = $(this).val();
                params["page"] = page;
                params["pageSize"] = pageSize;
                params["key"] = $("#key").val();
                params["content"] = $("#content").val();
                params["begin"] = $("#begin").val();
                params["end"] = $("#end").val();
                params["office"] = $("#office").val();
                params["work"] = $("#work").val();
                params["level"] = $("#level").val();
                $.grid.f_getData(grid,url,params);
            });
        }
    };
})(jQuery);

