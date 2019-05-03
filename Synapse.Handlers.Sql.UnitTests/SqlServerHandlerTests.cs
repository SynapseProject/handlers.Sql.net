// NUnit 3 tests
// See documentation : https://github.com/nunit/docs/wiki/NUnit-Documentation
using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using Synapse.Handlers.Sql;
using Synapse.Core;
using System;
using System.IO;

namespace Synapse.Handlers.Sql.UnitTests
{
    [TestFixture]
    public class SqlServerHandlerTests
    {
        string _database = "SANDBOX";
        string _dataSource = @"localhost\SQLEXPRESS";
        
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
            SqlServerHandlerConfig config = new SqlServerHandlerConfig
            {
                Database = _database,
                DataSource = _dataSource,
                OutputType = outputType,
                PrettyPrint = true,
                //IntegratedSecurity = true
                User = "siewhooi",
                Password = "siewhooi"
            };

            HandlerParameters parameters = new HandlerParameters
            {
                Text = @"SELECT COUNT(*) TotalRows FROM dbo.PRESIDENTS",
                IsQuery = true
            };
            SqlServerDatabaseEngine db = new SqlServerDatabaseEngine( config, parameters );

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
            SqlServerHandlerConfig config = new SqlServerHandlerConfig
            {
                Database = _database,
                DataSource = _dataSource,
                OutputFile = $@"c:\temp\outfile.{outputType}",
                OutputType = outputType,
                PrettyPrint = true,
                IntegratedSecurity = true
            };
            HandlerParameters parameters = new HandlerParameters
            {
                Text = @"SELECT * FROM PRESIDENTS WHERE AGE > @Age",
                IsQuery = true,
                Parameters = new List<ParameterType>
                {
                    new ParameterType{ Name = "@Age", Value = "90", Direction = System.Data.ParameterDirection.Input, Type = SqlParamterTypes.Int32 }
                }
            };
            SqlServerDatabaseEngine db = new SqlServerDatabaseEngine( config, parameters );

            string exitData = db.ExecuteCommand();

            Console.WriteLine( exitData );

            string actualResult = File.ReadAllText( config.OutputFile );
            string expectedResult = File.ReadAllText( Path.Combine( Environment.CurrentDirectory, $"presidents-older-than-90.{outputType}" ) );

            Assert.AreEqual( expectedResult, actualResult );
        }
        [Test]
        public void StoredProcedureWithInputOutputParameter()
        {
            SqlServerHandlerConfig config = new SqlServerHandlerConfig
            {
                Database = _database,
                DataSource = _dataSource,
                OutputType = OutputTypeType.Yaml,
                PrettyPrint = true,
                IntegratedSecurity = true
            };
            HandlerParameters parameters = new HandlerParameters
            {
                StoredProcedure = "dbo.uspDouble",
                IsQuery = true,
                Parameters = new List<ParameterType>
                {
                    new ParameterType{ Name = "@Param1", Value = "100", Direction = System.Data.ParameterDirection.InputOutput, Type = SqlParamterTypes.Int32 }
                }
            };
            SqlServerDatabaseEngine db = new SqlServerDatabaseEngine( config, parameters );

            string actualResult = db.ExecuteCommand();

            Console.WriteLine( actualResult );

            string expectedResult = File.ReadAllText( Path.Combine( Environment.CurrentDirectory, $"uspDouble.{config.OutputType}" ) );

            Assert.AreEqual( expectedResult, actualResult );
        }
        [Test]
        public void StoredProcedureWithMultipleReturnTypes()
        {
            SqlServerHandlerConfig config = new SqlServerHandlerConfig
            {
                Database = _database,
                DataSource = _dataSource,
                OutputType = OutputTypeType.Yaml,
                PrettyPrint = true,
                IntegratedSecurity = true
            };
            HandlerParameters parameters = new HandlerParameters
            {
                StoredProcedure = "dbo.MultiParams",
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
            SqlServerDatabaseEngine db = new SqlServerDatabaseEngine( config, parameters );

            string actualResult = db.ExecuteCommand();

            Console.WriteLine( actualResult );

            string expectedResult = File.ReadAllText( Path.Combine( Environment.CurrentDirectory, $"MultiParams.{config.OutputType}" ) );

            Assert.AreEqual( expectedResult, actualResult );
        }
        [Test]
        public void FunctionWithParameters()
        {
            SqlServerHandlerConfig config = new SqlServerHandlerConfig
            {
                Database = _database,
                DataSource = _dataSource,
                OutputType = OutputTypeType.Yaml,
                PrettyPrint = true,
                IntegratedSecurity = true
            };
            HandlerParameters parameters = new HandlerParameters
            {
                StoredProcedure = "dbo.funcAdd",
                IsQuery = true,
                Parameters = new List<ParameterType>
                {
                    new ParameterType{ Name = "@Result", Direction = System.Data.ParameterDirection.ReturnValue, Type = SqlParamterTypes.Int32 },
                    new ParameterType{ Name = "@Param1", Value = "100", Direction = System.Data.ParameterDirection.Input, Type = SqlParamterTypes.Int32 },
                    new ParameterType{ Name = "@Param2", Value = "200", Direction = System.Data.ParameterDirection.Input, Type = SqlParamterTypes.Int32 }
                }
            };
            SqlServerDatabaseEngine db = new SqlServerDatabaseEngine( config, parameters );

            string actualResult = db.ExecuteCommand();

            Console.WriteLine( actualResult );

            string expectedResult = File.ReadAllText( Path.Combine( Environment.CurrentDirectory, $"funcAdd.{config.OutputType}" ) );

            Assert.AreEqual( expectedResult, actualResult );
        }
    }
}
