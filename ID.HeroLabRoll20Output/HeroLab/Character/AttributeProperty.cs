using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using ID.HeroLabRoll20Output.HeroLab.Base;

namespace ID.HeroLabRoll20Output.HeroLab.Character
{
    public class AttributeProperty : HeroLabTextValue
    {
        [XmlAttribute("base")]
        public string Base { get; set; }
        [XmlAttribute("modified")]
        public string Modified { get; set; }
    }
}
