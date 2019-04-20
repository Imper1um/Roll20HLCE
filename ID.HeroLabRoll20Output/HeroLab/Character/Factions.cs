using System;
using System.Xml.Serialization;

namespace ID.HeroLabRoll20Output.HeroLab.Character
{
    [Serializable]
    public class Factions
    {
        [XmlElement("faction", typeof(Faction))]
        public Faction[] Items { get; set; }
    }
}