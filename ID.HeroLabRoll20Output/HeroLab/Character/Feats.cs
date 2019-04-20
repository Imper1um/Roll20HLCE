using System;
using System.Xml.Serialization;

namespace ID.HeroLabRoll20Output.HeroLab.Character
{
    [Serializable]
    public class Feats
    {
        [XmlElement("feat", typeof(Feat))]
        public Feat[] Items { get; set; }
    }
}