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

            DataHandler h = new DataHandler();
            JSformatter f = new JSformatter();

            // Start with the agent options (dynamic)
            DataTable agent_list = h.GetAgent_List();
            foreach (DataRow r in agent_list.Rows) {
                string agent = (string)r[0];
                string version = h.GetAgentVersion(agent);
                string var_name = f.GetAgentTableName(agent).Replace("_table", "_options");
                Console.WriteLine("var {0} = {{title: '{1} ({2})'}};", var_name, agent, version);
            }
            Console.WriteLine();

            // Continue with the inventory options - all static for now.
            Console.WriteLine(f.InvOptions);

            Console.WriteLine("var candlestick_all_options = {legend:'none', title:'Computers sending Inventory'};");
            Console.WriteLine("var candlestick_out_options = {legend:'none', title: 'Inventory data older than 4 weeks'};");
            Console.WriteLine();

            // Generate the gauge tables
            string agent_gauge = f.GetJSONFromTable(h.GetAgent_Gauge_Stats(), "agent_gauge_table", f.gauge_thead);
            Console.WriteLine(f.ConvertGauges(agent_gauge));
            // Generate the candlestick tables

            string inv_gauge = f.GetJSONFromTable(h.GetInventory_Gauge_Stats(), "inv_gauge_table", f.gauge_thead);
            Console.WriteLine(f.ConvertGauges(inv_gauge));

            // Generate the tables used in the agent line charts
            foreach (DataRow r in agent_list.Rows) {
                Console.WriteLine(f.GetJSONFromTable(h.GetAgent_Line_Stats(r[0].ToString()), f.GetAgentTableName((string)r[0]), f.agent_thead));
            }

            // Generate the tables used in the inventory line charts
            foreach (DataRow r in h.GetInventory_List().Rows) {
                Console.WriteLine(f.GetJSONFromTable(h.GetInventory_Line_Stats((string) r[0]), f.GetInvTableName((string)r[0]), f.inv_thead));
            }
            Console.ReadLine();
        }
    }

    // All SQL request come thru this class
    class DataHandler {
        // Agent data handlers
        public DataTable GetAgent_List() {
            string sql = "select distinct([agent name]) from TREND_AgentVersions";
            return DatabaseAPI.GetTable(sql);
        }
        public string GetAgentVersion(string agent_name) {
            string sql = String.Format("select max([Agent Highest Version]) from TREND_AgentVersions where [Agent name] = '{0}'", agent_name);
            return (string) DatabaseAPI.GetTable(sql).Rows[0][0];

        }
        public DataTable GetAgent_Gauge_Stats() {
            string sql = "select [Agent Name], [% up-to-date] from TREND_AgentVersions where _Exec_id = (select MAX(_exec_id) from TREND_AgentVersions) order by [agent name]";
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
        public readonly string inv_thead = "['Date', 'Agent #', 'Updated', 'Outdated']";
        public readonly string agent_thead = "['Date', 'Agent #', 'OK', 'NOK']";
        public readonly string gauge_thead = "['Label', 'Value']";
        public string GetJSONFromTable(DataTable t, string entry, string head) {
            StringBuilder b = new StringBuilder();

            b.AppendLine("var " + entry + " = [");
            b.AppendFormat("\t{0},\n", head);

            foreach (DataRow r in t.Rows) {
                b.Append("\t[");
                for (int i = 0; i < r.ItemArray.Length; i++) {
                    if (i == 0) {
                        b.AppendFormat("'{0}', ", r[i].ToString().Replace(',', '.'));
                    } else {
                        b.AppendFormat("{0}, ", r[i].ToString().Replace(',', '.'));
                    }
                }
                b.Remove(b.Length - 2, 1);
                b.Append("],\n");
            }
            // Remove the last comma we inserted
            b.Remove(b.Length - 2, 1);
            b.AppendLine("];");

            return b.ToString();
        }
        public string GetAgentTableName(string agent_name) {
            switch (agent_name) {
                case "Altiris Agent":
                    return "core_table";
                case "Altiris Software Update Agent":
                    return "sua_table";
                case "Altiris Inventory Agent":
                    return "inv_table";
                case "Software Management Solution Agent":
                    return "swm_table";
            }
            return "unk";
        }
        public string GetInvTableName(string agent_name) {
            switch (agent_name.ToLower()) {
                case "basic inventory":
                    return "bi_table";
                case "hw inventory":
                    return "hw_table";
                case "os inventory":
                    return "os_table";
                case "sw inventory":
                    return "sw_table";
                case "ug inventory":
                    return "ug_table";
            }
            return "unk";
        }
        public readonly string InvOptions = @"
var bi_options = { title: 'Basic Inventory (Core)' };
var hw_options = { title: 'Hardware Inventory' };
var os_options = { title: 'OS Inventory' };
var sw_options = { title: 'Software Inventory' };
var ug_options = { title: 'User Group Inventory' };
";
        public string ConvertGauges(string gauge) {
            StringBuilder b = new StringBuilder(gauge);
            //Handle Agent string
            b.Replace("Altiris Agent", "Core");
            b.Replace("Altiris Software Update Agent", "Patch");
            b.Replace("Altiris Inventory Agent", "Inv.");
            b.Replace("Software Management Solution Agent", "Soft.");

            b.Replace("Basic Inventory", "Core");
            b.Replace("HW Inventory", "HW");
            b.Replace("OS Inventory", "OS");
            b.Replace("SW Inventory", "SW");
            b.Replace("UG Inventory", "UG");
            return b.ToString();
        }
    }
}
