using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using ID.HeroLabRoll20Output.HeroLab.Base;

namespace ID.HeroLabRoll20Output.HeroLab.Character
{
    public class PathfinderClass : HeroLabNamedProperty
    {
        [XmlAttribute("level")]
        public string Level { get; set; }
        [XmlAttribute("spells")]
        public string Spells { get; set; }
        [XmlAttribute("casterlevel")]
        public string CasterLevel { get; set; }
        [XmlAttribute("concentrationcheck")]
        public string ConcentrationCheck { get; set; }
        [XmlAttribute("overcomespellresistance")]
        public string OvercomeSpellResistance { get; set; }
        [XmlAttribute("basespelldc")]
        public string BaseSpellDifficultyClass { get; set; }
        [XmlAttribute("castersource")]
        public string CasterSource { get; set; }
    }
}
