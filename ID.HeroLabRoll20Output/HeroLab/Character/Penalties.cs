using System;
using System.Xml.Serialization;

namespace ID.HeroLabRoll20Output.HeroLab.Character
{
    [Serializable]
    public class Penalties
    {
        [XmlElement("penalty", typeof(Penalty))]
        public Penalty[] Items { get; set; }
    }
}