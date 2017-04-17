using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.Common;
using System.IO;
using System.Text.RegularExpressions;

namespace Synapse.Handlers.Sql
{
    public class DatabaseEngine
    {
        public Action<string, string> Logger { get; set; }

        public HandlerParameters Parameters { get; set; }

        public DatabaseEngine() { }

        public DatabaseEngine(HandlerParameters parms, Action<string, string> logger = null)
        {
            Parameters = parms;
            Logger = logger;
        }

        public void ExecuteCommand(bool isDryRun = false)
        {
            DbConnection con = BuildConnection();

            String cmdText = Parameters.Query;
            bool isStoredProc = false;
            if (!String.IsNullOrEmpty(Parameters.StoredProcedure))
            {
                cmdText = Parameters.StoredProcedure;
                isStoredProc = true;
            }


            DbCommand command = BuildCommand(con, cmdText);

            String connString = con.ConnectionString;
            connString = Regex.Replace(connString, @";password=.*?;", @";password=********;");
            Logger?.Invoke("ExecuteCommand", "Connection String - " + connString);

            if (isStoredProc)
            {
                command.CommandType = System.Data.CommandType.StoredProcedure;
                Logger?.Invoke("ExecuteCommand", "Stored Procuedre = " + command.CommandText);
            }
            else
                Logger?.Invoke("ExecuteCommand", "Query - " + command.CommandText);

            if (Parameters.Parameters != null)
            {
                foreach (ParameterType parameter in Parameters.Parameters)
                {
                    AddParameter(command, parameter.Name, parameter.Value, parameter.Type, parameter.Size, parameter.Direction);
                    Logger?.Invoke("ExecuteCommand", parameter.Direction + " Paramter - [" + parameter.Name + "] = [" + parameter.Value + "]");
                }
            }

            DbDataReader reader = null;
            try
            {
                con.Open();
                if (isDryRun)
                {
                    Logger?.Invoke("ExecuteQuery", "Database connection was sucessful.");
                    Logger?.Invoke("ExecuteQuery", "IsDryRun flag is set.  Query or StoredProcedure will not be executed.");
                    con.Close();
                }
                else
                {
                    if (isStoredProc && (this.GetType() == typeof(OracleDatabaseEngine)))
                    {
                        command.ExecuteNonQuery();
                    }
                    else
                    {
                        reader = command.ExecuteReader();
                    }

                    // Log Any Output Parameters From Call
                    foreach (DbParameter parameter in command.Parameters)
                    {
                        ParseParameter(parameter);
                    }

                    ParseResults(reader);
                }
            }
            catch (Exception e)
            {
                Logger?.Invoke("ExecuteQuery", "ERROR : " + e.Message);
                throw e;
            }

            finally
            {
                con.Close();
                con.Dispose();
            }
        }

        public virtual DbConnection BuildConnection()
        {
            Logger?.Invoke("BuildConnection", @"Unknown database type.  Can not build connection.");
            return null;
        }

        public virtual DbParameter AddParameter(DbCommand cmd, String name, String value, SqlParamterTypes type, int size, System.Data.ParameterDirection direction)
        {
            Logger?.Invoke("AddParameter", @"Unknown database type.  Can not create parameter.");
            return null;
        }

        public virtual DbCommand BuildCommand(DbConnection con, String commandText)
        {
            Logger?.Invoke("BuildCommand", @"Unknown database type.  Can not create command.");
            throw new Exception("Unknown Connection Type [" + con.GetType() + "]");
        }

        public virtual void ParseParameter(DbParameter parameter)
        {
            if (parameter.Direction != System.Data.ParameterDirection.Input)
                Logger?.Invoke("Results", parameter.Direction + " Parameter - [" + parameter.ParameterName + "] = [" + parameter.Value + "]");
        }

        public void ParseResults(DbDataReader reader)
        {
            StreamWriter writer = null;
            if (reader == null)
                return;

            if (reader.HasRows)
            {
                int totalSets = 0;
                do
                {
                    StringBuilder sb = new StringBuilder();

                    // Display Column Headers
                    for (int i = 0; i < reader.FieldCount; i++)
                    {
                        sb.Append(reader.GetName(i));
                        if (i != (reader.FieldCount - 1))
                            sb.Append(",");
                    }
                    Logger?.Invoke("Results", sb.ToString());


                    int totalRows = 0;
                    while (reader.Read())
                    {
                        sb.Clear();
                        for (int i = 0; i < reader.FieldCount; i++)
                        {
                            Type type = reader[i].GetType();
                            String field = FormatData(reader.GetFieldType(i), reader.GetValue(i));
                            sb.Append(field);
                            if (i != (reader.FieldCount - 1))
                                sb.Append(",");
                        }

                        totalRows++;
                        Logger?.Invoke("Results", sb.ToString());
                    }

                    Logger?.Invoke("Results", "Total Records : " + totalRows);

                    if (writer != null)
                    {
                        writer.Close();
                    }

                    totalSets++;

                } while (reader.NextResult());
            }
        }

        private String FormatData(Type type, Object field)
        {
            String data = field.ToString();

            if (type == typeof(String))
                data = @"""" + field.ToString() + @"""";

            return data;
        }

        protected ParameterType GetParameterByName(String name)
        {
            ParameterType retParam = null;

            if (Parameters != null)
                foreach (ParameterType param in Parameters.Parameters)
                    if (param.Name.Equals(name))
                    {
                        retParam = param;
                        break;
                    }

            return retParam;
        }
    }
}
