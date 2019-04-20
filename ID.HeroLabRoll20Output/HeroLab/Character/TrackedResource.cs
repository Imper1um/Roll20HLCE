using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using ID.HeroLabRoll20Output.HeroLab.Base;

namespace ID.HeroLabRoll20Output.HeroLab.Character
{
    public class TrackedResource : HeroLabNamedProperty
    {
        [XmlAttribute("text")]
        public string Text { get; set; }

        [XmlAttribute("used")]
        public string Used { get; set; }

        [XmlAttribute("left")]
        public string Left { get; set; }

        [XmlAttribute("min")]
        public string Min { get; set; }

        [XmlAttribute("max")]
        public string Max { get; set; }
    }
}
