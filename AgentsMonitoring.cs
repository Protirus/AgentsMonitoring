using Altiris.NS.ContextManagement;
using System;
using Symantec.CWoC.APIWrappers;
using System.Data;
using System.Data.Sql;
using System.Text;

namespace Symantec.CWoC {
    class AgentsMonitoring {
        static void Main(string[] args) {
            // This tool generates the javascript as seen in sample Web-UI/agent_trending_data.js
            // This requires quite a few SQL queries, so I'll probably start with this.
        }
    }

    // All SQL request come thru this class
    class DataHandler {
        // Agent data handlers
        public DataTable GetAgent_List() {
            string sql = "select distinct([agent name]) from TREND_AgentVersions";
            return DatabaseAPI.GetTable(sql);
        }
        public DataTable GetAgent_Gauge_Stats(string stat_type) {
            string sql = "select [Agent Name], [Agent Highest Version], _exec_id, [% up-to-date] from TREND_AgentVersions where _Exec_id = (select MAX(_exec_id) from TREND_AgentVersions) order by [agent name]";
            return DatabaseAPI.GetTable(sql);
        }
        public DataTable GetAgent_CandlestickALL_Stats(string stat_type) {
            string sql = @"
select a1.[Agent Name], a1.Lowest /* LOW */, a3.[Agents Installed] as 'Previous' /* OPENING */, a2.[Agents Installed] as 'Current' /* CLOSING */, a1.Highest /* MAX */
  from (
select a1.[Agent Name], MIN(a1.[Agents installed]) as 'Lowest', MAX(a1.[agents installed]) as 'Highest'
  from TREND_AgentVersions a1
 group by [agent name], [Agent Highest Version]
		) a1
  join TREND_AgentVersions a2
    on a1.[Agent Name] = a2.[Agent Name]
  join TREND_AgentVersions a3
    on a2.[Agent Name] = a3.[Agent Name]
 where a2._Exec_id = (select MAX(_exec_id) from TREND_AgentVersions)
   and a3._Exec_id = (select MAX(_exec_id) - 1 from TREND_AgentVersions)
";
            return DatabaseAPI.GetTable(sql);
        }
        public DataTable GetAgent_CandlestickOutdated_Stats() {
            string sql = @"
select a1.[Agent Name], a1.Lowest /* LOW */, a3.[Agents to upgrade] as 'Previous' /* OPENING */, a2.[Agents to upgrade] as 'Current' /* CLOSING */, a1.Highest /* MAX */
  from (
select a1.[Agent Name], MIN(a1.[Agents to upgrade]) as 'Lowest', MAX(a1.[agents to upgrade]) as 'Highest'
  from TREND_AgentVersions a1
 group by [agent name], [Agent Highest Version]
		) a1
  join TREND_AgentVersions a2
    on a1.[Agent Name] = a2.[Agent Name]
  join TREND_AgentVersions a3
    on a2.[Agent Name] = a3.[Agent Name]
 where a2._Exec_id = (select MAX(_exec_id) from TREND_AgentVersions)
   and a3._Exec_id = (select MAX(_exec_id) - 1 from TREND_AgentVersions)
";
            return DatabaseAPI.GetTable(sql);
        }
        public DataTable GetAgent_Line_Stats(string agent_name) {
            string sql = @"select cast(cast(_Exec_time as date) as varchar(255)) as _exec_time, [Agents Installed], cast([Agents Installed] as int) - cast([Agents to upgrade] as int) as 'Agents up-to-date', [Agents to upgrade] from TREND_AgentVersions where [agent name] = '{0}'
";
            sql = string.Format(sql, agent_name);
            return DatabaseAPI.GetTable(sql);
        }
        // Inventory data handlers
        public DataTable GetInventory_List() {
            string sql = "select distinct([inventory type]) from TREND_InventoryStatus";
            return DatabaseAPI.GetTable(sql);
        }
        public DataTable GetInventory_Gauge_Stats() {
            string sql = "select[inventory type], [% up-to-date] from TREND_InventoryStatus where _Exec_id = (select MAX(_exec_id) from TREND_InventoryStatus) order by [inventory type]";
            return DatabaseAPI.GetTable(sql);
        }
        public DataTable GetInventory_CandleALL_Stats() {
            string sql = @"
select a1.[inventory type], a1.Lowest /* LOW */, a3.[Computer #] as 'Previous' /* OPENING */, a2.[Computer #] as 'Current' /* CLOSING */, a1.Highest /* MAX */
  from (
select a1.[inventory type], MIN(a1.[Computer #]) as 'Lowest', MAX(a1.[Computer #]) as 'Highest'
  from TREND_InventoryStatus a1
 group by [inventory type]
		) a1
  join TREND_InventoryStatus a2
    on a1.[inventory type] = a2.[inventory type]
  join TREND_InventoryStatus a3
    on a2.[inventory type] = a3.[inventory type]
 where a2._Exec_id = (select MAX(_exec_id) from TREND_InventoryStatus)
   and a3._Exec_id = (select MAX(_exec_id) - 1 from TREND_InventoryStatus)
";
            return DatabaseAPI.GetTable(sql);
        }
        public DataTable GetInventory_CandleOutdated_Stats() { 
            string sql = @"
select a1.[inventory type], a1.Lowest /* LOW */, a3.[Not updated in last 4 weeks] as 'Previous' /* OPENING */, a2.[Not updated in last 4 weeks] as 'Current' /* CLOSING */, a1.Highest /* MAX */
  from (
select a1.[inventory type], MIN(a1.[Not updated in last 4 weeks]) as 'Lowest', MAX(a1.[Not updated in last 4 weeks]) as 'Highest'
  from TREND_InventoryStatus a1
 group by [inventory type]
		) a1
  join TREND_InventoryStatus a2
    on a1.[inventory type] = a2.[inventory type]
  join TREND_InventoryStatus a3
    on a2.[inventory type] = a3.[inventory type]
 where a2._Exec_id = (select MAX(_exec_id) from TREND_InventoryStatus)
   and a3._Exec_id = (select MAX(_exec_id) - 1 from TREND_InventoryStatus)";
            return DatabaseAPI.GetTable(sql);
        }
        public DataTable GetInventory_Line_Stats(string agent_name) {
            string sql = "select cast(cast(_Exec_time as date) as varchar(255)) as 'Date', [Computer #], [Updated in last 4 weeks], [Not updated in last 4 weeks]  from TREND_InventoryStatus where [inventory type] = '{0}'";
            sql = string.Format(sql, agent_name);
            return DatabaseAPI.GetTable(sql);
        }
    }
    // Output data is generated thru this class
    class JSformatter {
        public string GetJSONFromTable(DataTable t, string entry, string head, int column) {
            StringBuilder b = new StringBuilder();

            b.AppendLine("var " + entry + " = [");
            b.AppendFormat("\t{0},", head);

            foreach (DataRow r in t.Rows) {
                b.Append("\t[");
                for (int i = 0; i < column; i++) {
                    b.AppendFormat("'{0}', ", r[i].ToString().Replace(',', '.'));
                }
                b.Append("],\n");
            }
            // Remove the last comma we inserted
            b.Remove(b.Length - 3, 1);
            b.AppendLine("];");

            return b.ToString();
        }


    }
}
