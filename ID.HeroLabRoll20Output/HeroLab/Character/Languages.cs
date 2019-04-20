using System;
using System.Xml.Serialization;

namespace ID.HeroLabRoll20Output.HeroLab.Character
{
    [Serializable]
    public class Languages
    {
        [XmlElement("language", typeof(Language))]
        public Language[] Items { get; set; }
    }
}