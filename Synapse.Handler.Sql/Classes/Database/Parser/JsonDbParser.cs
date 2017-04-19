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
    class JsonDbParser : DbParser
    {
        private bool isFirstResultSet = true;
        private bool isFirstRow = true;

        public JsonDbParser() : base() { }
        public JsonDbParser(String outputFile) : base(outputFile) { }

        protected override String FormatFileOpen()
        {
            StringBuilder line = new StringBuilder();
            line.AppendLine("{");
            line.AppendLine("  \"Results\": {");
            return line.ToString();
        }

        protected override String FormatFileClose()
        {
            StringBuilder line = new StringBuilder();
            line.AppendLine();
            if (!isFirstResultSet)
                line.AppendLine("    ]");
            line.AppendLine("  }");
            line.AppendLine("}");
            return line.ToString();
        }



        protected override String FormatParameterOpen(ParameterDirection direction, String name, Object value)
        {
            StringBuilder line = new StringBuilder();
            isFirstRow = true;
            if (isFirstResultSet)
            {
                isFirstResultSet = false;
                line.AppendLine("    \"ResultSet\": [");
            }
            else
                line.AppendLine(",");

            line.AppendLine("      {");

            return line.ToString();
        }

        protected override String FormatParameter(ParameterDirection direction, String name, Object value)
        {
            StringBuilder row = new StringBuilder();

            if (isFirstRow)
            {
                row.AppendLine("        \"Row\": [");
                isFirstRow = false;
            }
            else
                row.AppendLine(",");

            row.AppendLine("          {");

            row.AppendLine("            \"Name\": \"" + name + "\",");
            row.AppendLine("            \"Direction\": \"" + direction + "\",");
            row.AppendLine("            \"Type\": \"" + value?.GetType() + "\",");
            row.AppendLine("            \"Value\": \"" + value + "\"");

            row.AppendLine("          }");
            row.Append("        ]");
            return row.ToString();
        }

        protected override String FormatParameterClose(ParameterDirection direction, String name, Object value)
        {
            return Environment.NewLine + "      }";
        }

        protected override String FormatResultSetOpen(DbDataReader reader)
        {
            StringBuilder line = new StringBuilder();
            isFirstRow = true;
            if (isFirstResultSet)
            {
                isFirstResultSet = false;
                line.AppendLine("    \"ResultSet\": [");
            }
            else
                line.AppendLine(",");

            line.AppendLine("      {");

            return line.ToString();
        }

        protected override String FormatResultSetRow(DbDataReader reader)
        {
            StringBuilder row = new StringBuilder();

            if (isFirstRow)
            {
                row.AppendLine("        \"Row\": [");
                isFirstRow = false;
            }
            else
                row.AppendLine(",");

            row.AppendLine("          {");

            for (int i = 0; i < reader.FieldCount; i++)
            {
                String field = reader.GetValue(i)?.ToString();
                String columnName = reader.GetName(i);
                row.Append("            \"" + columnName + "\": \"" + field + "\"");
                if (i == (reader.FieldCount - 1))
                {
                    row.AppendLine();
                }
                else
                {
                    row.AppendLine(",");
                }
            }

            row.Append("          }");
            return row.ToString();
        }

        protected override String FormatResultSetClose(DbDataReader reader)
        {
            StringBuilder row = new StringBuilder();
            row.AppendLine();
            row.AppendLine("        ]");
            row.Append("      }");
            return row.ToString();
        }
    }
}
