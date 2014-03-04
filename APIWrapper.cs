using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using System.Data.SqlClient;

using Altiris.Common;
using Altiris.Database;
using Altiris.NS;
using Altiris.NS.ItemManagement;
using Altiris.NS.Logging;
using Altiris.NS.ContextManagement;
using Altiris.NS.Security;
using Altiris.Resource;
using Altiris.PatchManagementCore.Web;
using Altiris.PatchManagementCore;
using Altiris.PatchManagementCore.Constants;
using Altiris.PatchManagementCore.Policies;
using Altiris.PatchManagementCore.Tasks.Server;
using Altiris.TaskManagement.Data;

namespace Symantec.CWoC.APIWrappers {
    class DatabaseAPI {
        public static DataTable GetTable(string sqlStatement) {
            DataTable t = new DataTable();
            try {
                using (DatabaseContext context = DatabaseContext.GetContext()) {
                    SqlCommand cmdAllResources = context.CreateCommand() as SqlCommand;
                    cmdAllResources.CommandText = sqlStatement;

                    using (SqlDataReader r = cmdAllResources.ExecuteReader()) {
                        t.Load(r);
                    }
                }
                return t;
            } catch (Exception e) {
                throw new Exception("Failed to execute SQL command...");
            }
        }

        public static int ExecuteNonQuery(string sqlStatement) {
            try {
                using (DatabaseContext context = DatabaseContext.GetContext()) {
                    SqlCommand sql_cmd = context.CreateCommand() as SqlCommand;
                    sql_cmd.CommandText = sqlStatement;

                    return sql_cmd.ExecuteNonQuery();
                }
            } catch (Exception e) {
                LoggingAPI.ReportException(e);
                throw new Exception("Failed to execute non query SQL command...");
            }

        }

        public static int ExecuteScalar(string sqlStatement) {
            try {
                using (DatabaseContext context = DatabaseContext.GetContext()) {
                    SqlCommand cmd = context.CreateCommand() as SqlCommand;

                    cmd.CommandText = sqlStatement;
                    Object result = cmd.ExecuteScalar();

                    return Convert.ToInt32(result);
                }
            } catch (Exception e) {
                Console.WriteLine("Error: {0}\nException message = {1}\nStack trace = {2}.", e.Message, e.InnerException, e.StackTrace);
                throw new Exception("Failed to execute scalar SQL command...");
            }
        }

    }
}