using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MyIIS
{
    public class HttpResponse
    {
        public string StateCode { get; set; }
        public string StateDes { get; set; }
        public string ContentType { get; set; }
        //相应正文
        public byte[] Body { get; set; }
        public byte[] GetResponseHeader()
        {
            string strRequest = string.Format( @"HTTP/1.1 {0} {1}
                                    Cache-Control: private
                                    Content-Type: {2}; 
                                    Content-Encoding: gzip
                                    Vary: Accept-Encoding
                                    Server: Microsoft-IIS/7.5
                                    X-AspNet-Version: 4.0.30319
                                    Set-Cookie: ASP.NET_SessionId=ia50fz5psolur5vgs01y2322; path=/; HttpOnly
                                    X-Powered-By: ASP.NET
                                    Date: Mon, 21 Mar 2016 14:12:37 GMT
                                    Content-Length: {3}
                                                        
                                    ",StateCode,StateDes,ContentType,Body.Length);
            return Encoding.Default.GetBytes(strRequest);
        }
    }
}
