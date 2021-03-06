
CREATE DATABASE PushNotificationDB;

USE [PushNotificationDB]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[PersonInformation](
	[PersonID] [int] IDENTITY(1,1) NOT NULL,
	[PersonName] [nvarchar](50) NULL,
	[PersonPhoneNo] [nvarchar](50) NULL,
	[PersonAddress] [nvarchar](150) NULL,
	[CreateDate] [datetime] NULL,
CONSTRAINT [PK_PersonInformation] PRIMARY KEY CLUSTERED 
(
	[PersonID] ASC
)
) ON [PRIMARY]
GO
