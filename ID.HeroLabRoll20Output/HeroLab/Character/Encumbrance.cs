using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace ID.HeroLabRoll20Output.HeroLab.Character
{
    [Serializable]
    public class Encumbrance
    {
        [XmlAttribute("carried")]
        public string Carried { get; set; }

        [XmlAttribute("encumstr")]
        public string Strength { get; set; }

        [XmlAttribute("light")]
        public string Light { get; set; }

        [XmlAttribute("medium")]
        public string Medium { get; set; }

        [XmlAttribute("heavy")]
        public string Heavy { get; set; }

        [XmlAttribute("level")]
        public string Level { get; set; }
    }
}
