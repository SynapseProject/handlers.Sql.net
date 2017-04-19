using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SqlClient;
using System.Data.Common;

using Oracle.ManagedDataAccess.Client;
using Oracle.ManagedDataAccess.Types;

namespace Synapse.Handlers.Sql
{
    public class OracleDatabaseEngine : DatabaseEngine
    {
        HandlerConfig Config;

        public OracleDatabaseEngine() { }

        public OracleDatabaseEngine(HandlerConfig config, HandlerParameters parameters, Action<string, string> logger = null, OutputTypeType outputType = OutputTypeType.None, String outputFile = null) : base(parameters, logger)
        {
            this.Config = config;
            this.OutputType = config.OutputType;
            this.OutputFile = config.OutputFile;
            this.parser = this.GetParser(config.OutputType, config.OutputFile);
        }

        public override DbConnection BuildConnection()
        {
            OracleConnection con = new OracleConnection();

            StringBuilder sb = new StringBuilder();
            if (!String.IsNullOrWhiteSpace(Config.User))
                sb.Append(@"user id=" + Config.User + ";");
            if (!String.IsNullOrWhiteSpace(Config.Password))
                sb.Append(@"password=" + Config.Password + ";");
            if (!String.IsNullOrWhiteSpace(Config.DataSource))
                sb.Append(@"data source=" + Config.DataSource + ";");

            con.ConnectionString = sb.ToString();

            return con;
        }

        public override DbParameter AddParameter(DbCommand cmd, String name, String value, SqlParamterTypes type, int size, System.Data.ParameterDirection direction)
        {
            OracleParameter param = new OracleParameter();
            OracleCommand command = (OracleCommand)cmd;
            param.ParameterName = name;
            param.Direction = direction;
            param.Value = value;
            param.Size = size;

            int enumValue = (int)type;
            if (enumValue >= 100)
                param.OracleDbType = (OracleDbType)Enum.Parse(typeof(OracleDbType), type.ToString());
            else
                param.DbType = (System.Data.DbType)Enum.Parse(typeof(System.Data.DbType), type.ToString());

            // For Oracle Functions, ReturnValue Must Be First Parameter
            if (param.Direction == System.Data.ParameterDirection.ReturnValue)
                command.Parameters.Insert(0, param);
            else
                command.Parameters.Add(param);
            return param;
        }

        public override DbCommand BuildCommand(DbConnection con, String commandText)
        {
            OracleCommand command = new OracleCommand();
            command.Connection = (OracleConnection)con;
            command.CommandText = commandText;
            return command;
        }

        public override void ParseParameter(DbParameter parameter)
        {
            OracleParameter param = (OracleParameter)parameter;

            if (parameter.Direction != System.Data.ParameterDirection.Input)
            {
                if (param.OracleDbType == OracleDbType.RefCursor)
                {
                    OracleDataReader reader = ((OracleRefCursor)param.Value).GetDataReader();
                    parser.Parse(reader);
                }
                else
                    parser.Parse(param.Direction, param.ParameterName, param.Value);
            }
        }


    }
}
