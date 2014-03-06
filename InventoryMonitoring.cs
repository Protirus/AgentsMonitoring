using Altiris.NS.ContextManagement;
using System;
using Symantec.CWoC.APIWrappers;
using System.Data;
using System.Data.Sql;
using System.Text;

namespace Symantec.CWoC {
    class InventoryMonitoring {
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

            Console.WriteLine(@"
var bi_options = { title: 'Basic Inventory (Core)' };
var hw_options = { title: 'Hardware Inventory' };
var os_options = { title: 'OS Inventory' };
var sw_options = { title: 'Software Inventory' };
var ug_options = { title: 'User Group Inventory' };
");

            Console.WriteLine("var candlestick_all_options = {legend:'none', title:'Computers sending Inventory'};");
            Console.WriteLine("var candlestick_out_options = {legend:'none', title:'Inventory data older than 4 weeks'};");
            Console.WriteLine();

            // Generate the gauge tables
            string inv_gauge = f.GetJSONFromTable(h.GetInventory_Gauge_Stats(), "inv_gauge_table", f.gauge_thead);
            Console.WriteLine(f.ConvertGauges(inv_gauge));

            // Generate the candlestick tables
            Console.WriteLine(f.GetJSONFromTable(h.GetInventory_CandleALL_Stats(), "candlestick_all_table", ""));
            Console.WriteLine(f.GetJSONFromTable(h.GetInventory_CandleOutdated_Stats(), "candlestick_out_table", ""));

            // Generate the tables used in the inventory line charts
            foreach (DataRow r in h.GetInventory_List().Rows) {
                Console.WriteLine(f.GetJSONFromTable(h.GetInventory_Line_Stats((string)r[0]), f.GetInvTableName((string)r[0]), f.inv_thead));
            }

            return 0;
        }
        private static readonly string version = "InventoryMonitoring version 1.\n\nBuilt for .Net 2.0, brought to you by {CWoC}.\n";
        private static readonly string help_msg = @"Usage: InventoryMonitoring  [Parameter]

Parameters:
    -h, --help              Show this help message
    -v, --version           Output program version

This program will output to the console (stdout) a Javascript file containing Inventory Monitoring data to be consumed by a web-interface.
";

    }

    // All SQL request come thru this class
    class DataHandler {
        // Inventory data handlers
        public DataTable GetInventory_List() {
            string sql = "select distinct([inventory type]) from TREND_InventoryStatus";
            return DatabaseAPI.GetTable(sql);
        }
        public DataTable GetInventory_Gauge_Stats() {
            string sql = "select[inventory type], cast([% up-to-date] as varchar(255)) from TREND_InventoryStatus where _Exec_id = (select MAX(_exec_id) from TREND_InventoryStatus) order by [inventory type]";
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
        public string ConvertGauges(string gauge) {
            StringBuilder b = new StringBuilder(gauge);
            b.Replace("Basic Inventory", "Core");
            b.Replace("HW Inventory", "HW");
            b.Replace("OS Inventory", "OS");
            b.Replace("SW Inventory", "SW");
            b.Replace("UG Inventory", "UG");
            return b.ToString();
        }
    }
}
