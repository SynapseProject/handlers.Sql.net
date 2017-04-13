﻿using System;
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

        public SqlServerDatabaseEngine(SqlServerHandlerConfig config, HandlerParameters parameters) : base(parameters.Parameters)
        {
            this.Config = config;            
        }

        public SqlConnection BuildConnection()
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

    }
}
