using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Management.filter
{
    public class LoginFilter : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            //HttpContext.Current.Response.Write("OnActionExecuting:正要准备执行Action的时候但还未执行时执行");
            if(HttpContext.Current.Session["admin"]==null)
            {
                HttpContext.Current.Response.Write("<script>alert('请先登录');location.href='/Login/Index'</script>");
            }
        }

        public override void OnActionExecuted(ActionExecutedContext filterContext)
        {
            //HttpContext.Current.Response.Write("OnActionExecuted:Action执行时但还未返回结果时执行");
        }

        public override void OnResultExecuting(ResultExecutingContext filterContext)
        {
            //HttpContext.Current.Response.Write("OnResultExecuting:OnResultExecuting也和OnActionExecuted一样，但前者是在后者执行完后才执行");
        }

        public override void OnResultExecuted(ResultExecutedContext filterContext)
        {
            //HttpContext.Current.Response.Write("OnResultExecuted:是Action执行完后将要返回ActionResult的时候执行");
        }

    }
}