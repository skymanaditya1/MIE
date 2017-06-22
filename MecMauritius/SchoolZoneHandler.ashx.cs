using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MecMauritius
{
    /// <summary>
    /// Summary description for SchoolZoneHandler
    /// </summary>
    public class SchoolZoneHandler : IHttpHandler
    {

        public void ProcessRequest(HttpContext context)
        {
            context.Response.ContentType = "text/plain";
            context.Response.Write("Hello World");
        }

        public bool IsReusable
        {
            get
            {
                return false;
            }
        }
    }
}