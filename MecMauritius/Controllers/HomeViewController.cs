using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace MecMauritius.Controllers
{
    public class HomeViewController : Controller
    {
        // GET: HomeView
        [HttpGet]
        public ActionResult HomeIndex()
        {
            return View();
        }
        [HttpGet]
        public ActionResult AboutUs()
        {
            return View();
        }
    }
}