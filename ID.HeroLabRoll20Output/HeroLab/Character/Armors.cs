using System;
using System.Xml.Serialization;

namespace ID.HeroLabRoll20Output.HeroLab.Character
{
    [Serializable]
    public class Armors
    {
        [XmlElement("armor", typeof(Armor))]
        public Armor[] Items { get; set; }
    }
}