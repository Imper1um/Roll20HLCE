using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using ID.HeroLabRoll20Output.HeroLab.Base;

namespace ID.HeroLabRoll20Output.HeroLab.Character
{
    public class Faction :HeroLabNamedProperty
    {
        [XmlAttribute("cpa")]
        public string AvailablePrestigePoints { get; set; }
        [XmlAttribute("tpa")]
        public string TotalPrestigePoints { get; set; }
    }
}
