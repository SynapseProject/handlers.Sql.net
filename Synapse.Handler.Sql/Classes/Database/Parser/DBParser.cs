using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.Common;
using System.Data;
using System.IO;

namespace Synapse.Handlers.Sql
{
    public class DBParser
    {
        public Action<string, string> Logger { get; set; }
        public String OutputFile { get; set; }

        protected StreamWriter _file;
        protected StringBuilder _exitData = new StringBuilder();

        public DBParser() { }

        public DBParser(String outputFile)
        {
            OutputFile = outputFile;
            if (File.Exists(OutputFile))
                File.Delete(OutputFile);
        }

        public virtual String Parse(DbDataReader reader)
        {
            if (reader == null)
                return String.Empty;

            if (!String.IsNullOrEmpty(OutputFile))
                _file = new StreamWriter(OutputFile, true);

            if (reader.HasRows)
            {
                int totalSets = 0;
                do
                {
                    StringBuilder row = new StringBuilder();

                    // Display Column Headers
                    for (int i = 0; i < reader.FieldCount; i++)
                    {
                        row.Append(reader.GetName(i));
                        if (i != (reader.FieldCount - 1))
                            row.Append(",");
                    }
                    WriteRow(row.ToString());

                    int totalRows = 0;
                    while (reader.Read())
                    {
                        row.Clear();
                        for (int i = 0; i < reader.FieldCount; i++)
                        {
                            Type type = reader[i].GetType();
                            String field = FormatData(reader.GetFieldType(i), reader.GetValue(i));
                            row.Append(field);
                            if (i != (reader.FieldCount - 1))
                                row.Append(",");
                        }

                        totalRows++;
                        WriteRow(row.ToString());
                    }

                    Logger?.Invoke("ResultSet", "Total Records : " + totalRows);

                    totalSets++;

                } while (reader.NextResult());
            }

            _file?.Close();

            return _exitData.ToString();
        }

        public virtual void Parse(ParameterDirection direction, String name, Object value)
        {
            if (!String.IsNullOrEmpty(OutputFile))
                _file = new StreamWriter(OutputFile, true);

            if (direction != ParameterDirection.Input)
            {
                WriteRow("PARAMETER_DIRECTION, PARAMETER_NAME, PARAMETER_VALUE");
                WriteRow(direction + ",\"" + name + "\",\"" + value + "\"");
            }

            _file?.Close();
        }

        protected String FormatData(Type type, Object field)
        {
            String data = field.ToString();

            if (type == typeof(String))
                data = @"""" + field.ToString() + @"""";

            return data;
        }

        protected void WriteRow(String row)
        {
            Logger?.Invoke("ResultSet", row.ToString());
            if (_file != null)
                _file.WriteLine(row);
            else
                _exitData.AppendLine(row);

        }


    }
}
