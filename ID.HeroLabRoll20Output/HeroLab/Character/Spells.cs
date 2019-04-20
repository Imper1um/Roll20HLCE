using System;
using System.Xml.Serialization;


namespace ID.HeroLabRoll20Output.HeroLab.Character
{
    [Serializable]
    public class Spells
    {
        [XmlElement("spell", typeof(Spell))]
        public Spell[] Items { get; set; }
    }
}
