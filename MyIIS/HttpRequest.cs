using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyIIS
{
    public class HttpRequest
    {
        public  HttpRequest(string requestStr)
        {
            string[] lines = requestStr.Replace("\r\n", "\r").Split('\r');
            HttpMethod = lines[0].Split(' ')[0];//Post 或是 GET
            Url = lines[0].Split(' ')[1];
            HttpVersion = lines[0].Split(' ')[2];
        }

        public string HttpMethod { get; set; }
        public string Url { get; set; }
        public string HttpVersion { get; set; }
        public Dictionary<string, string> HeaderDictionary { get; set; }
        public Dictionary<string, string> BodyDictionary { get; set; }
    }
}
