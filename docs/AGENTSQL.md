# A Stored Procedure to Monitor Agent Upgrade Status Over-Time

---
Original Connect Article: [A Stored Procedure to Monitor Agent Upgrade Status Over-Time](https://www.symantec.com/connect/articles/stored-procedure-monitor-agent-upgrade-status-over-time)

By: [Ludovic Ferre](https://www.symantec.com/connect/user/ludovic-ferre)

---

## Table of content

- [Introduction](Introduction)
- [Design](Design)
- [SQL code](SQL-code)
- [Usage](Usage)
- [Conclusion](Conclusion)

## Introduction
Monitoring agent upgrade progress over time is an important task in large environments, and is beneficial in any environment to understand the managed computer pool behaviour and effects of seasonality and human behaviour on tasks execution, agent upgrades or patch compliance.

In this article we will create a stored procedure that will allow us to automatically find out which is agent versions are the highest and record the count of computers with the agent and the count of computers to update.

Design
The data will be collected in a custom table name '`TREND_AgentVersions`'. If the table does not exist it will be automatically created when running the procedure.

The data collection should not happen many times a day, so to avoid this we verify if the last recorded data set was taken within the last 23 hour. If so we will return the last dataset to the caller. If yes we collect fresh data and return the fresh data to the caller.

The data gathered itself is based on the Basic Inventory dataclass 'AeX AC Client Agent' .

We currently track the following agent versions:

- Symantec Altiris Agent (core)
- Altiris Inventory Solution agent
- Altiris Software Update Agent (Patch Management agent)
- Altiris Software Management Solution Agent

Other agents could be added, such as the Symantec Workspace Virtualization, but this could be done easily by amending the select code in the procedure.

The table will contain the following columns:

- _exec_id
- _exec_time
- Agent Name
- Agent Highest Version
- Agents Installed
- Agents to upgrade
- % up-to-date

The last field is the result of a computation that could be done at run time (when we select data from the table) but I have decided to store the data so that the information is readily usable for SMP reports and other consumption by users.

SQL Code
Here is the full procedure code:

- [spTrendAgentVersions.sql](..\spTrendAgentVersions.sql)

```sql
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
```

## Usage
Copy the SQL procedure code above or save the attached file to run it against your Symantec_CMDB database.

Once the procedure is created on the server you can call it from a SQL task on the SMP, with the following command:

`exec spTrendAgentVersions`

Save the task and schedule it to run daily. during the night (anytime between 2100 and 0500. Personally I like to schedule it before 23:59 as this ensure the _exec_date field matches the day when the results where collected. If you run the task past midnight the data will be shown for day <d> but the execution time (and date label in any UI) would show <d +1> which can be confusing.

## Conclusion

With a daily schedule you can now track the agent upgrade status of your computers over time. But in order to show the data in a visualize appealing manner you will need a custom User Interface. But this will be the subject of another article or download!