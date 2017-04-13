using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Xml.Serialization;
using System.Xml;

using YamlDotNet.Serialization;

using Synapse.Core.Utilities;

namespace Synapse.Handlers.Sql
{
    public class HandlerParameters
    {
        [XmlElement]
        public String Query { get; set; }
        [XmlElement]
        public String StoredProcedure { get; set; }
        [XmlArrayItem(ElementName = "Parameter")]
        public List<ParameterType> Parameters { get; set; }
    }

    public class ParameterType
    {
        [XmlElement]
        public System.Data.ParameterDirection Direction { get; set; } = System.Data.ParameterDirection.Input;
        [XmlElement]
        public SqlParamterTypes Type { get; set; }
        [XmlElement]
        public int Size { get; set; }
        [XmlElement]
        public string Name { get; set; }
        [XmlElement]
        public string Value { get; set; }
    }


}
