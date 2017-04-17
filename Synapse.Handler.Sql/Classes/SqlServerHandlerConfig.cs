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
    public class SqlServerHandlerConfig : HandlerConfig
    {
        [XmlElement]
        public bool IntegratedSecurity { get; set; }
        [XmlElement]
        public bool TrustedConnection { get; set; }
        [XmlElement]
        public string Database { get; set; }
        [XmlElement]
        public int ConnectionTimeout { get; set; }
    }

}
