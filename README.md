## Table of Contents
* [General Info](#general-info)
* [Features](#features)
* [Implementation](#implementation)
* [Database Scripts](#database-scripts)
* [Technologies](#technologies)
* [Setup](#setup)
* [Contributing](#contributing)
* [License](#license)

## General Info
A sample real time push notification application

## Features
* Notification Alert
* Notification Details
	
## Implementation
The steps as following are the implementation of the real time push notification using SignalR and SQL Dependecy.

**Step 1: Enable Service Broker**<br/>
Under the PushNotificationDB database, execute the following query to enable the service broker.
```
ALTER DATABASE PushNotificationDB SET ENABLE_BROKER WITH ROLLBACK IMMEDIATE;
```
**Step 2: Start and Stop SQL Dependency**<br/>
In the Global.asax file, start the SQL Dependency in the App_Start() event and Stop the SQL Dependency in the Application_End() event.
```
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
```
**Step 3: Install SignalR**<br/>
In the Package Manager Console, run the following command.
```
PM> Install-Package Microsoft.AspNet.SignalR
```
**Step 4: Enable SignalR**<br/>
In the Owin Startup Class, enable the SignalR as following.
```
public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            app.MapSignalR();
        }
    }
```
**Step 5: Add SignalR Hub Class**<br/>
Add the SignalR Hub Class for communicating between the server and the client.
```
public class NotificationHub : Hub
    {
       
    }
```
**Step 6: Register Notification with SQL Dependency**<br/>
By adding the following server side code we are going to register the notification with the SQL Dependency.
```
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
    }
```
**Step 7: Retrieve Changed Data**<br/>
Add the following controller action method for retrieving the changed data just happened in the database.
```
public JsonResult GetNotification()
        {
            var notificationRegisterTime = Session["LastTimeNotified"] != null ? Convert.ToDateTime(Session["LastTimeNotified"]) : DateTime.Now;
            NotificationComponent notiCom = new NotificationComponent();
            var list = notiCom.GetPersonInfo(notificationRegisterTime);
          
            //UPDATE SESSION FOR GETTING NEWLY ADDED INFORMATION ONLY
            Session["LastTimeNotified"] = DateTime.Now;
            return new JsonResult { Data = list, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
        }
```
**Step 8: SignalR Client Scripts**<br/>
```
<script src="~/Scripts/jquery.signalR-2.2.0.min.js"></script>
<script src="/signalr/hubs"></script>
<script type="text/javascript">
        $(function () {
            //CLICK ON NOTIFICATION ICON FOR SHOWING NOTIFICATION DETAILS
            $('span.noti').click(function (e) {
                e.stopPropagation();
                $('.noti-content').show();
                var count = 0;
                count = parseInt($('span.count').html()) || 0;

                //LOAD NOTIFICATION IF NOT LOADED ALREADY
                if (count > 0) {
                    updateNotification();                   
                }
                $('span.count', this).html('&nbsp;');              
                $('span.count').css('background-color', '');               
                $('span.count').css('padding', '');               
            })

            //HIDE NOTIFICATION
            $('html').click(function () {
                $('.noti-content').hide();
            })

            //UPDATE NOTIFICATION
            function updateNotification() {
                $('#notiContent').empty();
                $('#notiContent').append($('<li>Loading...</li>'));

                $.ajax({
                    type: 'GET',
                    url: '/home/GetNotification',
                    success: function (response) {
                        $('#notiContent').empty();
                        if (response.length  == 0) {
                            $('#notiContent').append($('<li>No data available!!!</li>'));
                        }
                        $.each(response, function (index, value) {
                            $('#notiContent').append($('<li><b>Name:</b> ' + value.PersonName + ' <b>Phone:</b> ' + value.PersonPhoneNo + ' <b>Address:</b> ' + value.PersonAddress + '</li>'));
                        });
                    },
                    error: function (error) {
                        console.log(error);
                    }
                })
            }

            //UPDATE NOTIFICATION COUNT
            function updateNotificationCount() {
                var count = 0;
                count = parseInt($('span.count').html()) || 0;
                count++;
                $('span.count').html(count);               
                $('span.count').css('background-color', '#fa3e3e');              
                $('span.count').css('padding', '0px 4px 4px 4px');               
            }

            //SIGNALR JAVASCRIPT CODE FOR STARTING HUB
            var notificationHub = $.connection.notificationHub;
            $.connection.hub.start().done(function () {
                console.log('Notification hub started');
            });

            //SIGNALR JAVASCRIPT CODE FOR COMPARING MESSAGE
            notificationHub.client.notify = function (message) {
                if (message && message.toLowerCase() == "added") {
                    updateNotificationCount();
                }
            }
        })
    </script>
```
**Step 9: Run Application**<br/>
After running the application execute the following `INSERT` query for adding a person information into the database
and see the real time push notifications as like as the screen shots given bellow.
```
USE [PushNotificationDB]
GO

INSERT INTO [dbo].[PersonInformation]
           ([PersonName]
           ,[PersonPhoneNo]
           ,[PersonAddress]
           ,[CreateDate])
     VALUES
           ('Person Name'
           ,'Person Phone No'
           ,'Person Address'
           ,GETDATE())
GO
```
**Notification Alert:**
![Push Notification](https://github.com/sumuongit/signalr-sql-dependency-push-notification/blob/master/PushNotification/Images/PN1.PNG)

**Notification Detail:**
![Push Notification](https://github.com/sumuongit/signalr-sql-dependency-push-notification/blob/master/PushNotification/Images/PN2.PNG)

## Database Scripts
[Database Scripts](https://github.com/sumuongit/signalr-sql-dependency-push-notification/tree/master/PushNotification/Database)


## Technologies
This application is created with:
* Visual Studio 2013
* C# 
* ASP.NET SignalR
* SQL Dependency
	
## Setup
To clone and run this repository you will need [Git](https://git-scm.com/) installed on your computer. From your command line:

```
# Clone this repository
$ git clone https://github.com/sumuongit/signalr-sql-dependency-push-notification.git
# Go into the repository
$ cd signalr-sql-dependency-push-notification

```

## Contributing
* Fork the repository
* Create a topic branch
* Implement your feature or bug fix
* Add, commit, and push your changes
* Submit a pull request

## License
[MIT License](https://github.com/sumuongit/signalr-sql-dependency-push-notification/blob/master/LICENSE)
