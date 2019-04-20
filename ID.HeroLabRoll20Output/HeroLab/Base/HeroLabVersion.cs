using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace ID.HeroLabRoll20Output.HeroLab.Base
{
    [Serializable]
    public class HeroLabVersion
    {
        [XmlAttribute("version")]
        public string Version { get; set; }
        [XmlAttribute("primary")]
        public string Primary { get; set; }
        [XmlAttribute("secondary")]
        public string Secondary { get; set; }
        [XmlAttribute("tertiary")]
        public string Tertiary { get; set; }
        [XmlAttribute("build")]
        public string Build { get; set; }
    }
}
