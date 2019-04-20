using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using ID.HeroLabRoll20Output.HeroLab.Base;

namespace ID.HeroLabRoll20Output.HeroLab.Character
{
    public class Journal : HeroLabNamedProperty
    {
        [XmlAttribute("gamedate")]
        public string GameDate { get; set; }
        
        [XmlAttribute("realdate")]
        public string RealDate { get; set; }

        [XmlAttribute("xp")]
        public string Experience { get; set; }

        [XmlAttribute("pp")]
        public string Platinum { get; set; }

        [XmlAttribute("gp")]
        public string Gold { get; set; }

        [XmlAttribute("sp")]
        public string Silver { get; set; }

        [XmlAttribute("cp")]
        public string Copper { get; set; }

        [XmlAttribute("cn1")]
        public string Custom1 { get; set; }

        [XmlAttribute("cn2")]
        public string Custom2 { get; set; }

        [XmlAttribute("cn3")]
        public string Custom3 { get; set; }

        [XmlAttribute("cn4")]
        public string Custom4 { get; set; }

        [XmlAttribute("prestigeaward")]
        public string PrestigeAward { get; set; }

        [XmlAttribute("prestigespend")]
        public string PrestigeSpend { get; set; }

        [XmlElement("description")]
        public string Description { get; set; }
    }
}
