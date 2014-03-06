using Altiris.NS.ContextManagement;
using System;
using Symantec.CWoC.APIWrappers;
using System.Data;
using System.Data.Sql;
using System.Text;

namespace Symantec.CWoC {
    class AgentsMonitoring {
        static int Main(string[] args) {
            // This tool generates the javascript as seen in sample Web-UI/agent_trending_data.js
            // This requires quite a few SQL queries, so I'll probably start with this.

            if (args.Length > 0) {
                string arg = args[0].ToLower();
                if (arg == "-h" || arg == "--help") {
                    Console.WriteLine(help_msg);
                    return 1;
                } else if (arg == "-v" || arg == "--version") {
                    Console.WriteLine(version);
                    return 1;
                } else {
                    return -1;
                }
            }

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

            Console.WriteLine("var candlestick_allagents_options = {legend:'none', title:'Agents installed'}");
            Console.WriteLine("var candlestick_outagents_options = {legend:'none', title:'Agents to upgrade'};");
            Console.WriteLine();

            // Generate the gauge tables
            string agent_gauge = f.GetJSONFromTable(h.GetAgent_Gauge_Stats(), "agent_gauge_table", f.gauge_thead);
            Console.WriteLine(f.ConvertGauges(agent_gauge));

            // Generate the candlestick tables
            Console.WriteLine(f.GetJSONFromTable(h.GetAgent_CandlestickALL_Stats(), "candlestick_agent_table", ""));
            Console.WriteLine(f.GetJSONFromTable(h.GetAgent_CandlestickOutdated_Stats(), "candlestick_agent_outdated_table", ""));

            // Generate the tables used in the agent line charts
            foreach (DataRow r in agent_list.Rows) {
                Console.WriteLine(f.GetJSONFromTable(h.GetAgent_Line_Stats(r[0].ToString()), f.GetAgentTableName((string)r[0]), f.agent_thead));
            }
            return 0;
        }

        private static readonly string version = "AgentsMonitoring version 1.\n\nBuilt for .Net 2.0, brought to you by {CWoC}.\n";
        private static readonly string help_msg = @"Usage: AgentsMonitoring  [Parameter]

Parameters:
    -h, --help              Show this help message
    -v, --version           Output program version only

This program will output to the console (stdout) a Javascript file containing Agent Monitoring data to be consumed by a web-interface.
";
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
            string sql = "select [Agent Name], cast([% up-to-date] as varchar(255)) from TREND_AgentVersions where _Exec_id = (select MAX(_exec_id) from TREND_AgentVersions) order by [agent name]";
            return DatabaseAPI.GetTable(sql);
        }
        public DataTable GetAgent_CandlestickALL_Stats() {
            string sql = @"
select a1.[Agent Name], a1.Lowest /* LOW */, a3.[Agents Installed] as 'Previous' /* OPENING */, a2.[Agents Installed] as 'Current' /* CLOSING */, a1.Highest /* MAX */
  from (
select a1.[Agent Name], MIN(a1.[Agents installed]) as 'Lowest', MAX(a1.[agents installed]) as 'Highest'
  from TREND_AgentVersions a1
 group by [agent name]
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
 group by [agent name]
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
    }
    // Output data is generated thru this class
    class JSformatter {
        public readonly string agent_thead = "['Date', 'Agent #', 'OK', 'NOK']";
        public readonly string gauge_thead = "['Label', 'Value']";
        public string GetJSONFromTable(DataTable t, string entry, string head) {
            StringBuilder b = new StringBuilder();

            b.AppendLine("var " + entry + " = [");
            if (head != "") {
                b.AppendFormat("\t{0},\n", head);
            }

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
        public string ConvertGauges(string gauge) {
            StringBuilder b = new StringBuilder(gauge);
            //Handle Agent string
            b.Replace("Altiris Agent", "Core");
            b.Replace("Altiris Software Update Agent", "Patch");
            b.Replace("Altiris Inventory Agent", "Inv.");
            b.Replace("Software Management Solution Agent", "Soft.");
            return b.ToString();
        }
    }
}
