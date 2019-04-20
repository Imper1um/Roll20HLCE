using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace ID.HeroLabRoll20Output.HeroLab.Character
{
    [Serializable]
    public class Health 
    {
        [XmlAttribute("hitdice")]
        public string HitDice { get; set; }
        [XmlAttribute("hitpoints")]
        public string HitPoints { get; set; }
        [XmlAttribute("damage")]
        public string Damage { get; set; }
        [XmlAttribute("nonlethal")]
        public string NonLethal { get; set; }
        [XmlAttribute("currenthp")]
        public string CurrentHp { get; set; }
    }
}
