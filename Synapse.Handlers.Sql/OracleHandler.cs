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

    public override object GetConfigInstance()
    {
        ExtendedHandlerConfig config = new ExtendedHandlerConfig();

        config.User = @"scott";
        config.Password = @"tiger";
        config.DataSource = @"(DESCRIPTION = (ADDRESS = (PROTOCOL = TCP)(HOST = localhost)(PORT = 1521))(CONNECT_DATA =(SERVER = DEDICATED)(SERVICE_NAME = XE)))";
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

        parms.Text = @"SELECT * FROM PRESIDENTS WHERE AGE > :AGE";
        parms.IsQuery = true;
        parms.Parameters = new List<ParameterType>();

        ParameterType p = new ParameterType();
        p.Name = @"AGE";
        p.Value = @"70";
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

        OracleDatabaseEngine db = new OracleDatabaseEngine(config, parameters, Logger);
        result.ExitData = db.ExecuteCommand(startInfo.IsDryRun);

        return result;
    }

    public void Logger(string context, string message)
    {
        OnLogMessage(context, message);
    }

}

