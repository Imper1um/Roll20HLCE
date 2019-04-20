using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace ID.HeroLabRoll20Output.HeroLab.Character
{
    [Serializable]
    public class HeroPoints
    {
        [XmlAttribute("enabled")]
        public string Enabled { get; set; }

        [XmlAttribute("total")]
        public string Total { get; set; }
    }
}
