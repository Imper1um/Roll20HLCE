using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace ID.HeroLabRoll20Output.HeroLab.Base
{
    [Serializable]
    [XmlRoot(ElementName = "document")]
    public class HeroLabDocument
    {
        [XmlElement("public")]
        public HeroLabPublic Public { get; set; }
    }
}
