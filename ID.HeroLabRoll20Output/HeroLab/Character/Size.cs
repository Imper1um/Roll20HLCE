using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using ID.HeroLabRoll20Output.HeroLab.Base;

namespace ID.HeroLabRoll20Output.HeroLab.Character
{
    public class Size : HeroLabNamedProperty
    {
        [XmlElement("space")]
        public HeroLabTextValue Space { get; set; }
        [XmlElement("reach")]
        public HeroLabTextValue Reach { get; set; }
    }
}
