using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyIIS
{
    //约定后缀  demo.wa
    class DemoClass : IHttpHandler
    {
        public void ProcessRequest(HttpContext context)
        {
            string str = string.Format(@"<html><head></head><body><h1>{0}</h1></body></html>", DateTime.Now.ToString());
            context.Response.Body = Encoding.Default.GetBytes(str);
            context.Response.StateCode = "200";
            context.Response.StateDes = "OK";
            context.Response.ContentType = "text/html";
        }
    }
}
