using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using ID.HeroLabRoll20Output.HeroLab.Base;

namespace ID.HeroLabRoll20Output.HeroLab.Character
{
    public class PathfinderAttribute : HeroLabNamedProperty
    {
        [XmlElement("attrvalue")]
        public AttributeProperty Value { get; set; }
        [XmlElement("attrbonus")]
        public AttributeProperty Bonus { get; set; }
        [XmlElement("situationalmodifiers")]
        public SituationalModifiers SituationalModifiers { get; set; }
    }
}
