using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;


namespace WorkOrderWizard.Controllers
{
    public class HomeController : Controller
    {

        public ActionResult Index()
        {

            return View("CreateWorkOrders");
        }

        public ActionResult CreateWorkOrders()
        {
            return View();
        }

        [HttpGet]
        public ActionResult ViewWorkOrders()
        {
            return View();
        }
    }
}
