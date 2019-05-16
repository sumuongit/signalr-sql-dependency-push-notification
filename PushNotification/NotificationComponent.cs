using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using Microsoft.AspNet.SignalR;
using PushNotification.Models;
using System.Data;

namespace PushNotification
{
    public class NotificationComponent
    {       
        //REGISTERING NOTIFICATION ALONG WITH ADDING SQL DEPENDENCY
        public void RegisterNotification()
        {
            string conString = ConfigurationManager.ConnectionStrings["sqlConString"].ConnectionString;

            //NOTE: [dbo].[PersonInformation] WITH [dbo] IS MANDATORY WHILE USING SQL DEPENDENCY
            string sqlCommand = @"SELECT [PersonID],[PersonName],[PersonPhoneNo],[PersonAddress] FROM [dbo].[PersonInformation]";
            
            using (SqlConnection con = new SqlConnection(conString))
            {
                SqlCommand cmd = new SqlCommand(sqlCommand, con);
               
                if (con.State != System.Data.ConnectionState.Open)
                {
                    con.Open();
                }
                cmd.Notification = null;
                SqlDependency sqlDep = new SqlDependency(cmd);
                sqlDep.OnChange += sqlDep_OnChange;

                //COMMAND EXECUTION IS MANDATORY
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                   
                }
            }
        }

        private void sqlDep_OnChange(object sender, SqlNotificationEventArgs e)
        {
            if (e.Type == SqlNotificationType.Change)
            {
                SqlDependency sqlDep = sender as SqlDependency;
                sqlDep.OnChange -= sqlDep_OnChange;

                //SENDING NOTIFICATION MESSAGE TO CLIENT
                var notificationHub = GlobalHost.ConnectionManager.GetHubContext<NotificationHub>();
                notificationHub.Clients.All.notify("added");

                //RE-REGISTERING NOTIFICATION
                RegisterNotification();
            }
        }

        public List<PersonInformation> GetPersonInfo(DateTime dateTime)
        {
            string conString = System.Configuration.ConfigurationManager.ConnectionStrings["sqlConString"].ConnectionString;
            
            using (SqlConnection con = new SqlConnection(conString))
            {
                SqlCommand cmd = new SqlCommand(@"SELECT [PersonID],[PersonName],[PersonPhoneNo],[PersonAddress] FROM [dbo].[PersonInformation] WHERE [CreateDate] > '" + dateTime + "' order by [CreateDate] DESC", con);

                if (con.State != System.Data.ConnectionState.Open)
                {
                    con.Open();
                }
                
                SqlDataAdapter da = new SqlDataAdapter();
                da.SelectCommand = cmd;
                DataTable dt = new DataTable();
                da.Fill(dt);

                List<PersonInformation> personList = new List<PersonInformation>();
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    PersonInformation person = new PersonInformation();
                    person.PersonID = Convert.ToInt32(dt.Rows[i]["PersonID"]);
                    person.PersonName = dt.Rows[i]["PersonName"].ToString();
                    person.PersonPhoneNo = dt.Rows[i]["PersonPhoneNo"].ToString();
                    person.PersonAddress = dt.Rows[i]["PersonAddress"].ToString();
                    personList.Add(person);
                }

                return personList;               
            }
        }

        // IMPLEMENTING ABOVE METHOD USING LINQ

        //public List<PersonInformation> GetPersonInfo(DateTime dateTime)
        //{
        //    using (PushNotificationDBEntities dbEntities = new PushNotificationDBEntities())
        //    {
        //        return dbEntities.PersonInformations.Where(a => a.CreateDate > dateTime).OrderByDescending(x => x.CreateDate).ToList();
        //    }
        //}
    }
}