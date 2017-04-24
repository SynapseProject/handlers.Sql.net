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
    public class DbParser
    {
        public Action<string, string> Logger { get; set; }
        public String OutputFile { get; set; }
        public String ExitData {  get { return _exitData?.ToString(); } }
        public bool PrettyPrint { get; set; } = false;

        protected StreamWriter _file;
        protected StringBuilder _exitData = new StringBuilder();

        public DbParser() { }

        public DbParser(String outputFile)
        {
            OutputFile = outputFile;
            if (File.Exists(OutputFile))
                File.Delete(OutputFile);
        }

        public void Open()
        {
            if (!String.IsNullOrEmpty(OutputFile))
                _file = new StreamWriter(OutputFile, true);
            String str = FormatFileOpen();
            WriteLine(str);
        }

        public void Close()
        {
            String str = FormatFileClose();
            WriteLine(str);
            _file?.Close();
        }

        public String Parse(DbDataReader reader)
        {
            if (reader == null)
                return String.Empty;

            if (reader.HasRows)
            {
                int totalSets = 0;
                do
                {
                    WriteLine(FormatResultSetOpen(reader));
                    int totalRows = 0;
                    while (reader.Read())
                    {
                        WriteLine(FormatResultSetRow(reader));
                        totalRows++;
                    }
                    WriteLine(FormatResultSetClose(reader));

                    Logger?.Invoke("Summary", "Total Records : " + totalRows);

                    totalSets++;

                } while (reader.NextResult());
            }

            return _exitData.ToString();
        }

        public void Parse(ParameterDirection direction, String name, Object value)
        {
            if (direction != ParameterDirection.Input)
            {
                WriteLine(FormatParameterOpen(direction, name, value));
                WriteLine(FormatParameter(direction, name, value));
                WriteLine(FormatParameterClose(direction, name, value));
            }
        }

        protected virtual String FormatParameterOpen(ParameterDirection direction, String name, Object value)
        {
            return "\"Name\",\"Direction\",\"Type\",\"Value\"" + Environment.NewLine;
        }

        protected virtual String FormatParameter(ParameterDirection direction, String name, Object value)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("\"" + name + "\",\"" + direction + "\",\"" + value?.GetType() + "\",\"" + value + "\"");
            return sb.ToString();
        }

        protected virtual String FormatParameterClose(ParameterDirection direction, String name, Object value)
        {
            return null;
        }

        protected virtual String FormatResultSetOpen(DbDataReader reader)
        {
            StringBuilder row = new StringBuilder();

            for (int i = 0; i < reader.FieldCount; i++)
            {
                row.Append(FormatData(typeof(String), reader.GetName(i)));
                if (i != (reader.FieldCount - 1))
                    row.Append(",");
            }
            row.AppendLine();

            return row.ToString();
        }

        protected virtual String FormatResultSetRow(DbDataReader reader)
        {
            StringBuilder row = new StringBuilder();

            for (int i = 0; i < reader.FieldCount; i++)
            {
                Type type = reader[i].GetType();
                String field = FormatData(reader.GetFieldType(i), reader.GetValue(i));
                row.Append(field);
                if (i != (reader.FieldCount - 1))
                    row.Append(",");
            }
            row.AppendLine();

            return row.ToString();
        }

        protected virtual String FormatResultSetClose(DbDataReader reader)
        {
            return null;
        }

        protected virtual String FormatFileOpen()
        {
            return null;
        }

        protected virtual String FormatFileClose()
        {
            return null;
        }

        protected String FormatData(Type type, Object field)
        {
            String data = field.ToString();

            if (data.Contains(" ") || data.Contains(",") || type == typeof(String))
                data = @"""" + field.ToString() + @"""";

            return data;
        }

        protected void WriteLine(String row)
        {
            if (row != null)
            {
                String[] lines = row.Split(new string[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries);
                foreach (String line in lines)
                    Logger?.Invoke("ResultSet", line);
                if (_file != null)
                    _file.Write(row);
                else
                    _exitData.Append(row);
            }

        }


    }
}
