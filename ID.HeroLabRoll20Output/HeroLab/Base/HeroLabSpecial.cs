using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace ID.HeroLabRoll20Output.HeroLab.Base
{
    public class HeroLabSpecial : HeroLabNamedProperty
    {
        [XmlAttribute("shortname")]
        public string ShortName { get; set; }
        [XmlAttribute("sourcetext")]
        public string SourceText { get; set; }
        [XmlElement("description")]
        public string Description { get; set; }
        [XmlElement("specsource")]
        public string SpecSource { get; set; }
        [XmlElement("type")]
        public string Type { get; set; }
    }
}
