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
    ExtendedHandlerConfig config = null;
    HandlerParameters parameters = null;

    public override IHandlerRuntime Initialize(string configStr)
    {
        config = this.DeserializeOrDefault<ExtendedHandlerConfig>(configStr);
        return base.Initialize(configStr);
    }

    public override ExecuteResult Execute(HandlerStartInfo startInfo)
    {
        ExecuteResult result = new ExecuteResult();
        result.Status = StatusType.Success;

        if (startInfo.Parameters != null)
            parameters = this.DeserializeOrDefault<HandlerParameters>(startInfo.Parameters);

        OracleDatabaseEngine db = new OracleDatabaseEngine(config, parameters, Logger);
        result.ExitData = db.ExecuteCommand(startInfo.IsDryRun);

        return result;
    }

    public void Logger(string context, string message)
    {
        OnLogMessage(context, message);
    }

}

