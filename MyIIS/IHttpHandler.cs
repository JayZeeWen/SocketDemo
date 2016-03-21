using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MyIIS
{
    public interface IHttpHandler
    {
        void ProcessRequest(HttpContext context);
    }
}
