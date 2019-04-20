using System;
using System.Xml.Serialization;

namespace ID.HeroLabRoll20Output.HeroLab.Character
{
    [Serializable]
    public class RaceSubTypes
    {
        [XmlElement("subtype", typeof(RaceType))]
        public RaceType[] Items { get; set; }
    }
}