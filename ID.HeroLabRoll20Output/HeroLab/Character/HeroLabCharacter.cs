using System;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using ID.HeroLabRoll20Output.HeroLab.Base;

namespace ID.HeroLabRoll20Output.HeroLab.Character
{
    [Serializable]
    public class HeroLabCharacter
    {
        [XmlAttribute("characterindex")]
        public string CharacterIndex { get; set; }

        [XmlAttribute("nature")]
        public string Nature { get; set; }

        [XmlAttribute("role")]
        public string Role { get; set; }

        [XmlAttribute("relationship")]
        public string Relationship { get; set; }

        [XmlAttribute("type")]
        public string CharacterType { get; set; }

        [XmlAttribute("name")]
        public string Name { get; set; }

        [XmlAttribute("playername")]

        public string PlayerName { get; set; }
        [XmlElement("bookinfo")]
        public HeroLabNamedProperty BookInfo { get; set; }

        [XmlElement("race")]
        public Race Race { get; set; }
        [XmlElement("templates")]
        public Templates Templates { get; set; }
        
        [XmlElement("alignment")]
        public HeroLabNamedProperty Alignment { get; set; }

        [XmlElement("size")]
        public Size Size { get; set; }

        [XmlElement("diety")]
        public HeroLabNamedProperty Diety { get; set; }

        [XmlElement("challengerating")]
        public HeroLabTextValue ChallengeRating { get; set; }

        [XmlElement("xpaward")]
        public HeroLabTextValue XpAward { get; set; }

        [XmlElement("classes")]
        public PathfinderClasses Classes { get; set; }

        [XmlElement("factions")]
        public Factions Factions { get; set; }

        [XmlElement("types")]
        public RaceTypes RaceTypes { get; set; }

        [XmlElement("subtypes")]
        public RaceSubTypes SubTypes { get; set; }

        [XmlElement("heropoints")]
        public HeroPoints HeroPoints { get; set; }

        [XmlElement("senses")]
        public HeroLabSpecialArray Senses { get; set; }

        [XmlElement("favoredclasses")]
        public FavoredClasses FavoredClasses { get; set; }

        [XmlElement("health")]
        public Health Health { get; set; }

        [XmlElement("xp")]
        public Experience Experience { get; set; }

        [XmlElement("money")]
        public Money Money { get; set; }

        [XmlElement("personal")]
        public Personal Personal { get; set; }

        [XmlElement("languages")]
        public Languages Languages { get; set; }

        [XmlElement("attributes")]
        public PathfinderAttributes Attributes { get; set; }

        [XmlElement("saves")]
        public Saves Saves { get; set; }

        [XmlElement("defensive")]
        public HeroLabSpecialArray Defensive { get; set; }

        [XmlElement("damagereduction")]
        public HeroLabSpecialArray DamageReduction { get; set; }

        [XmlElement("immunities")]
        public HeroLabSpecialArray Immunities { get; set; }

        [XmlElement("resistances")]
        public HeroLabSpecialArray Resistances { get; set; }

        [XmlElement("weaknesses")]
        public HeroLabSpecialArray Weaknesses { get; set; }

        [XmlElement("armorclass")]
        public ArmorClass ArmorClass { get; set; }

        [XmlElement("penalties")]
        public Penalties Penalties { get; set; }

        [XmlElement("maneuvers")]
        public Maneuvers Maneuvers { get; set; }

        [XmlElement("initiative")]
        public Initiative Initiative { get; set; }

        [XmlElement("movement")]
        public Movement Movement { get; set; }

        [XmlElement("encumbrance")]
        public Encumbrance Encumbrance { get; set; }

        [XmlElement("skills")]
        public Skills Skills { get; set; }

        [XmlElement("skillabilities")]
        public HeroLabSpecialArray SkillAbilities { get; set; }

        [XmlElement("feats")]
        public Feats Feats { get; set; }

        [XmlElement("attack")]
        public Attack Attack { get; set; }

        [XmlElement("melee")]
        public Weapons Melee { get; set; }

        [XmlElement("ranged")]
        public Weapons Ranged { get; set; }

        [XmlElement("defenses")]
        public Armors Defenses { get; set; }

        [XmlElement("magicitems")]
        public PathfinderItems MagicItems { get; set; }

        [XmlElement("gear")]
        public PathfinderItems Gear { get; set; }

        [XmlElement("spelllike")]
        public HeroLabSpecialArray SpellLikeAbilities { get; set; }

        [XmlElement("trackedresources")]
        public TrackedResources TrackedResources { get; set; }

        [XmlElement("otherspecials")]
        public HeroLabSpecialArray OtherSpecials { get; set; }

        [XmlElement("spellsknown")]
        public Spells SpellsKnown { get; set; }

        [XmlElement("spellsmemorized")]
        public Spells SpellsMemorized { get; set; }

        [XmlElement("spellclasses")]
        public SpellClasses SpellClasses { get; set; }

        [XmlElement("journals")]
        public Journals Journals { get; set; }
    }

}
