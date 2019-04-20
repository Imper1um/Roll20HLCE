using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace ID.HeroLabRoll20Output.HeroLab.Character
{
    [Serializable]
    public class RangedAttack
    {
        [XmlAttribute("attack")]
        public string Attack { get; set; }

        [XmlAttribute("rangeinctext")]
        public string RangeIncrementText { get; set; }

        [XmlAttribute("rangeincvalue")]
        public string RangeIncrementValue { get; set; }
    }
}
