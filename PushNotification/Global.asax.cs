using System;
using System.Configuration;
using System.Data.SqlClient;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace PushNotification
{
    public class MvcApplication : System.Web.HttpApplication
    {
        string con = ConfigurationManager.ConnectionStrings["sqlConString"].ConnectionString;
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            RouteConfig.RegisterRoutes(RouteTable.Routes);

            //START SQL DEPENDENCY
            SqlDependency.Start(con);
        }

        protected void Session_Start(object sender, EventArgs e)
        {
            NotificationComponent notiCom = new NotificationComponent();
            var currentDateTime = DateTime.Now;
            HttpContext.Current.Session["LastTimeNotified"] = currentDateTime;
            notiCom.RegisterNotification();
        }
        protected void Application_End()
        {
            //STOP SQL DEPENDENCY
            SqlDependency.Stop(con);
        }
    }
}
