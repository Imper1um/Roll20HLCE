using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using ID.HeroLabRoll20Output.HeroLab.Base;

namespace ID.HeroLabRoll20Output.HeroLab.Character
{
    [Serializable]
    public class Personal
    {
        [XmlAttribute("gender")]
        public string Gender { get; set; }

        [XmlAttribute("age")]
        public string Age { get; set; }
        
        [XmlAttribute("hair")]
        public string Hair { get; set; }

        [XmlAttribute("eyes")]
        public string Eyes { get; set; }

        [XmlAttribute("skin")]
        public string Skin { get; set; }

        [XmlElement("description")]
        public string Description { get; set; }

        [XmlElement("charheight")]
        public HeroLabTextValue Height { get; set; }

        [XmlElement("charweight")]
        public HeroLabTextValue Weight { get; set; }
    }
}
