using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Xml;
using System.Xml.Serialization;
using System.IO;
using System.Data.Common;

using Oracle.ManagedDataAccess.Client;
using Oracle.ManagedDataAccess.Types;

using Synapse.Core;
using Synapse.Handlers.Sql;

public class OracleHandler : HandlerRuntimeBase
{
    OracleHandlerConfig config = null;
    HandlerParameters parameters = null;

    public override IHandlerRuntime Initialize(string configStr)
    {
        config = this.DeserializeOrDefault<OracleHandlerConfig>(configStr);
        return base.Initialize(configStr);
    }

    public override ExecuteResult Execute(HandlerStartInfo startInfo)
    {
        ExecuteResult result = new ExecuteResult();
        result.Status = StatusType.Success;

        if (startInfo.Parameters != null)
            parameters = this.DeserializeOrDefault<HandlerParameters>(startInfo.Parameters);

        OracleDatabaseEngine db = new OracleDatabaseEngine(config, parameters, Logger);

        String command = parameters.Query;
        bool isStoredProc = false;
        if (!String.IsNullOrEmpty(parameters.StoredProcedure))
        {
            command = parameters.StoredProcedure;
            isStoredProc = true;
        }

        OracleConnection con = db.BuildConnection();
        DbDataReader reader = db.ExecuteCommand(con, command, isStoredProc, false);
        db.ParseResults(reader);

        con.Close();
        con.Dispose();

        return result;
    }

    public void Logger(string context, string message)
    {
        OnLogMessage(context, message);
    }

}

