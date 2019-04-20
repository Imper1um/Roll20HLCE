using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using ID.HeroLabRoll20Output.HeroLab.Base;

namespace ID.HeroLabRoll20Output.HeroLab.Character
{
    public class Feat : HeroLabNamedProperty
    {
        [XmlAttribute("categorytext")]
        public string CategoryText { get; set; }

        [XmlAttribute("useradded")]
        public string UserAdded { get; set; }

        [XmlElement("description")]
        public string Description { get; set; }

        [XmlElement("specsource")]
        public string SpecSource { get; set; }
    }
}
