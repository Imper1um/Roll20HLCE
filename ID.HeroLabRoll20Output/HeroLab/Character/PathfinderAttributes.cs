using System;
using System.Xml.Serialization;

namespace ID.HeroLabRoll20Output.HeroLab.Character
{
    [Serializable]
    public class PathfinderAttributes
    {
        [XmlElement("attribute", typeof(PathfinderAttribute))]
        public PathfinderAttribute[] Items { get; set; }
    }
}