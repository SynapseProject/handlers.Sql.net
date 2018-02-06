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

public class OdbcHandler : HandlerRuntimeBase
{
    HandlerConfig config = null;
    HandlerParameters parameters = null;

    public override IHandlerRuntime Initialize(string configStr)
    {
        config = this.DeserializeOrDefault<SqlServerHandlerConfig>(configStr);
        return base.Initialize(configStr);
    }

    public override object GetConfigInstance()
    {
        HandlerConfig config = new HandlerConfig();

        config.ConnectionString = @"DSN=SOMEDSN";
        config.ConnectionTimeout = 60;
        config.CommandTimeout = 300;
        config.OutputType = OutputTypeType.Xml;
        config.OutputFile = @"C:\Temp\FileNotReallyNeeded.xml";
        config.PrettyPrint = true;

        return config;
    }

    public override object GetParametersInstance()
    {
        HandlerParameters parms = new HandlerParameters();

        parms.Text = @"SELECT TABLE_SCHEMA, TABLE_NAME FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_TYPE = 'BASE TABLE' AND TABLE_CATALOG = ?";
        parms.IsQuery = true;
        parms.Parameters = new List<ParameterType>();

        ParameterType p = new ParameterType();
        p.Name = @"NotReallyUsedInOdbc";
        p.Value = @"SANDBOX";
        parms.Parameters.Add(p);

        return parms;

    }

    public override ExecuteResult Execute(HandlerStartInfo startInfo)
    {
        ExecuteResult result = new ExecuteResult();
        result.Status = StatusType.Success;

        // TODO : Implement DryRun Functionality
        if (startInfo.IsDryRun)
            throw new NotImplementedException("Dry Run Functionality Has Not Yet Been Implemented.");

        if (startInfo.Parameters != null)
            parameters = this.DeserializeOrDefault<HandlerParameters>(startInfo.Parameters);

        OdbcDatabaseEngine db = new OdbcDatabaseEngine(config, parameters, Logger);
        result.ExitData = db.ExecuteCommand(startInfo.IsDryRun);

        return result;
    }

    public void Logger(string context, string message)
    {
        OnLogMessage(context, message);
    }
}

