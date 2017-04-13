using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.Common;
using System.IO;

namespace Synapse.Handlers.Sql
{
    public class DatabaseEngine
    {
        public List<ParameterType> Parameters { get; set; }

        public DatabaseEngine() { }

        public DatabaseEngine(List<ParameterType> parms)
        {
            Parameters = parms;
        }

        public DbDataReader ExecuteCommand(DbConnection con, String cmdText, bool isStoredProc = false, bool isDryRun = false)
        {
            DbCommand command = BuildCommand(con, cmdText);

//            String connString = con.ConnectionString;
//            connString = Regex.Replace(connString, @";password=.*?;", @";password=********;");
//            OnStepProgress("ExecuteQuery", "Connection String - " + connString);

            if (isStoredProc)
            {
                command.CommandType = System.Data.CommandType.StoredProcedure;
//                OnStepProgress("ExecuteQuery", "Stored Procedure - " + command.CommandText);
            }
            else
//                OnStepProgress("ExecuteQuery", "Query - " + command.CommandText);

            if (Parameters != null)
            {
                foreach (ParameterType parameter in Parameters)
                {
                    AddParameter(command, parameter.Name, parameter.Value, parameter.Type, parameter.Size, parameter.Direction);
//                    OnStepProgress("ExeucteQuery", parameter.Direction + " Paramter - [" + parameter.Name + "] = [" + parameter.Value + "]");
                }
            }

            DbDataReader reader = null;
            try
            {
                con.Open();
                if (isDryRun)
                {
//                    OnStepProgress("ExecuteQuery", "Database connection was sucessful.");
//                    OnStepProgress("ExecuteQuery", "IsDryRun flag is set.  Query or StoredProcedure will not be executed.");
                    con.Close();
                }
                else
                {
                    if (isStoredProc && (this.GetType() == typeof(OracleDatabaseEngine)))
                    {
                        command.ExecuteNonQuery();
                    }
                    else
                    {
                        reader = command.ExecuteReader();
                    }

                    // Log Any Output Parameters From Call
                    foreach (DbParameter parameter in command.Parameters)
                    {
                        ParseParameter(parameter);
                    }
                }
            }
            catch (Exception e)
            {
//                OnStepProgress("ExecuteQuery", "ERROR : " + e.Message);
                throw e;
            }

            return reader;
        }

        public virtual DbParameter AddParameter(DbCommand cmd, String name, String value, SqlParamterTypes type, int size, System.Data.ParameterDirection direction)
        {
//            OnStepProgress("BuildParameter", @"Unknown database type.  Can not create parameter.");
            return null;
        }

        public virtual DbCommand BuildCommand(DbConnection con, String commandText)
        {
            //            OnStepProgress("BuildCommand", @"Unknown database type.  Can not create command.");
            throw new Exception("Unknown Connection Type [" + con.GetType() + "]");
        }

        public virtual void ParseParameter(DbParameter parameter)
        {
            //TODO : Implement Me
//            if (parameter.Direction != System.Data.ParameterDirection.Input)
//                OnStepProgress("Results", parameter.Direction + " Parameter - [" + parameter.ParameterName + "] = [" + parameter.Value + "]");
        }

        public void ParseResults(DbDataReader reader, String fileName = null, String delimeter = null, bool showColumnNames = true, bool showResults = true, bool appendToFile = true, bool mergeResults = false)
        {
            StreamWriter writer = null;
            if (reader == null)
                return;

            if (reader.HasRows)
            {
                int totalSets = 0;
                do
                {
                    if (!String.IsNullOrWhiteSpace(fileName))
                    {
                        String setFileName = fileName;
                        bool doAppend = appendToFile;
                        if (totalSets > 0 && !mergeResults)
                        {
                            String filePath = System.IO.Path.GetDirectoryName(fileName);
                            String fileRoot = System.IO.Path.GetFileNameWithoutExtension(fileName);
                            String fileExt = System.IO.Path.GetExtension(fileName);
                            setFileName = String.Format(@"{0}\{1}_{2:D3}{3}", filePath, fileRoot, (totalSets + 1), fileExt);
                        }

                        // Merge Results Means Append To File
                        if (totalSets > 0 && mergeResults)
                            doAppend = true;

                        writer = new StreamWriter(setFileName, doAppend);
//                        OnStepProgress("Results", "OutputFile - [" + setFileName + "]");
                    }

                    StringBuilder sb = new StringBuilder();
                    if (showColumnNames)
                    {
                        for (int i = 0; i < reader.FieldCount; i++)
                        {
                            sb.Append(reader.GetName(i));
                            if (i != (reader.FieldCount - 1))
                                sb.Append(delimeter);
                        }
                        WriteRow(sb.ToString(), writer, showResults);
                    }


                    int totalRows = 0;
                    while (reader.Read())
                    {
                        sb.Clear();
                        for (int i = 0; i < reader.FieldCount; i++)
                        {
                            Type type = reader[i].GetType();
                            String field = FormatData(reader.GetFieldType(i), reader.GetValue(i));
                            sb.Append(field);
                            if (i != (reader.FieldCount - 1))
                                sb.Append(delimeter);
                        }

                        totalRows++;
                        WriteRow(sb.ToString(), writer, showResults);
                    }

//                    OnStepProgress("Results", "Total Records : " + totalRows);

                    if (writer != null)
                    {
                        writer.Close();
                    }

                    totalSets++;

                } while (reader.NextResult());

            }

        }

        protected void WriteParameter(String name, Object value, String fileName, bool showColumnNames, bool appendToFile)
        {
            StreamWriter writer = null;

            if (!String.IsNullOrWhiteSpace(fileName))
            {
                writer = new StreamWriter(fileName, appendToFile);
//                OnStepProgress("Results", "OutputFile - [" + fileName + "]");

                if (showColumnNames)
                    writer.WriteLine(name);
                if (value == null)
                    writer.WriteLine("");
                else
                    writer.WriteLine(value.ToString());

                writer.Close();
                writer.Dispose();
            }
        }

        private String FormatData(Type type, Object field)
        {
            String data = field.ToString();

            if (type == typeof(String))
                data = @"""" + field.ToString() + @"""";

            return data;
        }

        private void WriteRow(String line, StreamWriter file, bool showResults = true)
        {
            if (file == null)
            {
//                OnStepProgress("Results", line);
            }
            else
            {
                file.WriteLine(line);
//                if (showResults)
//                   OnStepProgress("Results", line);
            }
        }

        protected ParameterType GetParameterByName(String name)
        {
            ParameterType retParam = null;

            if (Parameters != null)
                foreach (ParameterType param in Parameters)
                    if (param.Name.Equals(name))
                    {
                        retParam = param;
                        break;
                    }

            return retParam;
        }
    }
}
