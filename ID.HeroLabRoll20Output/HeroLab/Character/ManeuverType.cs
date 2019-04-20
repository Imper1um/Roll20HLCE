using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using ID.HeroLabRoll20Output.HeroLab.Base;

namespace ID.HeroLabRoll20Output.HeroLab.Character
{
    public class ManeuverType : HeroLabNamedProperty
    {
        [XmlAttribute("bonus")]
        public string Bonus { get; set; }

        [XmlAttribute("cmb")]
        public string CombatManeuverBonus { get; set; }

        [XmlAttribute("cmd")]
        public string CombatManeuverDefense { get; set; }

        [XmlElement("situationalmodifiers")]
        public SituationalModifiers SituationalModifiers { get; set; }
    }
}
