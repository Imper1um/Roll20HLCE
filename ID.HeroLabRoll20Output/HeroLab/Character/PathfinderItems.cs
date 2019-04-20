using System;
using System.Xml.Serialization;

namespace ID.HeroLabRoll20Output.HeroLab.Character
{
    [Serializable]
    public class PathfinderItems
    {
        [XmlElement("item", typeof(Item))]
        public Item[] Items { get; set; }
    }
}