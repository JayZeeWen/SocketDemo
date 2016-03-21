using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace MyIIS
{
    public class HttpApplication : IHttpHandler
    {
        /// <summary>
        /// 处理当前http请求
        /// </summary>
        /// <param name="context"></param>
        public void ProcessRequest(HttpContext context)
        {
            //获得请求信息
            //获得请求的文件
            string basePath = AppDomain.CurrentDomain.BaseDirectory;
            string fileName = Path.Combine(basePath, context.Request.Url.TrimStart('/'));

            #region 处理动态文件
            string ext =  Path.GetExtension(context.Request.Url);
            if(ext == ".wa")
            {
                var className = Path.GetFileNameWithoutExtension(context.Request.Url);
                //反射
                var obj = Assembly.Load("MyIIS").CreateInstance("MyIIS." + className);
                var demo = obj as IHttpHandler;
                demo.ProcessRequest(context);
                return;
            }

            #endregion

            
            #region 处理静态文件
            if (File.Exists(fileName))
            {
                context.Response.StateCode = "200";
                context.Response.StateDes = "OK";
                context.Response.ContentType = GetContenType(Path.GetExtension(context.Request.Url));
                context.Response.Body = File.ReadAllBytes(fileName);
            }
            else
            {
                context.Response.StateCode = "404";
                context.Response.StateDes = "Not Found";
                context.Response.ContentType = "text/html";
                context.Response.Body = Encoding.Default.GetBytes(@"<html><body>文件不存在</body></html>");
            }    
            #endregion       

        }

        //获取请求文件的类型
        public string GetContenType(string ext)
        {
            string type = "text/html; charset=UTF-8";
            switch (ext)
            {
                case ".wa":
                case ".aspx":
                case ".html":
                case ".htm":
                    type = "text/html; charset=UTF-8";
                    break;
                case ".png":
                    type = "image/png";
                    break;
                case ".gif":
                    type = "image/gif";
                    break;
                case ".jpg":
                case ".jpeg":
                    type = "image/jpeg";
                    break;
                case ".css":
                    type = "text/css";
                    break;
                case ".js":
                    type = "application/x-javascript";
                    break;
                default:
                    type = "text/plain; charset=gbk";
                    break;
            }
            return type;
        }
    }
}
