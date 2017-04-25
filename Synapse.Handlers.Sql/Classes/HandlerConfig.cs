using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Xml.Serialization;
using System.Xml;

using Synapse.Core;

using YamlDotNet.Serialization;

using Synapse.Core.Utilities;

namespace Synapse.Handlers.Sql
{
    public class HandlerConfig
    {
        [XmlElement]
        public string ConnectionString { get; set; }
        [XmlElement]
        public OutputTypeType OutputType { get; set; }
        [XmlElement]
        public string OutputFile { get; set; }
        [XmlElement]
        public bool PrettyPrint { get; set; } = false;
    }

}
