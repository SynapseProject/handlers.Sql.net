using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Xml;
using System.Xml.Serialization;
using System.IO;
using System.Data.SqlClient;
using System.Data.Common;

using Synapse.Core;
using Synapse.Handlers.Sql;

public class SqlServerHandler : HandlerRuntimeBase
{
    SqlServerHandlerConfig config = null;
    HandlerParameters parameters = null;

    public override IHandlerRuntime Initialize(string configStr)
    {
        config = this.DeserializeOrDefault<SqlServerHandlerConfig>(configStr);
        return base.Initialize(configStr);
    }

    public override ExecuteResult Execute(HandlerStartInfo startInfo)
    {
        ExecuteResult result = new ExecuteResult();
        result.Status = StatusType.Success;

        if (startInfo.Parameters != null)
            parameters = this.DeserializeOrDefault<HandlerParameters>(startInfo.Parameters);

        SqlServerDatabaseEngine db = new SqlServerDatabaseEngine(config, parameters);

        String command = parameters.Query;
        bool isStoredProc = false;
        if (!String.IsNullOrEmpty(parameters.StoredProcedure))
        {
            command = parameters.StoredProcedure;
            isStoredProc = true;
        }

        SqlConnection con = db.BuildConnection();
        DbDataReader reader = db.ExecuteCommand(con, command, isStoredProc, false);
        db.ParseResults(reader);

        con.Close();
        con.Dispose();

        return result;
    }
}

