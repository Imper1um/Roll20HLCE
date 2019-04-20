using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using ID.HeroLabRoll20Output.HeroLab.Base;

namespace ID.HeroLabRoll20Output.HeroLab.Character
{
    public class Race : HeroLabNamedProperty
    {
        [XmlAttribute("racetext")]
        public string RaceText { get; set; }

        [XmlAttribute("ethnicity")]
        public string Ethnicity { get; set; }
    }
}
