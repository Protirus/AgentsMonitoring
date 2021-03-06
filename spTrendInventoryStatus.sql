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
