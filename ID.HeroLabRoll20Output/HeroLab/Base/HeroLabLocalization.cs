using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace ID.HeroLabRoll20Output.HeroLab.Base
{
    [Serializable]
    public class HeroLabLocalization
    {
        [XmlAttribute("language")]
        public string Language { get; set; }
        [XmlAttribute("units")]
        public string Units { get; set; }
    }
}
