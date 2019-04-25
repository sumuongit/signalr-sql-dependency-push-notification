## Table of contents
* [General info](#general-info)
* [Technologies](#technologies)
* [Setup](#setup)
* [License](#license)

## General info
A sample real time push notification application

**NOTE:** After running the application please execute the following `INSERT` query for adding a person information into the database
and see the real time push notifications as the screen shots given bellow.

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
**Notification:**
![Push Notification](https://github.com/sumuongit/signalr-sql-dependency-push-notification/blob/master/PushNotification/Images/PN1.PNG)

**Notification Details:**
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
To run this application, building the source locally using git:

```
$ git clone https://github.com/sumuongit/signalr-sql-dependency-push-notification.git

```

## License
[MIT License](https://github.com/sumuongit/signalr-sql-dependency-push-notification/blob/master/LICENSE)
