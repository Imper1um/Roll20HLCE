using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using ID.HeroLabRoll20Output.HeroLab.Base;

namespace ID.HeroLabRoll20Output.HeroLab.Character
{
    public class Save : HeroLabNamedProperty
    {
        [XmlAttribute("abbr")]
        public string Abbreviation { get; set; }

        [XmlAttribute("save")]
        public string Value { get; set; }

        [XmlAttribute("base")]
        public string Base { get; set; }

        [XmlAttribute("fromattr")]
        public string FromAttribute { get; set; }

        [XmlAttribute("fromresist")]
        public string FromResist { get; set; }

        [XmlAttribute("frommisc")]
        public string FromMisc { get; set; }

        [XmlElement("situationalmodifiers")]
        public SituationalModifiers SituationalModifiers { get; set; }
    }
}
