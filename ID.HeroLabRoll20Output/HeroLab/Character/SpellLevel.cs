using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace ID.HeroLabRoll20Output.HeroLab.Character
{
    [Serializable]
    public class SpellLevel
    {
        [XmlAttribute("level")]
        public string Level { get; set; }

        [XmlAttribute("maxcasts")]
        public string MaxCasts { get; set; }

        [XmlAttribute("used")]
        public string Used { get; set; }
    }
}
