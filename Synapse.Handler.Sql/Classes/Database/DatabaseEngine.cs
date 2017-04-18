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
        public OutputTypeType OutputType { get; set; }
        public String OutputFile { get; set; }
        protected DBParser parser = new DBParser();

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
            parser.Logger = Logger;
            parser.Parse(parameter.Direction, parameter.ParameterName, parameter.Value);
        }

        public void ParseResults(DbDataReader reader)
        {
            parser.Logger = Logger;
            parser.Parse(reader);
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

        protected DBParser GetParser(OutputTypeType outputType, String outputFile = null)
        {
            DBParser parser = new DBParser(outputFile);
            switch (outputType)
            {
                case OutputTypeType.Xml:
                    parser = new DBParser(outputFile);
                    break;
                case OutputTypeType.Json:
                    parser = new DBParser(outputFile);
                    break;
                case OutputTypeType.Yaml:
                    parser = new DBParser(outputFile);
                    break;
            }

            parser.Logger = Logger;
            return parser;
        }
    }
}
