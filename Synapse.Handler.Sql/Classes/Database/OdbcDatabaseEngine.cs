using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
//using System.Data.SqlClient;
using System.Data.Common;
using System.Data.Odbc;

using Oracle.ManagedDataAccess.Client;

namespace Synapse.Handlers.Sql
{
    public class OdbcDatabaseEngine : DatabaseEngine
    {
        HandlerConfig Config;

        public OdbcDatabaseEngine() { }

        public OdbcDatabaseEngine(HandlerConfig config, HandlerParameters parameters, Action<string, string> logger = null) : base(parameters, logger)
        {
            this.Config = config;
            this.OutputType = config.OutputType;
            this.OutputFile = config.OutputFile;
            this.parser = this.GetParser(config.OutputType, config.OutputFile);
        }

        public override DbConnection BuildConnection()
        {
            OdbcConnection con = new OdbcConnection();
            con.ConnectionString = Config.ConnectionString;
            return con;
        }

        public override DbParameter AddParameter(DbCommand cmd, String name, String value, SqlParamterTypes type, int size, System.Data.ParameterDirection direction)
        {
            OdbcParameter param = new OdbcParameter();
            OdbcCommand command = (OdbcCommand)cmd;
            param.ParameterName = name;
            param.Value = value;
            param.Direction = direction;
            param.Size = size;

            param.DbType = (System.Data.DbType)Enum.Parse(typeof(System.Data.DbType), type.ToString());

            command.Parameters.Add(param);
            return param;
        }

        public override DbCommand BuildCommand(DbConnection con, String commandText)
        {
            OdbcCommand command = new OdbcCommand();
            command.Connection = (OdbcConnection)con;
            command.CommandText = commandText;
            return command;
        }

        public override void ParseParameter(DbParameter parameter)
        {
            OdbcParameter param = (OdbcParameter)parameter;
            parser.Parse(param.Direction, param.ParameterName, param.Value);
        }
    }
}
