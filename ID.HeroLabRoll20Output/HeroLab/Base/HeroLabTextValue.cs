using System;
using System.Xml.Serialization;

namespace ID.HeroLabRoll20Output.HeroLab.Base
{
    [Serializable]
    public class HeroLabTextValue
    {
        [XmlAttribute("text")]
        public string Text { get; set; }
        
        [XmlAttribute("value")]
        public string Value { get; set; }
    }
}
