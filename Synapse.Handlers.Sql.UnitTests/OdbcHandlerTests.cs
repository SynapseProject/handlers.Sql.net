using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using System.IO;
using Synapse.Core;

namespace Synapse.Handlers.Sql.UnitTests
{
    [TestFixture]
    public class OdbcHandlerTests
    {
        string _connectionString = "dsn=sandbox;database=sandbox";

        [OneTimeSetUp]
        public void Init()
        {
            Directory.SetCurrentDirectory( Path.Combine( System.Reflection.Assembly.GetExecutingAssembly().Location, @"..\..\..\Files" ) );
        }
        [Test]
        [TestCase( OutputTypeType.Json )]
        [TestCase( OutputTypeType.Xml )]
        [TestCase( OutputTypeType.Yaml )]
        public void SimpleCommandText(OutputTypeType outputType)
        {
            // build connection
            HandlerConfig config = new HandlerConfig
            {
                ConnectionString = _connectionString,
                OutputType = outputType,
                PrettyPrint = true
            };

            HandlerParameters parameters = new HandlerParameters
            {
                Text = @"SELECT COUNT(*) TotalRows FROM dbo.PRESIDENTS",
                IsQuery = true
            };
            OdbcDatabaseEngine db = new OdbcDatabaseEngine( config, parameters );

            string actualResult = db.ExecuteCommand();

            Console.WriteLine( actualResult );

            string expectedResult = File.ReadAllText( Path.Combine( Environment.CurrentDirectory, $"count-presidents.{outputType}" ) );

            Assert.AreEqual( expectedResult, actualResult );
        }
        [Test]
        [TestCase( OutputTypeType.Json )]
        [TestCase( OutputTypeType.Xml )]
        [TestCase( OutputTypeType.Yaml )]
        [TestCase( OutputTypeType.Csv )]
        public void CommandTextWithInputParameterAndOutputFile(OutputTypeType outputType)
        {
            HandlerConfig config = new HandlerConfig
            {
                ConnectionString = _connectionString,
                OutputFile = $@"c:\temp\outfile.{outputType}",
                OutputType = outputType,
                PrettyPrint = true
            };
            HandlerParameters parameters = new HandlerParameters
            {
                Text = @"SELECT * FROM PRESIDENTS WHERE AGE > ?",
                IsQuery = true,
                Parameters = new List<ParameterType>
                {
                    new ParameterType{ Name = "@Age", Value = "90", Direction = System.Data.ParameterDirection.Input, Type = SqlParamterTypes.Int32 }
                }
            };
            OdbcDatabaseEngine db = new OdbcDatabaseEngine( config, parameters );

            string exitData = db.ExecuteCommand();

            Console.WriteLine( exitData );

            string actualResult = File.ReadAllText( config.OutputFile );
            string expectedResult = File.ReadAllText( Path.Combine( Environment.CurrentDirectory, $"presidents-older-than-90.{outputType}" ) );

            Assert.AreEqual( expectedResult, actualResult );
        }
        [Test]
        public void StoredProcedureWithInputOutputParameter()
        {
            HandlerConfig config = new HandlerConfig
            {
                ConnectionString = _connectionString,
                OutputType = OutputTypeType.Yaml,
                PrettyPrint = true
            };
            HandlerParameters parameters = new HandlerParameters
            {
                StoredProcedure = "{ call dbo.uspDouble(?) }",
                IsQuery = true,
                Parameters = new List<ParameterType>
                {
                    new ParameterType{ Name = "@Param1", Value = "100", Direction = System.Data.ParameterDirection.InputOutput, Type = SqlParamterTypes.Int32 }
                }
            };
            OdbcDatabaseEngine db = new OdbcDatabaseEngine( config, parameters );

            string actualResult = db.ExecuteCommand();

            Console.WriteLine( actualResult );

            string expectedResult = File.ReadAllText( Path.Combine( Environment.CurrentDirectory, $"uspDouble.{config.OutputType}" ) );

            Assert.AreEqual( expectedResult, actualResult );
        }
        [Test]
        public void StoredProcedureWithMultipleReturnTypes()
        {
            HandlerConfig config = new HandlerConfig
            {
                ConnectionString = _connectionString,
                OutputType = OutputTypeType.Yaml,
                PrettyPrint = true
            };
            HandlerParameters parameters = new HandlerParameters
            {
                StoredProcedure = "{? = call dbo.MultiParams(?, ?, ?, ?)}",
                IsQuery = true,
                Parameters = new List<ParameterType>
                {
                    new ParameterType{ Name = "@ReturnValue", Direction = System.Data.ParameterDirection.ReturnValue, Type = SqlParamterTypes.Int32 },
                    new ParameterType{ Name = "@Param1", Value = "100", Direction = System.Data.ParameterDirection.Input, Type = SqlParamterTypes.Int32 },
                    new ParameterType{ Name = "@Param2", Direction = System.Data.ParameterDirection.Output, Type = SqlParamterTypes.Int32 },
                    new ParameterType{ Name = "@Param3", Direction = System.Data.ParameterDirection.Output, Type = SqlParamterTypes.Int32 },
                    new ParameterType{ Name = "@Param4", Direction = System.Data.ParameterDirection.Output, Type = SqlParamterTypes.DateTime }
                }
            };
            OdbcDatabaseEngine db = new OdbcDatabaseEngine( config, parameters );

            string actualResult = db.ExecuteCommand();

            Console.WriteLine( actualResult );

            string expectedResult = File.ReadAllText( Path.Combine( Environment.CurrentDirectory, $"MultiParams.{config.OutputType}" ) );

            Assert.AreEqual( expectedResult, actualResult );
        }
        [Test]
        public void FunctionWithParameters()
        {
            HandlerConfig config = new HandlerConfig
            {
                ConnectionString = _connectionString,
                OutputType = OutputTypeType.Yaml,
                PrettyPrint = true
            };
            HandlerParameters parameters = new HandlerParameters
            {
                StoredProcedure = "{? = call dbo.funcAdd(?, ?) }",
                IsQuery = true,
                Parameters = new List<ParameterType>
                {
                    new ParameterType{ Name = "@Result", Direction = System.Data.ParameterDirection.ReturnValue, Type = SqlParamterTypes.Int32 },
                    new ParameterType{ Name = "@Param1", Value = "100", Direction = System.Data.ParameterDirection.Input, Type = SqlParamterTypes.Int32 },
                    new ParameterType{ Name = "@Param2", Value = "200", Direction = System.Data.ParameterDirection.Input, Type = SqlParamterTypes.Int32 }
                    
                }
            };
            OdbcDatabaseEngine db = new OdbcDatabaseEngine( config, parameters );

            string actualResult = db.ExecuteCommand();

            Console.WriteLine( actualResult );

            string expectedResult = File.ReadAllText( Path.Combine( Environment.CurrentDirectory, $"funcAdd.{config.OutputType}" ) );

            Assert.AreEqual( expectedResult, actualResult );
        }
    }
    
}
