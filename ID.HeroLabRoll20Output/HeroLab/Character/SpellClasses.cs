using System;
using System.Xml.Serialization;

namespace ID.HeroLabRoll20Output.HeroLab.Character
{
    [Serializable]
    public class SpellClasses
    {
        [XmlElement("spellclass", typeof(SpellClass))]
        public SpellClass[] Items { get; set; }
    }
}