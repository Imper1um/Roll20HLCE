using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using ID.HeroLabRoll20Output.HeroLab.Base;

namespace ID.HeroLabRoll20Output.HeroLab.Character
{
    public class SpellClass : HeroLabNamedProperty
    {
        [XmlAttribute("maxspelllevel")]
        public string MaxSpellLevel { get; set; }

        [XmlAttribute("spells")]
        public string Spells { get; set; }

        [XmlElement("spelllevel", typeof(SpellLevel))]
        public SpellLevel[] Levels { get; set; }
    }
}
