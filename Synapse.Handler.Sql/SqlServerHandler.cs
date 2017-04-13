using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Xml;
using System.Xml.Serialization;
using System.IO;

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


        return result;
    }
}

