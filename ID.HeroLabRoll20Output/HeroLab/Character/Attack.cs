using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using ID.HeroLabRoll20Output.HeroLab.Base;

namespace ID.HeroLabRoll20Output.HeroLab.Character
{
    public class Attack : HeroLabSpecialArray
    {
        [XmlAttribute("attackbonus")]
        public string AttackBonus { get; set; }

        [XmlAttribute("meleeattack")]
        public string MeleeAttack { get; set; }

        [XmlAttribute("rangedattack")]
        public string RangedAttack { get; set; }

        [XmlAttribute("baseattack")]
        public string BaseAttack { get; set; }
    }
}
