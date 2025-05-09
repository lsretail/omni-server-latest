-- use  collation  SQL_Latin1_General_CP1_CI_AS   not Icelandic_CI_AS   use COLLATE DATABASE_DEFAULT in select from #temp tables  
/*
if (SERVERPROPERTY('IsFullTextInstalled') = 0)
 Begin;
  RAISERROR ('Full-Text search is not installed. Sql Server must have Full-Text Search feature installed to continue...', -- Message text.
               16, -- Severity.
               1 -- State.
               );
 End 
 */

----------------------------
 -- OneList 
----------------------------
IF EXISTS(SELECT * FROM sysobjects WHERE name = N'OneList' and xtype='U')
    DROP TABLE [dbo].OneList
GO
CREATE TABLE [dbo].OneList(
    [Id] nvarchar(50) NOT NULL,
	[ExternalType] int NOT NULL DEFAULT(0),
	[IsHospitality] tinyint,
    [Description] nvarchar(100) NOT NULL,
    [StoreId] nvarchar(50) NULL DEFAULT (''),
    [SalesType] nvarchar(20) NULL DEFAULT (''),
    [Currency] nvarchar(5) NULL DEFAULT (''),
	[ListType] int NOT NULL DEFAULT(0),  -- 0= Basket, 1=wishlist
	[TotalAmount] decimal(19, 8),
    [TotalNetAmount] decimal(19, 8),
    [TotalTaxAmount] decimal(19, 8),
    [TotalDiscAmount] decimal(19, 8),
    [ShippingAmount] decimal(19, 8),
    [PointAmount] decimal(19, 8),
    [CreateDate] datetime NOT NULL DEFAULT getdate(),
	[LastAccessed] datetime NULL,
PRIMARY KEY NONCLUSTERED 
( [Id] ASC) WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON)  
) ON [PRIMARY] 
GO

----------------------------
 --  OneListItem  ----JIJ v2.1 added  
----------------------------
IF EXISTS(SELECT * FROM sysobjects WHERE name = N'OneListItem' and xtype='U')
    DROP TABLE [dbo].[OneListItem]
GO
CREATE TABLE [dbo].[OneListItem](
    [Id] nvarchar(50) NOT NULL,
    [OneListId] nvarchar(50) NOT NULL, 
	[DisplayOrderId] int NOT NULL,
    [ItemId] nvarchar(20) NOT NULL,
	[ItemDescription] nvarchar(50) DEFAULT(''),
	[Location] nvarchar(30) DEFAULT(''),
    [BarcodeId] nvarchar(20) NOT NULL DEFAULT(''),
    [UomId] nvarchar(10) DEFAULT(''),
    [VariantId] nvarchar(20) DEFAULT(''),
	[VariantDescription] nvarchar(50) DEFAULT(''),
    [ProductGroup] nvarchar(20) DEFAULT(''),
    [ItemCateGOry] nvarchar(20) DEFAULT(''),
	[IsADeal] tinyint,
    [IsManual] tinyint,
    [Immutable] tinyint,
	[ImageId] nvarchar(20) DEFAULT(''),
    [Quantity] decimal(19, 8) NOT NULL,
	[NetPrice] decimal(19, 8),
    [Price] decimal(19, 8),
    [NetAmount] decimal(19, 8),
    [TaxAmount] decimal(19, 8),
    [DiscountAmount] decimal(19, 8),
    [DiscountPercent] decimal(19, 8),
    [CreateDate] datetime NOT NULL DEFAULT getdate(),
PRIMARY KEY NONCLUSTERED 
( [Id] ASC) WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) 
) ON [PRIMARY] 
GO
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[OneListItem]') AND name = N'IX_OneListId_OneListItem')
CREATE NONCLUSTERED INDEX [IX_OneListId_OneListItem] ON [dbo].[OneListItem] 
(
    [OneListId] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
GO  

----------------------------
 --  OneListOffer
----------------------------
IF EXISTS(SELECT * FROM sysobjects WHERE name = N'OneListOffer' and xtype='U')
    DROP TABLE [dbo].[OneListOffer]
GO
CREATE TABLE [dbo].[OneListOffer](
    [OfferId] nvarchar(20) NOT NULL,
    [OfferType] int NOT NULL,
    [OneListId] nvarchar(50) NOT NULL, 
	[DisplayOrderId] int NOT NULL,
    [CreateDate] datetime NOT NULL DEFAULT getdate(),
PRIMARY KEY NONCLUSTERED 
( [OfferId],[OneListId] ASC) WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) 
) ON [PRIMARY] 
GO
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[OneListOffer]') AND name = N'IX_OneListId_OneListOffer')
CREATE NONCLUSTERED INDEX [IX_OneListId_OneListOffer] ON [dbo].[OneListOffer] 
(
    [OneListId] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
GO 

----------------------------
 --  OneListItemDiscount 
----------------------------
IF EXISTS(SELECT * FROM sysobjects WHERE name = N'OneListItemDiscount' and xtype='U')
    DROP TABLE [dbo].[OneListItemDiscount]
GO
CREATE TABLE [dbo].[OneListItemDiscount](
	[Id] nvarchar(50) NOT NULL, 
    [OneListId] nvarchar(50) NOT NULL, 
	[OneListItemId] nvarchar(50) NOT NULL,
    [LineNumber] int NOT NULL,
    [No] nvarchar(20) NOT NULL, 
	[DiscountType] int NOT NULL,
	[PeriodicDiscType] int NOT NULL,
	[PeriodicDiscGroup] nvarchar(20) NOT NULL,
	[Description] nvarchar(20) NOT NULL,
	[DiscountAmount] decimal(19, 8) NOT NULL,
	[DiscountPercent] decimal(19, 8) NOT NULL,
	[Quantity] decimal(19, 8) NOT NULL,
	[OfferNumber] nvarchar(20) NOT NULL,
    [CreateDate] datetime NOT NULL DEFAULT getdate(),
PRIMARY KEY NONCLUSTERED 
( [Id] ASC) WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) 
) ON [PRIMARY] 
GO
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[OneListItemDiscount]') AND name = N'IX_Id_OneListItemDiscount')
CREATE NONCLUSTERED INDEX [IX_Id_OneListItemDiscount] ON [dbo].[OneListItemDiscount] 
(
    [OneListId],[OneListItemId] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
GO

----------------------------
 --  OneListSubLine
----------------------------
IF EXISTS(SELECT * FROM sysobjects WHERE name = N'OneListSubLine' and xtype='U')
    DROP TABLE [dbo].[OneListSubLine]
GO
CREATE TABLE [dbo].[OneListSubLine](
    [Id] nvarchar(50) NOT NULL,
    [OneListId] nvarchar(50) NOT NULL, 
    [OneListItemId] nvarchar(50) NOT NULL,
	[LineNumber] int NULL,
	[Type] int NOT NULL,
    [ItemId] nvarchar(20) NOT NULL,
	[ItemDescription] nvarchar(50) DEFAULT(''),
    [UomId] nvarchar(10) DEFAULT(''),
    [VariantId] nvarchar(20) DEFAULT(''),
	[VariantDescription] nvarchar(50) DEFAULT(''),
    [Quantity] decimal(19, 8) NOT NULL,
	[DealLineId] int NULL,
	[DealModLineId] int NULL,
    [ModifierGroupCode] nvarchar(20) DEFAULT(''),
    [ModifierSubCode] nvarchar(20) DEFAULT(''),
	[ParentSubLineId] int NULL,
PRIMARY KEY NONCLUSTERED 
( [Id] ASC) WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) 
) ON [PRIMARY] 
GO
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[OneListSubLine]') AND name = N'IX_OneListId_OneListSubLine')
CREATE NONCLUSTERED INDEX [IX_OneListId_OneListSubLine] ON [dbo].[OneListSubLine] 
(
    [OneListId] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
GO  

----------------------------
 -- OneListLink
----------------------------
IF EXISTS(SELECT * FROM sysobjects WHERE name = N'OneListLink' and xtype='U')
    DROP TABLE [dbo].OneListLink
GO
CREATE TABLE [dbo].OneListLink(
	[CardId] nvarchar(50) NOT NULL DEFAULT(''),
    [OneListId] nvarchar(50) NOT NULL,
    [Name] nvarchar(50) NOT NULL,
	[Status] int NOT NULL DEFAULT(0),
	[Owner] tinyint NOT NULL DEFAULT(0),
PRIMARY KEY NONCLUSTERED 
( [CardId],[OneListId] ASC) WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON)  
) ON [PRIMARY] 
GO

----------------------------
 -- ResetPassword
---------------------------- 
IF  EXISTS (SELECT * FROM sysobjects WHERE name = N'ResetPassword' and xtype='U')
DROP TABLE [dbo].[ResetPassword]
GO
CREATE TABLE [dbo].[ResetPassword](
    [ResetCode] nvarchar(200) NOT NULL,
    [ContactId] nvarchar(20) NOT NULL,
    [Email] nvarchar(200) NOT NULL,
    [Enabled] bit NOT NULL,
    [Created] datetime NULL ,
CONSTRAINT [PK_ResetPassword] PRIMARY KEY NONCLUSTERED  
( [ResetCode] ASC) WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON)  
) ON [PRIMARY] 
GO

----------------------------
 -- LSKeys
----------------------------
IF EXISTS(SELECT * FROM sysobjects WHERE name = N'LSKeys' and xtype='U')
    DROP TABLE [dbo].[LSKeys]
GO
CREATE TABLE [dbo].[LSKeys](
	[LSKey] [nvarchar](50) NOT NULL,
	[Description] [nvarchar](50) NULL,
	[Active] [bit] NOT NULL,
 CONSTRAINT [PK_LSKeys] PRIMARY KEY CLUSTERED 
(
	[LSKey] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO

ALTER TABLE [dbo].[LSKeys] ADD CONSTRAINT [DF_LSKeys_Active] DEFAULT ((1)) FOR [Active]
GO
 
----------------------------
 -- TenantConfig
----------------------------
IF EXISTS(SELECT * FROM sysobjects WHERE name = N'TenantConfig' and xtype='U')
    DROP TABLE [dbo].[TenantConfig]
GO
CREATE TABLE [dbo].[TenantConfig](
	[LSKey] [nvarchar](50) NOT NULL,
	[Key] [nvarchar](50) NOT NULL,
	[Value] [nvarchar](2048) NOT NULL,
    [DataType] nvarchar(10) NOT NULL,
	[Comment] nvarchar(250) NULL,
	[Advanced] tinyint NULL
 CONSTRAINT [PK_Tenants] PRIMARY KEY CLUSTERED 
(
	[LSKey],[Key] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO

----------------------------
 -- Users
----------------------------
IF EXISTS(SELECT * FROM sysobjects WHERE name = N'Users' and xtype='U')
    DROP TABLE [dbo].[Users]
CREATE TABLE [dbo].[Users](
	[Username] [nvarchar](50) NOT NULL,
	[Password] [nvarchar](200) NOT NULL,
	[Admin] [bit] NOT NULL,
	[Active] [bit] NOT NULL,
 CONSTRAINT [PK_Users] PRIMARY KEY CLUSTERED 
(
	[Username] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
ALTER TABLE [dbo].[Users] ADD CONSTRAINT [DF_Users_Admin] DEFAULT ((0)) FOR [Admin]
GO
ALTER TABLE [dbo].[Users] ADD CONSTRAINT [DF_Users_Active] DEFAULT ((1)) FOR [Active]
GO

----------------------------
 -- UserKeys
----------------------------
IF EXISTS(SELECT * FROM sysobjects WHERE name = N'UserKeys' and xtype='U')
    DROP TABLE [dbo].[UserKeys]
CREATE TABLE [dbo].[UserKeys](
	[Username] [nvarchar](50) NOT NULL,
	[LSKey] [nvarchar](200) NOT NULL,
 CONSTRAINT [PK_UserKeys] PRIMARY KEY CLUSTERED 
(
	[Username] ASC,
	[LSKey] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO

----------------------------
 --  Images
----------------------------
IF EXISTS(SELECT * FROM sysobjects WHERE name = N'Images' and xtype='U')
    DROP TABLE [dbo].[Images]
GO
CREATE TABLE [dbo].[Images](
    [Id] nvarchar(50) NOT NULL,
    [Image] [image] NULL,
    [Type] int NOT NULL, --file=0, blob=1, url=2
    [Location] nvarchar(300) NOT NULL,
    [LastDateModified] datetime NOT NULL default getdate(),
PRIMARY KEY NONCLUSTERED 
( [Id]  ASC) WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) 
) ON [PRIMARY] 
GO

----------------------------
 --  ImageLink
----------------------------
IF EXISTS(SELECT * FROM sysobjects WHERE name = N'ImageLink' and xtype='U')
    DROP TABLE [dbo].[ImageLink]
GO
CREATE TABLE [dbo].[ImageLink](
    [TableName] nvarchar(50) NOT NULL,
    [RecordId] nvarchar(250) NOT NULL,
    [KeyValue] nvarchar(250) NOT NULL,
    [ImageId] nvarchar(50) NOT NULL,
    [DisplayOrder] int NOT NULL,
    [CreatedDate] datetime NULL default getdate(),
PRIMARY KEY NONCLUSTERED 
( [TableName],[KeyValue],[ImageId]  ASC) 
WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) 
) ON [PRIMARY] 
GO
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[ImageLink]') AND name = N'IX_ImageId_ImageLink')
CREATE NONCLUSTERED INDEX [IX_ImageId_MemberTrigger] ON [dbo].[ImageLink] 
(
    [ImageId] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
GO

----------------------------
 -- ImagesCache.
----------------------------
IF EXISTS(SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[ImagesCache]') and type='U')
    DROP TABLE [dbo].[ImagesCache]
GO
-- [image] has null, easer to insert in sql mgt studio
CREATE TABLE [dbo].[ImagesCache](
	[LSKey] [nvarchar](50) NOT NULL,
    [Id] nvarchar(50) NOT NULL ,
    [Width] int NOT NULL,
    [Height] int NOT NULL,
    [MinSize] tinyint NOT NULL,
    [Base64] [varchar](MAX) NOT NULL,
    [AvgColor] nvarchar(10) NOT NULL,
    [Format] nvarchar(10) NULL,
    [URL] nvarchar(500) NOT NULL,
    [LastModifiedDate] datetime NOT NULL DEFAULT (getdate()),
    [RV] rowversion NOT NULL,
 CONSTRAINT [PK_ImagesCache] PRIMARY KEY NONCLUSTERED 
(
	[LSKey] ASC,
    [Id] ASC,
    [Width] ASC,
    [Height] ASC,
	[MinSize] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] 
GO

----------------------------
 -- PushNotification
---------------------------- 
IF EXISTS(SELECT * FROM sysobjects WHERE name = N'PushNotification' and xtype='U')
    DROP TABLE [dbo].[PushNotification]
GO
CREATE TABLE [dbo].[PushNotification](
	[NotificationId] nvarchar(50) NOT NULL,
	[ContactId] nvarchar(200) NOT NULL,
	[DateCreated] datetime NOT NULL DEFAULT getdate(),
	[LastModified] datetime NOT NULL DEFAULT getdate(),
	[DateSent] datetime NULL DEFAULT NULL,
	[RetryCounter] int NOT NULL DEFAULT (0),
 CONSTRAINT [PK_PushNotification_1] PRIMARY KEY CLUSTERED 
(
	[NotificationId] ASC,
	[ContactId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO 
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[PushNotification]') AND name = N'IX_PushNotification_Id')
CREATE NONCLUSTERED   INDEX [IX_PushNotification_Id] ON [dbo].[PushNotification] 
(
    [NotificationId],[ContactId] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
GO  

----------------------------
 -- SpgNotification
---------------------------- 
IF EXISTS(SELECT * FROM sysobjects WHERE name = N'SpgNotification' and xtype='U')
    DROP TABLE [dbo].[SpgNotification]
GO
CREATE TABLE [dbo].[SpgNotification](
	[CardId] [nvarchar](50) NOT NULL,
	[Token] [nvarchar](150) NOT NULL,
 CONSTRAINT [PK_SpgNotification] PRIMARY KEY CLUSTERED 
(
	[CardId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO

----------------------------
 -- SpgNotificationLog
---------------------------- 
IF EXISTS(SELECT * FROM sysobjects WHERE name = N'SpgNotificationLog' and xtype='U')
    DROP TABLE [dbo].[SpgNotificationLog]
GO
CREATE TABLE [dbo].[SpgNotificationLog](
	[Id] [bigint] IDENTITY(1,1) NOT NULL,
	[CardId] [nvarchar](50) NOT NULL,
	[Message] [nvarchar](500) NULL,
	[SentAt] [datetime] NULL,
	[Success] [tinyint] NULL,
	[ErrorMsg] [nvarchar](200) NULL,
 CONSTRAINT [PK_SpgNotificationLog] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO

----------------------------
 -- DeviceSecurity
---------------------------- 
IF EXISTS(SELECT * FROM sysobjects WHERE name = N'DeviceSecurity' and xtype='U')
    DROP TABLE [dbo].[DeviceSecurity]
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[DeviceSecurity]') AND type in (N'U'))
BEGIN
CREATE TABLE [dbo].[DeviceSecurity](
    [SecurityToken] nvarchar(50) NOT NULL,
    [DeviceId] nvarchar(50) NOT NULL,
    [FcmToken] nvarchar(200) NULL,
    [ContactId] nvarchar(20) NOT NULL,
    [Created] datetime NOT NULL DEFAULT (getdate()),
 CONSTRAINT [PK_DeviceSecurity] PRIMARY KEY CLUSTERED 
(
	[DeviceId],[ContactId] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
END
GO
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[DeviceSecurity]') AND name = N'IX_DeviceId_DeviceSecurity')
CREATE NONCLUSTERED INDEX [IX_DeviceId_DeviceSecurity] ON [dbo].[DeviceSecurity] 
(
    [DeviceId],[ContactId] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
GO

----------------------------
 -- RodDeviceUnlock
---------------------------- 
IF EXISTS(SELECT * FROM sysobjects WHERE name = N'RodDeviceUnlock' and xtype='U')
    DROP TABLE [dbo].[RodDeviceUnlock]
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[RodDeviceUnlock]') AND type in (N'U'))
BEGIN
CREATE TABLE [dbo].[RodDeviceUnlock](
	[StoreId] [nvarchar](50) NOT NULL,
	[CardId] [nvarchar](50) NOT NULL,
	[Regtime] [datetime] NULL,
 CONSTRAINT [PK_RodDeviceUnlock] PRIMARY KEY CLUSTERED 
(
	[StoreId] ASC,
	[CardId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
END
GO

----------------------------
 -- Notification
---------------------------- 
IF EXISTS(SELECT * FROM sysobjects WHERE name = N'Notification' and xtype='U')
    DROP TABLE [dbo].[Notification]
GO
CREATE TABLE [dbo].[Notification](
    [Id] nvarchar(50) NOT NULL,
    [Type] int NOT NULL,     -- 0=Account,1=Contact,2=Club,3=Scheme    
    [TypeCode] nvarchar(20) NOT NULL,
    [PrimaryText] nvarchar(500) NOT NULL,
    [SecondaryText] nvarchar(2000) NULL,
    [DisplayFrequency] int NULL,    --Always,Once
    [ValidFrom] datetime NULL,
    [ValidTo] datetime NULL,
    [Created] datetime NULL,
    [CreatedBy] nvarchar(50) NULL,
    [LastModifiedDate] datetime NULL DEFAULT (getdate()), --date replicated to table
    [DateLastModified] datetime NULL DEFAULT (getdate()), --date inserted to table
    [QRText] nvarchar(1000)  NULL DEFAULT (''),  -- JIJ v2.0 added this new col
    [NotificationType] int  NULL DEFAULT (0),   --0 Central  1 Commerce
    [Status] int NULL DEFAULT (1),  --0 disabled, 1=Enabled
 CONSTRAINT [PK_Notification] PRIMARY KEY NONCLUSTERED 
(
    [Id] ASC,
	[TypeCode] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON)  
) ON [PRIMARY] 
GO
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[Notification]') AND name = N'IX_Notification_TypeCode')
CREATE NONCLUSTERED   INDEX [IX_Notification_TypeCode] ON [dbo].[Notification] 
(
    [TypeCode] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
GO 
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[Notification]') AND name = N'IX_Notification_LastModifiedDate')
CREATE NONCLUSTERED   INDEX [IX_Notification_LastModifiedDate] ON [dbo].[Notification] 
(
    [LastModifiedDate] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
GO 

----------------------------
 -- Task Tables for Commerce Tasks
---------------------------- 
IF  EXISTS (SELECT * FROM sysobjects WHERE name = N'Task' and xtype='U')
DROP TABLE [dbo].[Task] 
GO
CREATE TABLE [dbo].[Task](
	[Id] nvarchar(40) NOT NULL,
	[Status] nvarchar(20) NULL,
	[Type] nvarchar(20) NULL,
	[TransactionId] nvarchar(30) NULL,
	[CreateTime] datetime NOT NULL,
	[ModifyTime] datetime NOT NULL,
	[ModifyUser] nvarchar(30) NULL,
	[ModifyLocation] nvarchar(30) NULL,
	[StoreId] nvarchar(30) NULL,
	[RequestUser] nvarchar(30) NULL,
	[RequestUserName] nvarchar(30) NULL,
	[RequestLocation] nvarchar(30) NULL,
	[AssignUser] nvarchar(30) NULL,
	[AssignUserName] nvarchar(30) NULL,
	[AssignLocation] nvarchar(30) NULL,
 CONSTRAINT [PK_Task] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
 
IF  EXISTS (SELECT * FROM sysobjects WHERE name = N'TaskLine' and xtype='U')
DROP TABLE [dbo].[TaskLine] 
GO
CREATE TABLE [dbo].[TaskLine](
	[Id] nvarchar(40) NOT NULL,
	[TaskId] nvarchar(40) NOT NULL,
	[LineNumber] int NOT NULL,
	[Status] nvarchar(20) NULL,
	[ModifyTime] datetime NOT NULL,
	[ModifyUser] nvarchar(30) NULL,
	[ModifyLocation] nvarchar(30) NULL,
	[ItemId] nvarchar(30) NULL,
	[ItemName] nvarchar(50) NULL,
	[VariantId] nvarchar(30) NULL,
	[VariantName] nvarchar(150) NULL,
	[Quantity] int NOT NULL,
 CONSTRAINT [PK_TaskLine] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO

IF  EXISTS (SELECT * FROM sysobjects WHERE name = N'TaskLog' and xtype='U')
DROP TABLE [dbo].[TaskLog] 
GO
CREATE TABLE [dbo].[TaskLog](
	[Id] [bigint] IDENTITY(1,1) NOT NULL,
	[TaskId] nvarchar(40) NOT NULL,
	[ModifyTime] datetime NOT NULL,
	[ModifyUser] nvarchar(30) NULL,
	[ModifyLocation] nvarchar(30) NULL,
	[StatusFrom] nvarchar(20) NULL,
	[StatusTo] nvarchar(20) NULL,
	[RequestUserFrom] nvarchar(30) NULL,
	[RequestUserTo] nvarchar(30) NULL,
	[AssignUserFrom] nvarchar(30) NULL,
	[AssignUserTo] nvarchar(30) NULL,
 CONSTRAINT [PK_TaskLog] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO

IF  EXISTS (SELECT * FROM sysobjects WHERE name = N'TaskLogLine' and xtype='U')
DROP TABLE [dbo].[TaskLogLine] 
GO
CREATE TABLE [dbo].[TaskLogLine](
	[Id] [bigint] IDENTITY(1,1) NOT NULL,
	[TaskLineId] nvarchar(40) NOT NULL,
	[ModifyTime] datetime NOT NULL,
	[ModifyUser] nvarchar(30) NULL,
	[ModifyLocation] nvarchar(30) NULL,
	[StatusFrom] nvarchar(20) NULL,
	[StatusTo] nvarchar(20) NULL,
 CONSTRAINT [PK_TaskLogLine] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO

IF  EXISTS (SELECT * FROM sysobjects WHERE name = N'UserLogin' and xtype='U')
DROP TABLE [dbo].[UserLogin] 
GO
CREATE TABLE [dbo].[UserLogin](
	[Username] [nvarchar](50) NOT NULL,
	[Token] [nvarchar](150) NOT NULL,
	[DateModified] [datetime] NOT NULL
 CONSTRAINT [PK_UserLogin] PRIMARY KEY CLUSTERED 
(
	[Username], [Token] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO

----------------------------------------------------------------------------------------------------------------
-- TRIGGERS ------------------------------
---------------------------------------------------------------------------------------------------------------

IF  EXISTS (SELECT * FROM sys.triggers WHERE object_id = OBJECT_ID(N'[dbo].[TenantConfigTrg]'))
DROP TRIGGER [dbo].[TenantConfigTrg]
GO
CREATE TRIGGER [dbo].[TenantConfigTrg]
   ON [dbo].[TenantConfig] AFTER INSERT, UPDATE
 AS
    SET NOCOUNT ON;
    BEGIN
    if exists (select * from inserted where [DataType] not in ('string','int','bool','decimal') )
    begin
        RAISERROR('TenantConfig.DataType must either String, Int, Bool or Decimal',16,1)
        ROLLBACK TRAN
    end

    --not perfect!  breaks in a batch but better than nothing at all 
    DECLARE @value nvarchar(4000)
    DECLARE @datatype nvarchar(10)
    select @value = [Value], @datatype = [DataType] FROM inserted
    if (@datatype = 'int' and ISNUMERIC(@value) != 1)
        begin
        RAISERROR('TenantConfig.Value must be an integer',16,1)
        ROLLBACK TRAN
    end
    if (@datatype = 'decimal' and ISNUMERIC(@value) != 1)
        begin
        RAISERROR('TenantConfig.Value must be a decimal',16,1)
        ROLLBACK TRAN
    end
    if (@datatype = 'bool' and not(@value = 'true' or @value = 'false' ))
        begin
        RAISERROR('TenantConfig.Value must be true or false',16,1)
        ROLLBACK TRAN
    end

   END
GO 