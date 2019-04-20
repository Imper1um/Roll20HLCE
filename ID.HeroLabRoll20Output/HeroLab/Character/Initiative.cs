using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using ID.HeroLabRoll20Output.HeroLab.Base;

namespace ID.HeroLabRoll20Output.HeroLab.Character
{
    [Serializable]
    public class Initiative
    {
        [XmlAttribute("total")]
        public string Total { get; set; }

        [XmlAttribute("attrtext")]
        public string AttributeText { get; set; }

        [XmlAttribute("misctext")]
        public string MiscellaneousText { get; set; }

        [XmlAttribute("attrname")]
        public string AttributeName { get; set; }

        [XmlElement("situationalmodifiers")]
        public SituationalModifiers SituationalModifiers { get; set; }
    }
}
