using System.Web;
using System.Web.Mvc;
using Management.Models;

namespace Management
{
    public class FilterConfig
    {
        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {   
            //filters.Add(new HttpResponseExceptionAttribute());  
            //注册异常处理过滤器。  
            filters.Add(new MyExceptionFileAttribute());  
            filters.Add(new HandleErrorAttribute());

        }
    }
}