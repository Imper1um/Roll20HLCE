using System;
using System.Xml.Serialization;

namespace ID.HeroLabRoll20Output.HeroLab.Character
{
    [Serializable]
    public class Skills
    {
        [XmlElement("skill", typeof(Skill))]
        public Skill[] Items { get; set; }
    }
}