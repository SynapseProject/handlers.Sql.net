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
            return "<Results>";
        }

        protected override String FormatFileClose()
        {
            return "</Results>";
        }



        protected override String FormatParameterOpen(ParameterDirection direction, String name, Object value)
        {
            return @"    <Parameter>";
        }

        protected override String FormatParameter(ParameterDirection direction, String name, Object value)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("        <Direction>" + direction + "</Direction>");
            sb.AppendLine("        <Name>" + name + "</Name>");
            sb.Append("        <Value>" + value + "</Value>");
            return sb.ToString();
        }

        protected override String FormatParameterClose(ParameterDirection direction, String name, Object value)
        {
            return @"    </Parameter>";
        }

        protected override String FormatResultSetOpen(DbDataReader reader)
        {
            return "    <ResultSet>";
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
            row.Append("        </Row>");

            return row.ToString();
        }

        protected override String FormatResultSetClose(DbDataReader reader)
        {
            return "    </ResultSet>";
        }
    }
}
