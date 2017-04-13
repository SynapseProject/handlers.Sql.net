using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Oracle.ManagedDataAccess.Client;

namespace Synapse.Handlers.Sql
{
    public class OracleDatabaseEngine : DatabaseEngine
    {
        OracleHandlerConfig Config;

        public OracleDatabaseEngine() { }

        public OracleDatabaseEngine(OracleHandlerConfig config, HandlerParameters parameters) : base(parameters.Parameters)
        {
            this.Config = config;            
        }

        public OracleConnection BuildConnection()
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

    }
}
