using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace ID.HeroLabRoll20Output.HeroLab.Character
{
    [Serializable]
    public class Maneuvers
    {
        [XmlAttribute("cmb")]
        public string CombatManeuverBonus { get; set; }

        [XmlAttribute("cmd")]
        public string CombatManeuverDefense { get; set; }

        [XmlAttribute("cmdflatfooted")]
        public string CombatManeuverDefenseFlatFooted { get; set; }

        [XmlElement("maneuvertype", typeof(ManeuverType))]
        public ManeuverType[] Items { get; set; }
    }
}
