using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using ID.HeroLabRoll20Output.HeroLab.Base;

namespace ID.HeroLabRoll20Output.HeroLab.Character
{
    public class Spell : HeroLabNamedProperty
    {
        [XmlAttribute("level")]
        public string Level { get; set; }

        [XmlAttribute("class")]
        public string Class { get; set; }

        [XmlAttribute("casttime")]
        public string CastTime { get; set; }

        [XmlAttribute("range")]
        public string Range { get; set; }

        [XmlAttribute("target")]
        public string Target { get; set; }

        [XmlAttribute("area")]
        public string Area { get; set; }

        [XmlAttribute("effect")]
        public string Effect { get; set; }

        [XmlAttribute("duration")]
        public string Duration { get; set; }

        [XmlAttribute("save")]
        public string Save { get; set; }

        [XmlAttribute("resist")]
        public string Resist { get; set; }

        [XmlAttribute("dc")]
        public string DifficultyClass { get; set; }

        [XmlAttribute("casterlevel")]
        public string CasterLevel { get; set; }

        [XmlAttribute("componenttext")]
        public string ComponentText { get; set; }

        [XmlAttribute("schooltext")]
        public string SchoolText { get; set; }

        [XmlAttribute("subschooltext")]
        public string SubschoolText { get; set; }

        [XmlAttribute("descriptortext")]
        public string DescriptorText { get; set; }

        [XmlAttribute("savetext")]
        public string SaveText { get; set; }

        [XmlAttribute("resisttext")]
        public string ResistText { get; set; }

        [XmlAttribute("spontaneous")]
        public string Spontaneous { get; set; }

        [XmlAttribute("unlimited")]
        public string Unlimited { get; set; }

        [XmlElement("description")]
        public string Description { get; set; }

        [XmlElement("castsleft")]
        public string CastsLeft { get; set; }

        [XmlElement("spellcomp", typeof(string))]
        public List<string> SpellComponents { get; set; }

        [XmlElement("spellschool", typeof(string))]
        public List<string> SpellSchools { get; set; }

        [XmlElement("spellsubschool", typeof(string))]
        public List<string> SpellSubSchools { get; set; }

        [XmlElement("spelldescript", typeof(string))]
        public List<string> SpellDescriptors { get; set; }
    }
}
