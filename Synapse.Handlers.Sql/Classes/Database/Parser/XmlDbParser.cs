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
    class XmlDbParser : DbParser
    {
        public XmlDbParser() : base() { }
        public XmlDbParser(String outputFile) : base(outputFile) { }

        protected override String FormatFileOpen()
        {
            if (PrettyPrint)
                return "<Results>" + Environment.NewLine;
            else
                return "<Results>";
        }

        protected override String FormatFileClose()
        {
            return "</Results>";
        }



        protected override String FormatParameterOpen(ParameterDirection direction, String name, Object value)
        {
            StringBuilder line = new StringBuilder();
            if (PrettyPrint)
            {
                line.AppendLine("    <ResultSet>");
                line.AppendLine("      <Row>");
            }
            else
            {
                line.Append("<ResultSet>");
                line.Append("<Row>");
            }

            return line.ToString();
        }

        protected override String FormatParameter(ParameterDirection direction, String name, Object value)
        {
            StringBuilder sb = new StringBuilder();
            if (PrettyPrint)
            {
                sb.AppendLine("          <Name>" + name + "</Name>");
                sb.AppendLine("          <Direction>" + direction + "</Direction>");
                sb.AppendLine("          <Type>" + value?.GetType() + "</Type>");
                sb.AppendLine("          <Value>" + value + "</Value>");
            }
            else
            {
                sb.Append("<Name>" + name + "</Name>");
                sb.Append("<Direction>" + direction + "</Direction>");
                sb.Append("<Type>" + value?.GetType() + "</Type>");
                sb.Append("<Value>" + value + "</Value>");
            }

            return sb.ToString();
        }

        protected override String FormatParameterClose(ParameterDirection direction, String name, Object value)
        {
            StringBuilder line = new StringBuilder();
            if (PrettyPrint)
            {
                line.AppendLine("      </Row>");
                line.AppendLine("    </ResultSet>");
            }
            else
            {
                line.Append("</Row>");
                line.Append("</ResultSet>");
            }

            return line.ToString();
        }

        protected override String FormatResultSetOpen(DbDataReader reader)
        {
            if (PrettyPrint)
                return "    <ResultSet>" + Environment.NewLine;
            else
                return "<ResultSet>";

        }

        protected override String FormatResultSetRow(DbDataReader reader)
        {
            StringBuilder row = new StringBuilder();

            if (PrettyPrint)
                row.AppendLine("        <Row>");
            else
                row.Append("<Row>");
            for (int i = 0; i < reader.FieldCount; i++)
            {
                String field = reader.GetValue(i)?.ToString();
                String columnName = reader.GetName(i);
                if (PrettyPrint)
                    row.AppendLine("            <" + columnName + ">" + field + @"</" + columnName + ">");
                else
                    row.Append("<" + columnName + ">" + field + @"</" + columnName + ">");
            }

            if (PrettyPrint)
                row.AppendLine("        </Row>");
            else
                row.Append("</Row>");
            return row.ToString();
        }

        protected override String FormatResultSetClose(DbDataReader reader)
        {
            if (PrettyPrint)
                return "    </ResultSet>" + Environment.NewLine;
            else
                return "</ResultSet>";
        }
    }
}
