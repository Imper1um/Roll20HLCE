using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using ID.HeroLabRoll20Output.HeroLab.Character;

namespace ID.HeroLabRoll20Output.HeroLab.Base
{
    [Serializable]
    public class HeroLabPublic
    {
        [XmlElement("program")]
        public HeroLabProgram Program { get; set; }
        [XmlElement("localization")]
        public HeroLabLocalization Localization { get; set; }
        [XmlElement("character", typeof(HeroLabCharacter))]
        public HeroLabCharacter[] Characters { get; set; }
    }
}
