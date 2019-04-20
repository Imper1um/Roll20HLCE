using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CsvHelper;

namespace ID.HeroLabRoll20Output.Spells
{
    public class SpellSource
    {
        public SpellSource(string name, string school, string subschool, string descriptor, string spellLevel,
            string castingTime, string components, string costlyComponents, string range, string area, string effect,
            string targets, string duration, string dismissible, string shapeable, string savingThrow,
            string spellResistence, string description, string descriptionFormated, string source, string fullText,
            string verbal, string somatic, string material, string focus, string divineFocus, string sor, string wiz,
            string cleric, string druid, string ranger, string bard, string paladin, string alchemist, string summoner,
            string witch, string inquisitor, string oracle, string antipaladin, string magus, string adept,
            string deity, string slaLevel, string domain, string shortDescription, string acid, string air,
            string chaotic, string cold, string curse, string darkness, string death, string disease, string earth,
            string electricity, string emotion, string evil, string fear, string fire, string force, string good,
            string languageDependent, string lawful, string light, string mindAffecting, string pain, string poison,
            string shadow, string sonic, string water, string linktext, string id, string materialCosts,
            string bloodline, string patron, string mythicText, string augmented, string mythic, string bloodrager,
            string shaman, string psychic, string medium, string mesmerist, string occultist, string spiritualist,
            string skald, string investigator, string hunter, string hauntStatistics, string ruse, string draconic,
            string meditative, string summonerUnchained)
        {
            Name = name;
            School = school;
            Subschool = subschool;
            Descriptor = descriptor;
            SpellLevel = spellLevel;
            CastingTime = castingTime;
            Components = components;
            CostlyComponents = costlyComponents;
            Range = range;
            Area = area;
            Effect = effect;
            Targets = targets;
            Duration = duration;
            Dismissible = dismissible;
            Shapeable = shapeable;
            SavingThrow = savingThrow;
            SpellResistence = spellResistence;
            Description = description;
            DescriptionFormated = descriptionFormated;
            Source = source;
            FullText = fullText;
            Verbal = verbal;
            Somatic = somatic;
            Material = material;
            Focus = focus;
            DivineFocus = divineFocus;
            Sor = sor;
            Wiz = wiz;
            Cleric = cleric;
            Druid = druid;
            Ranger = ranger;
            Bard = bard;
            Paladin = paladin;
            Alchemist = alchemist;
            Summoner = summoner;
            Witch = witch;
            Inquisitor = inquisitor;
            Oracle = oracle;
            Antipaladin = antipaladin;
            Magus = magus;
            Adept = adept;
            Deity = deity;
            SlaLevel = slaLevel;
            Domain = domain;
            ShortDescription = shortDescription;
            Acid = acid;
            Air = air;
            Chaotic = chaotic;
            Cold = cold;
            Curse = curse;
            Darkness = darkness;
            Death = death;
            Disease = disease;
            Earth = earth;
            Electricity = electricity;
            Emotion = emotion;
            Evil = evil;
            Fear = fear;
            Fire = fire;
            Force = force;
            Good = good;
            LanguageDependent = languageDependent;
            Lawful = lawful;
            Light = light;
            MindAffecting = mindAffecting;
            Pain = pain;
            Poison = poison;
            Shadow = shadow;
            Sonic = sonic;
            Water = water;
            Linktext = linktext;
            Id = id;
            MaterialCosts = materialCosts;
            Bloodline = bloodline;
            Patron = patron;
            MythicText = mythicText;
            Augmented = augmented;
            Mythic = mythic;
            Bloodrager = bloodrager;
            Shaman = shaman;
            Psychic = psychic;
            Medium = medium;
            Mesmerist = mesmerist;
            Occultist = occultist;
            Spiritualist = spiritualist;
            Skald = skald;
            Investigator = investigator;
            Hunter = hunter;
            HauntStatistics = hauntStatistics;
            Ruse = ruse;
            Draconic = draconic;
            Meditative = meditative;
            SummonerUnchained = summonerUnchained;
        }

        public SpellSource(CsvReader csvReader) : this(csvReader.GetField<string>("name"),
            csvReader.GetField<string>("school"), csvReader.GetField<string>("subschool"),
            csvReader.GetField<string>("descriptor"), csvReader.GetField<string>("spell_level"),
            csvReader.GetField<string>("casting_time"), csvReader.GetField<string>("components"),
            csvReader.GetField<string>("costly_components"), csvReader.GetField<string>("range"),
            csvReader.GetField<string>("area"), csvReader.GetField<string>("effect"),
            csvReader.GetField<string>("targets"), csvReader.GetField<string>("duration"),
            csvReader.GetField<string>("dismissible"), csvReader.GetField<string>("shapeable"),
            csvReader.GetField<string>("saving_throw"), csvReader.GetField<string>("spell_resistence"),
            csvReader.GetField<string>("description"), csvReader.GetField<string>("description_formated"),
            csvReader.GetField<string>("source"), csvReader.GetField<string>("full_text"),
            csvReader.GetField<string>("verbal"), csvReader.GetField<string>("somatic"),
            csvReader.GetField<string>("material"), csvReader.GetField<string>("focus"),
            csvReader.GetField<string>("divine_focus"), csvReader.GetField<string>("sor"),
            csvReader.GetField<string>("wiz"), csvReader.GetField<string>("cleric"),
            csvReader.GetField<string>("druid"), csvReader.GetField<string>("ranger"),
            csvReader.GetField<string>("bard"), csvReader.GetField<string>("paladin"),
            csvReader.GetField<string>("alchemist"), csvReader.GetField<string>("summoner"),
            csvReader.GetField<string>("witch"), csvReader.GetField<string>("inquisitor"),
            csvReader.GetField<string>("oracle"), csvReader.GetField<string>("antipaladin"),
            csvReader.GetField<string>("magus"), csvReader.GetField<string>("adept"),
            csvReader.GetField<string>("deity"), csvReader.GetField<string>("SLA_Level"),
            csvReader.GetField<string>("domain"), csvReader.GetField<string>("short_description"),
            csvReader.GetField<string>("acid"), csvReader.GetField<string>("air"),
            csvReader.GetField<string>("chaotic"), csvReader.GetField<string>("cold"),
            csvReader.GetField<string>("curse"), csvReader.GetField<string>("darkness"),
            csvReader.GetField<string>("death"), csvReader.GetField<string>("disease"),
            csvReader.GetField<string>("earth"), csvReader.GetField<string>("electricity"),
            csvReader.GetField<string>("emotion"), csvReader.GetField<string>("evil"),
            csvReader.GetField<string>("fear"), csvReader.GetField<string>("fire"), csvReader.GetField<string>("force"),
            csvReader.GetField<string>("good"), csvReader.GetField<string>("language_dependent"),
            csvReader.GetField<string>("lawful"), csvReader.GetField<string>("light"),
            csvReader.GetField<string>("mind_affecting"), csvReader.GetField<string>("pain"),
            csvReader.GetField<string>("poison"), csvReader.GetField<string>("shadow"),
            csvReader.GetField<string>("sonic"), csvReader.GetField<string>("water"),
            csvReader.GetField<string>("linktext"), csvReader.GetField<string>("id"),
            csvReader.GetField<string>("material_costs"), csvReader.GetField<string>("bloodline"),
            csvReader.GetField<string>("patron"), csvReader.GetField<string>("mythic_text"),
            csvReader.GetField<string>("augmented"), csvReader.GetField<string>("mythic"),
            csvReader.GetField<string>("bloodrager"), csvReader.GetField<string>("shaman"),
            csvReader.GetField<string>("psychic"), csvReader.GetField<string>("medium"),
            csvReader.GetField<string>("mesmerist"), csvReader.GetField<string>("occultist"),
            csvReader.GetField<string>("spiritualist"), csvReader.GetField<string>("skald"),
            csvReader.GetField<string>("investigator"), csvReader.GetField<string>("hunter"),
            csvReader.GetField<string>("haunt_statistics"), csvReader.GetField<string>("ruse"),
            csvReader.GetField<string>("draconic"), csvReader.GetField<string>("meditative"),
            csvReader.GetField<string>("summoner_unchained"))
        {

        }

        public string Name { get; }
        public string School { get; }
        public string Subschool { get; }
        public string Descriptor { get; }
        public string SpellLevel { get; }
        public string CastingTime { get; }
        public string Components { get; }
        public string CostlyComponents { get; }
        public string Range { get; }
        public string Area { get; }
        public string Effect { get; }
        public string Targets { get; }
        public string Duration { get; }
        public string Dismissible { get; }
        public string Shapeable { get; }
        public string SavingThrow { get; }
        public string SpellResistence { get; }
        public string Description { get; }
        public string DescriptionFormated { get; }
        public string Source { get; }
        public string FullText { get; }
        public string Verbal { get; }
        public string Somatic { get; }
        public string Material { get; }
        public string Focus { get; }
        public string DivineFocus { get; }
        public string Sor { get; }
        public string Wiz { get; }
        public string Cleric { get; }
        public string Druid { get; }
        public string Ranger { get; }
        public string Bard { get; }
        public string Paladin { get; }
        public string Alchemist { get; }
        public string Summoner { get; }
        public string Witch { get; }
        public string Inquisitor { get; }
        public string Oracle { get; }
        public string Antipaladin { get; }
        public string Magus { get; }
        public string Adept { get; }
        public string Deity { get; }
        public string SlaLevel { get; }
        public string Domain { get; }
        public string ShortDescription { get; }
        public string Acid { get; }
        public string Air { get; }
        public string Chaotic { get; }
        public string Cold { get; }
        public string Curse { get; }
        public string Darkness { get; }
        public string Death { get; }
        public string Disease { get; }
        public string Earth { get; }
        public string Electricity { get; }
        public string Emotion { get; }
        public string Evil { get; }
        public string Fear { get; }
        public string Fire { get; }
        public string Force { get; }
        public string Good { get; }
        public string LanguageDependent { get; }
        public string Lawful { get; }
        public string Light { get; }
        public string MindAffecting { get; }
        public string Pain { get; }
        public string Poison { get; }
        public string Shadow { get; }
        public string Sonic { get; }
        public string Water { get; }
        public string Linktext { get; }
        public string Id { get; }
        public string MaterialCosts { get; }
        public string Bloodline { get; }
        public string Patron { get; }
        public string MythicText { get; }
        public string Augmented { get; }
        public string Mythic { get; }
        public string Bloodrager { get; }
        public string Shaman { get; }
        public string Psychic { get; }
        public string Medium { get; }
        public string Mesmerist { get; }
        public string Occultist { get; }
        public string Spiritualist { get; }
        public string Skald { get; }
        public string Investigator { get; }
        public string Hunter { get; }
        public string HauntStatistics { get; }
        public string Ruse { get; }
        public string Draconic { get; }
        public string Meditative { get; }
        public string SummonerUnchained { get; }

    }
}
