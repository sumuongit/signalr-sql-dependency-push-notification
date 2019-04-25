using System;
using System.Web.Mvc;

namespace PushNotification.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }

        public JsonResult GetNotification()
        {
            var notificationRegisterTime = Session["LastTimeNotified"] != null ? Convert.ToDateTime(Session["LastTimeNotified"]) : DateTime.Now;
            NotificationComponent notiCom = new NotificationComponent();
            var list = notiCom.GetPersonInfo(notificationRegisterTime);
          
            //UPDATE SESSION FOR GETTING NEWLY ADDED INFORMATION ONLY
            Session["LastTimeNotified"] = DateTime.Now;
            return new JsonResult { Data = list, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
        }
	}
}