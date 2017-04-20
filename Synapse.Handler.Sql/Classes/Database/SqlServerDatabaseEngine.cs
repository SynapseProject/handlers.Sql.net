using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SqlClient;
using System.Data.Common;

using Oracle.ManagedDataAccess.Client;

namespace Synapse.Handlers.Sql
{
    public class SqlServerDatabaseEngine : DatabaseEngine
    {
        SqlServerHandlerConfig Config;

        public SqlServerDatabaseEngine() { }

        public SqlServerDatabaseEngine(SqlServerHandlerConfig config, HandlerParameters parameters, Action<string, string> logger = null) : base(parameters, logger)
        {
            this.Config = config;
            this.OutputType = config.OutputType;
            this.OutputFile = config.OutputFile;
            this.parser = this.GetParser(config.OutputType, config.OutputFile);

            // Default ExecuteType For SqlServer is "Query"
            if (Parameters.ExecuteType == ExecuteTypeType.None)
                Parameters.ExecuteType = ExecuteTypeType.Query;
        }

        public override DbConnection BuildConnection()
        {
            SqlConnection con = new SqlConnection();

            StringBuilder sb = new StringBuilder();
            if (!String.IsNullOrWhiteSpace(Config.User))
                sb.Append(@"user id=" + Config.User + ";");
            if (!String.IsNullOrWhiteSpace(Config.Password))
                sb.Append(@"password=" + Config.Password + ";");
            if (!String.IsNullOrWhiteSpace(Config.DataSource))
                sb.Append(@"data source=" + Config.DataSource + ";");
            if (Config.IntegratedSecurity)
                sb.Append(@"Integrated Security=SSPI;");
            if (Config.TrustedConnection)
                sb.Append(@"Trusted_Connection=yes;");
            if (!String.IsNullOrWhiteSpace(Config.Database))
                sb.Append(@"database=" + Config.Database + ";");
            if (Config.ConnectionTimeout > 0)
                sb.Append(@"connection timeout=" + Config.ConnectionTimeout + ";");

            con.ConnectionString = sb.ToString();

            return con;
        }

        public override DbParameter AddParameter(DbCommand cmd, String name, String value, SqlParamterTypes type, int size, System.Data.ParameterDirection direction)
        {
            SqlParameter param = new SqlParameter();
            SqlCommand command = (SqlCommand)cmd;
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
            SqlCommand command = new SqlCommand();
            command.Connection = (SqlConnection)con;
            command.CommandText = commandText;
            return command;
        }

        public override void ParseParameter(DbParameter parameter)
        {
            SqlParameter param = (SqlParameter)parameter;
            parser.Parse(param.Direction, param.ParameterName, param.Value);
        }
    }
}
