GO

SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE procedure [dbo].[spTrendAgentVersions]
  @force as int = 0
as
/* 
      STORED AGENT COUNTS
*/
-- PART I: Make sure underlying infrastructure exists and is ready to use
if (not exists(select 1 from sys.objects where type = 'U' and name = 'TREND_AgentVersions'))
begin
  CREATE TABLE [dbo].[TREND_AgentVersions](
    [_Exec_id] [int] NOT NULL,
    [_Exec_time] [datetime] NOT NULL,
    [Agent Name] varchar(255) NOT NULL,
    [Agent Highest Version] varchar(255) not null,
    [Agents Installed] varchar(255) NOT NULL,
    [Agents to upgrade] varchar(255) NOT NULL,
    [% up-to-date] money
  ) ON [PRIMARY]

  CREATE UNIQUE CLUSTERED INDEX [IX_TREND_AgentVersions] ON [dbo].[TREND_AgentVersions] 
  (
    [_exec_id] ASC,
    [Agent Name]
  )WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = 
OFF, ONLINE = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]

end

-- PART II: Get data into the trending table if no data was captured in the last 23 hours
if ((select MAX(_exec_time) from TREND_AgentVersions) <  dateadd(hour, -23, getdate()) or (select COUNT(*) from TREND_AgentVersions) = 0) or (@force = 1)
begin

  declare @id as int
    set @id = (select MAX(_exec_id) from TREND_AgentVersions)

  insert into TREND_AgentVersions
  select (ISNULL(@id + 1, 1)), GETDATE() as '_Exec_time', _cur.[Agent Name], _cur.Latest as 'Agent highest version', _cur.[Agent #] as 'Agents installed', isnull(_old.[Agent #], 0) as 'Agents to upgrade',
       CAST(CAST(_cur.[Agent #] - isnull(_old.[Agent #], 0) as float) / CAST(_cur.[agent #] as float) * 100 as money) as '% up-to-date'
    from 
      (
        select [Agent name], COUNT(*) as 'Agent #', max(a.[Product Version]) as 'Latest'
          from Inv_AeX_AC_Client_Agent a
         where [Agent Name] in ('Altiris Agent'
                    , 'Altiris Inventory Agent'
                    , 'Altiris Software Update Agent'
                    , 'Software Management Solution Agent'
                    ,'Symantec Workspace Virtualization Agent'
                    )
         group by [agent name]
      ) _cur
    left join (
      select a1.[Agent name], COUNT(*) as 'Agent #'
        from Inv_AeX_AC_Client_Agent a1
        join (
            select [Agent name], max(a.[Product Version]) as 'Latest'
              from Inv_AeX_AC_Client_Agent a
             where [Agent Name] in (  'Altiris Agent'
                        , 'Altiris Inventory Agent'
                        , 'Altiris Software Update Agent'
                        , 'Software Management Solution Agent'
                        , 'Symantec Workspace Virtualization Agent'
                        )
             group by [agent name]
          ) a2
        on a1.[Agent Name] = a2.[Agent Name]
       where a1.[Product Version] < a2.Latest
       group by a1.[Agent Name]
      ) _old
    on _cur.[Agent Name] = _old.[Agent Name]
   order by [Agent Name]
   
end
select *
  from TREND_AgentVersions
 where _exec_id = (select MAX(_exec_id) from TREND_AgentVersions)
 order by [Agent Name]
 
GO
