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
            return "<Results>" + Environment.NewLine;
        }

        protected override String FormatFileClose()
        {
            return "</Results>";
        }



        protected override String FormatParameterOpen(ParameterDirection direction, String name, Object value)
        {
            StringBuilder line = new StringBuilder();
            line.AppendLine("    <ResultSet>");
            line.AppendLine("      <Row>");

            return line.ToString();
        }

        protected override String FormatParameter(ParameterDirection direction, String name, Object value)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("          <Name>" + name + "</Name>");
            sb.AppendLine("          <Direction>" + direction + "</Direction>");
            sb.AppendLine("          <Type>" + value?.GetType() + "</Type>");
            sb.AppendLine("          <Value>" + value + "</Value>");
            return sb.ToString();
        }

        protected override String FormatParameterClose(ParameterDirection direction, String name, Object value)
        {
            StringBuilder line = new StringBuilder();
            line.AppendLine("      </Row>");
            line.AppendLine("    </ResultSet>");

            return line.ToString();
        }

        protected override String FormatResultSetOpen(DbDataReader reader)
        {
            return "    <ResultSet>" + Environment.NewLine;
        }

        protected override String FormatResultSetRow(DbDataReader reader)
        {
            StringBuilder row = new StringBuilder();

            row.AppendLine("        <Row>");
            for (int i = 0; i < reader.FieldCount; i++)
            {
                String field = reader.GetValue(i)?.ToString();
                String columnName = reader.GetName(i);
                row.AppendLine("            <" + columnName + ">" + field + @"</" + columnName + ">");
            }
            row.AppendLine("        </Row>");

            return row.ToString();
        }

        protected override String FormatResultSetClose(DbDataReader reader)
        {
            return "    </ResultSet>" + Environment.NewLine;
        }
    }
}
