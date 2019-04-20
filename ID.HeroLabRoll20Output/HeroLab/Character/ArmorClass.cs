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
    public class ArmorClass
    {
        [XmlAttribute("ac")]
        public string TotalArmorClass { get; set; }

        [XmlAttribute("touch")]
        public string TouchArmorClass { get; set; }

        [XmlAttribute("flatfooted")]
        public string FlatFootedArmorClass { get; set; }

        [XmlAttribute("fromarmor")]
        public string FromArmorClass { get; set; }

        [XmlAttribute("fromshield")]
        public string FromShield { get; set; }

        [XmlAttribute("fromdexterity")]
        public string FromDexterity { get; set; }

        [XmlAttribute("fromwisdom")]
        public string FromWisdom { get; set; }

        [XmlAttribute("fromcharisma")]
        public string FromCharisma { get; set; }

        [XmlAttribute("fromsize")]
        public string FromSize { get; set; }

        [XmlAttribute("fromnatural")]
        public string FromNatural { get; set; }

        [XmlAttribute("fromdeflect")]
        public string FromDeflect { get; set; }

        [XmlAttribute("fromdodge")]
        public string FromDodge { get; set; }
        [XmlAttribute("frommisc")]
        public string FromMisc { get; set; }

        [XmlElement("situationalmodifiers")]
        public SituationalModifiers SituationalModifiers { get; set; }
    }
}
