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
    class YamlDbParser : DbParser
    {
        private bool isFirstResultSet = true;
        private bool isFirstRow = true;

        public YamlDbParser() : base() { }
        public YamlDbParser(String outputFile) : base(outputFile) { }

        protected override String FormatFileOpen()
        {
            return "Results:" + Environment.NewLine;
        }

        protected override String FormatFileClose()
        {
            return null;
        }



        protected override String FormatParameterOpen(ParameterDirection direction, String name, Object value)
        {
            StringBuilder line = new StringBuilder();
            if (isFirstResultSet)
            {
                isFirstResultSet = false;
                line.AppendLine("  ResultSet:");
            }
            line.AppendLine("  - Row:");

            return line.ToString();
        }

        protected override String FormatParameter(ParameterDirection direction, String name, Object value)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("    - Name: " + name);
            sb.AppendLine("      Direction: " + direction);
            sb.AppendLine("      Type: " + value.GetType());
            sb.AppendLine("      Value: " + value);
            return sb.ToString();
        }

        protected override String FormatParameterClose(ParameterDirection direction, String name, Object value)
        {
            return null;
        }

        protected override String FormatResultSetOpen(DbDataReader reader)
        {
            isFirstRow = true;
            if (isFirstResultSet)
            {
                isFirstResultSet = false;
                return "  ResultSet:" + Environment.NewLine;
            }
            else
                return null;
        }

        protected override String FormatResultSetRow(DbDataReader reader)
        {
            StringBuilder row = new StringBuilder();

            if (isFirstRow)
            {
                row.AppendLine("  - Row:");
                isFirstRow = false;
            }
            for (int i = 0; i < reader.FieldCount; i++)
            {
                String field = reader.GetValue(i)?.ToString();
                String columnName = reader.GetName(i);
                if (i == 0)
                    row.AppendLine("    - " + columnName + ": " + field);
                else if (i != (reader.FieldCount - 1))
                    row.AppendLine("      " + columnName + ": " + field);
                else
                    row.AppendLine("      " + columnName + ": " + field);
            }

            return row.ToString();
        }

        protected override String FormatResultSetClose(DbDataReader reader)
        {
            return null;
        }
    }
}
