using System;
using System.Xml.Serialization;

namespace ID.HeroLabRoll20Output.HeroLab.Character
{
    [Serializable]
    public class Journals
    {
        [XmlElement("journal", typeof(Journal))]
        public Journal[] Items { get; set; }
    }
}