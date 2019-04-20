using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace ID.HeroLabRoll20Output.HeroLab.Character
{
    [Serializable]
    public class Money
    {
        [XmlAttribute("total")]
        public string Total { get; set; }

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

        [XmlAttribute("valuables")]
        public string Valuables { get; set; }
    }
}
