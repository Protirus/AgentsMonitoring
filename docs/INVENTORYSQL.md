# A Stored Procedure to Monitor Inventory Status Over-Time

---
Original Connect Article: [A Stored Procedure to Monitor Inventory Status Over-Time](https://www.symantec.com/connect/articles/stored-procedure-monitor-inventory-status-over-time)

By: [Ludovic Ferre](https://www.symantec.com/connect/user/ludovic-ferre)

---

## Table of content

- [Introduction](Introduction)
- [Design](Design)
- [SQL code](SQL-code)
- [Usage](Usage)
- [Conclusion](Conclusion)

## Introduction
Monitoring inventory updates (refresh rates) over time is an important task in large environments, and is beneficial in any environment. In this article we will create a stored procedure that will allow us to automatically track how many agents have sent an inventory (per inventory type) in the past few weeks to complement the built-in inventory status reports.

## Design
The data will be collected in a custom table name '`TREND_InventoryUpdates`'. If the table does not exist it will be automatically created when running the procedure.

The data collection should not happen many times a day, so to avoid this we verify if the last recorded data set was taken within the last 23 hour. If so we will return the last dataset to the caller. If yes we collect fresh data and return the fresh data to the caller.

The data gathered itself is based on the ResourceUpdateSummary table, for the following inventory types:

- Basic Inventory (from the core agent)
- Hardware Inventory
- Operating System Inventory
- Software Inventory
- User Group Inventory

If you have custom inventory classes that have a standard name (for example 'MyCompany - ...) you could also add those as a specific type to the procedure.

The custom table storing the tracking data will contain the following columns:

- _exec_id
- _exec_time
- Inventory Type
- Computers
- Updated in the last 4 week
- Not updated in the last 4 weeks
- % up-to-date

The last two fields are the result of a computation that could be done at run time (when we select data from the table) but I have decided to store the data so that the information is readily usable for SMP reports and other consumption by users.

***Important note!*** I have chosen 4 weeks (28 days) as a threshold here. This is a good starting point, and you could change this however their is not plans to support such customisation in the upcoming custom UI to display the gathered data.

## SQL Code
Here is the full procedure code, name spTrendInventoryStatus:

```sql
GO

SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE procedure [dbo].[spTrendInventoryStatus]
  @force as int = 0
as

-- PART I: Make sure underlying infrastructure exists and is ready to use
if (not exists(select 1 from sys.objects where type = 'U' and name = 'TREND_InventoryStatus'))
begin
  CREATE TABLE [dbo].[TREND_InventoryStatus](
    [_Exec_id] [int] NOT NULL,
    [_Exec_time] [datetime] NOT NULL,
    [Inventory Type] varchar(255) NOT NULL,
    [Computer #] int not null,
    [Updated in last 4 weeks] int NOT NULL,
    [Not Updated in last 4 weeks] int NOT NULL,
    [% up-to-date] money
  ) ON [PRIMARY]

  CREATE UNIQUE CLUSTERED INDEX [IX_TREND_InventoryStatus] ON [dbo].[TREND_InventoryStatus] 
  (
    [_exec_id] ASC,
    [Inventory Type]
  )WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = 
OFF, ONLINE = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]

end

-- PART II: Get data into the trending table if no data was captured in the last 23 hours
if ((select MAX(_exec_time) from TREND_InventoryStatus) <  dateadd(hour, -23, getdate()) or (select COUNT(*) from TREND_InventoryStatus) = 0) or (@force = 1)
BEGIN

  declare @id as int
    set @id = (select MAX(_exec_id) from TREND_InventoryStatus)

  declare @basinv int, @basinv_utd int, @os int, @os_utd int, @hw int, @hw_utd int, @sw int, @sw_utd int, @ug int, @ug_utd int

  select @basinv = COUNT(distinct(ResourceGuid))
    from ResourceUpdateSummary rus
    join DataClass dc on rus.InventoryClassGuid = dc.guid
   where dc.Name like 'AeX AC%'
  select @basinv_utd = COUNT(distinct(ResourceGuid))
    from ResourceUpdateSummary rus
    join DataClass dc on rus.InventoryClassGuid = dc.guid
   where dc.Name like 'AeX AC%' and rus.ModifiedDate > GETDATE () - 28

  select @os = COUNT(distinct(ResourceGuid))
    from ResourceUpdateSummary rus
    join DataClass dc on rus.InventoryClassGuid = dc.guid
   where dc.Name like 'OS %'
  select @os_utd = COUNT(distinct(ResourceGuid))
    from ResourceUpdateSummary rus
    join DataClass dc on rus.InventoryClassGuid = dc.guid
   where dc.Name like 'OS %' and rus.ModifiedDate > GETDATE () - 28

  select @hw = COUNT(distinct(ResourceGuid))
    from ResourceUpdateSummary rus
    join DataClass dc on rus.InventoryClassGuid = dc.guid
   where dc.Name like 'HW %'
  select @hw_utd = COUNT(distinct(ResourceGuid))
    from ResourceUpdateSummary rus
    join DataClass dc on rus.InventoryClassGuid = dc.guid
   where dc.Name like 'HW %' and rus.ModifiedDate > GETDATE () - 28

  select @basinv = COUNT(distinct(ResourceGuid))
    from ResourceUpdateSummary rus
    join DataClass dc on rus.InventoryClassGuid = dc.guid
   where dc.Name like 'AeX AC%'
  select @basinv_utd = COUNT(distinct(ResourceGuid))
    from ResourceUpdateSummary rus
    join DataClass dc on rus.InventoryClassGuid = dc.guid
   where dc.Name like 'AeX AC%' and rus.ModifiedDate > GETDATE () - 28

  select @sw = COUNT(distinct(ResourceGuid))
    from ResourceUpdateSummary rus
    join DataClass dc on rus.InventoryClassGuid = dc.guid
   where dc.Name like 'SW %'
  select @sw_utd = COUNT(distinct(ResourceGuid))
    from ResourceUpdateSummary rus
    join DataClass dc on rus.InventoryClassGuid = dc.guid
   where dc.Name like 'SW %' and rus.ModifiedDate > GETDATE () - 28

  select @ug = COUNT(distinct(ResourceGuid))
    from ResourceUpdateSummary rus
    join DataClass dc on rus.InventoryClassGuid = dc.guid
   where dc.Name like 'HW %'
  select @ug_utd = COUNT(distinct(ResourceGuid))
    from ResourceUpdateSummary rus
    join DataClass dc on rus.InventoryClassGuid = dc.guid
   where dc.Name like 'HW %' and rus.ModifiedDate > GETDATE () - 28

  insert into TREND_InventoryStatus
  select (ISNULL(@id + 1, 1)), GETDATE() as '_Exec_time', 'Basic Inventory' as 'Inventory type', @basinv as 'Computers', @basinv_utd as 'Updated in last 4 weeks', @basinv - @basinv_utd as 'Not Updated in the last 4 weeks', cast(cast(@basinv_utd as float) /  cast(@basinv as float) * 100 as money) '% up-to-date'
   union
  select (ISNULL(@id + 1, 1)), GETDATE() as '_Exec_time', 'OS Inventory', @os, @os_utd, @os - @os_utd, cast(cast(@os_utd as float) /  cast(@os as float) * 100 as money)
   union
  select (ISNULL(@id + 1, 1)), GETDATE() as '_Exec_time', 'HW Inventory', @hw, @hw_utd, @hw - @hw_utd, cast(cast(@hw_utd as float) /  cast(@hw as float) * 100 as money)
   union
  select (ISNULL(@id + 1, 1)), GETDATE() as '_Exec_time', 'SW Inventory', @sw, @sw_utd, @sw - @sw_utd, cast(cast(@sw_utd as float) /  cast(@sw as float) * 100 as money)
   union
  select (ISNULL(@id + 1, 1)), GETDATE() as '_Exec_time', 'UG Inventory', @ug, @ug_utd, @ug - @ug_utd, cast(cast(@ug_utd as float) /  cast(@ug as float) * 100 as money)

END

select *
  from TREND_InventoryStatus
 where [_Exec_id] = (select MAX(_exec_id) from TREND_InventoryStatus)
 order by [Inventory type]

GO
```

## Usage
Copy the SQL procedure code above or save the attached file to run it against your Symantec_CMDB database.

Once the procedure is created on the server you can call it from a SQL task on the SMP, with the following command:

`exec spTrendInventoryStatus`

Save the task and schedule it to run daily. during the night (anytime between 2100 and 0500. Personally I like to schedul it before 23:59 as this ensure the _exec_date field matches the day when the results where collected. If you run the task past midnight the data will be shown for day <d> but the execution time (and date label in any UI) would show <d +1> which can be confusing.

## Conclusion
With a daily schedule you can now track the inventory status of your computers over time. But in order to show the data in a visualize appealing manner you will need a custom User Interface. But this will be the subject of another article or download!