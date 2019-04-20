using System;
using System.Xml.Serialization;

namespace ID.HeroLabRoll20Output.HeroLab.Character
{
    [Serializable]
    public class Weapons
    {
        [XmlElement("weapon", typeof(Weapon))]
        public Weapon[] Items { get; set; }
    }
}