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
        protected DbParser parser = new DbParser();

        public DatabaseEngine() { }

        public DatabaseEngine(HandlerParameters parms, Action<string, string> logger = null)
        {
            Parameters = parms;
            Logger = logger;
        }

        public void ExecuteCommand(bool isDryRun = false)
        {
            DbConnection con = BuildConnection();
            DbCommand command = BuildCommand(con, Parameters.Command);

            String connString = con.ConnectionString;
            connString = Regex.Replace(connString, @";password=.*?;", @";password=********;");
            Logger?.Invoke("ExecuteCommand", "Connection String - " + connString);

            command.CommandType = GetCommandType(Parameters.CommandType);
            Logger?.Invoke("ExecuteCommand", "Command = " + command.CommandText);
            Logger?.Invoke("ExecuteCommand", "CommandType = " + command.CommandType);

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
                    int rowsChanged = 0;
                    Logger?.Invoke("ExecuteCommand", "ExecuteType = " + Parameters.ExecuteType);
                    if (Parameters.ExecuteType == ExecuteTypeType.NonQuery)
                    {
                        rowsChanged = command.ExecuteNonQuery();
                        Logger?.Invoke("ExecuteCommand", "Rows Affected = " + rowsChanged);
                    }
                    else
                        reader = command.ExecuteReader();

                    parser.Open();

                    ParseResults(reader);

                    // Log Any Output Parameters From Call
                    for (int i=0; i<command.Parameters.Count; i++)
                    {
                        DbParameter parameter = command.Parameters[i];
                        ParseParameter(parameter);
                    }

                    parser.Close();
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

        protected DbParser GetParser(OutputTypeType outputType, String outputFile = null)
        {
            DbParser parser = new DbParser(outputFile);
            switch (outputType)
            {
                case OutputTypeType.Xml:
                    parser = new XmlDbParser(outputFile);
                    break;
                case OutputTypeType.Json:
                    parser = new JsonDbParser(outputFile);
                    break;
                case OutputTypeType.Yaml:
                    parser = new YamlDbParser(outputFile);
                    break;
            }

            parser.Logger = Logger;
            return parser;
        }

        protected System.Data.CommandType GetCommandType(CommandTypeType type)
        {
            System.Data.CommandType cmdType = System.Data.CommandType.Text;

            if (type == CommandTypeType.StoredProcedure)
                cmdType = System.Data.CommandType.StoredProcedure;
            else if (type == CommandTypeType.TableDirect)
                cmdType = System.Data.CommandType.TableDirect;

            return cmdType;
        }
    }
}
