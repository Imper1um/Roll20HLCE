using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using ID.HeroLabRoll20Output.HeroLab.Base;

namespace ID.HeroLabRoll20Output.HeroLab.Character
{
    public class Weapon : HeroLabNamedProperty
    {

        [XmlAttribute("categorytext")]
        public string CategoryText { get; set; }

        [XmlAttribute("typetext")]
        public string TypeText { get; set; }

        [XmlAttribute("attack")]
        public string Attack { get; set; }
        
        [XmlAttribute("crit")]
        public string Crit { get; set; }

        [XmlAttribute("damage")]
        public string Damage { get; set; }

        [XmlAttribute("useradded")]
        public string UserAdded { get; set; }

        [XmlAttribute("quantity")]
        public string Quantity { get; set; }

        [XmlElement("rangedattack")]
        public RangedAttack RangedAttack { get; set; }


        [XmlElement("weight")]
        public HeroLabTextValue Weight { get; set; }

        [XmlElement("height")]
        public HeroLabTextValue Height { get; set; }

        [XmlElement("description")]
        public string Description { get; set; }

        [XmlElement("wepcategory", typeof(string))]
        public List<string> WeaponCategory { get; set; }

        [XmlElement("weptype", typeof(string))]
        public List<string> WeaponType { get; set; }

        [XmlElement("situationalmodifiers")]
        public SituationalModifiers SituationalModifiers { get; set; }
    }
}
