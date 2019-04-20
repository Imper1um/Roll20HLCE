using System;
using System.Xml.Serialization;


namespace ID.HeroLabRoll20Output.HeroLab.Base
{
    [Serializable]
    public class HeroLabSpecialArray
    {
        [XmlElement("special", typeof(HeroLabSpecial))]
        public HeroLabSpecial[] Items { get; set; }
    }
}
