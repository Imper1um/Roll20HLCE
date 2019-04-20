using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using ID.HeroLabRoll20Output.HeroLab.Base;

namespace ID.HeroLabRoll20Output.HeroLab.Character
{
    public class Skill : HeroLabNamedProperty
    {
        [XmlAttribute("ranks")]
        public string Ranks { get; set; }

        [XmlAttribute("attrbonus")]
        public string AttributeBonus { get; set; }

        [XmlAttribute("attrname")]
        public string AttributeName { get; set; }

        [XmlAttribute("value")]
        public string Value { get; set; }

        [XmlAttribute("armorcheck")]
        public string ArmorCheck { get; set; }

        [XmlAttribute("classskill")]
        public string ClassSkill { get; set; }

        [XmlElement("description")]
        public string Description { get; set; }

        [XmlElement("situationalmodifiers")]
        public SituationalModifiers SituationalModifiers { get; set; }
    }
}
