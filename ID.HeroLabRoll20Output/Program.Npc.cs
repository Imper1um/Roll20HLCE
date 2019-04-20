using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using ID.HeroLabRoll20Output.HeroLab.Base;
using ID.HeroLabRoll20Output.HeroLab.Character;
using ID.HeroLabRoll20Output.Spells;

namespace ID.HeroLabRoll20Output
{
    public partial class Program
    {
        private static void ProcessNpcCharacter(HeroLabCharacter character)
        {

            using (var file = File.OpenWrite($"C:\\PathfinderExporter\\{character.Name}_NPC.txt"))
            using (var fileWriter = new StreamWriter(file))
            {
                fileWriter.Write("!HeroLabImporter --mode clear --set npc 1");
                fileWriter.Write($" --name {character.Name}");

                WriteNpcMain(fileWriter, character);
                WriteNpcDefense(fileWriter, character);
                WriteNpcOffense(fileWriter, character);
                WriteNpcStatistics(fileWriter, character);
                fileWriter.WriteLine();

                //Melee Weapons
                if (character.Melee?.Items?.Any() == true)
                    foreach (var melee in character.Melee.Items)
                        WriteNpcMeleeWeapon(fileWriter, "melee", character, melee);

                //Ranged Weapons
                if (character.Ranged?.Items?.Any() == true)
                    foreach (var ranged in character.Ranged.Items)
                        WriteNpcMeleeWeapon(fileWriter, "ranged", character, ranged);

                //Special Attacks
                if (character.Attack?.Items?.Any() == true)
                    foreach (var sa in character.Attack.Items)
                        WriteNpcSpecialAttack(fileWriter, character, sa);

                //Spell-Like Abilities
                if (character.SpellLikeAbilities?.Items?.Any() == true)
                    foreach (var spellLikeAbility in character.SpellLikeAbilities.Items)
                        WriteNpcSpellLikeAbility(fileWriter, character, spellLikeAbility);

                //Spells
                if (character.SpellsMemorized?.Items?.Any() == true)
                    foreach (var spell in character.SpellsMemorized.Items)
                        WriteNpcSpell(fileWriter, character, spell);

                //Feats
                if (character.Feats?.Items?.Any() == true)
                    foreach (var feat in character.Feats.Items)
                        WriteFeat(fileWriter, character, feat);

                //Special Abilities
                if (character.OtherSpecials?.Items?.Any() == true)
                    foreach (var specialAbilities in character.OtherSpecials.Items)
                        WriteSpecialAbility(fileWriter, character, specialAbilities);
            }
        }

        private static void WriteSpecialAbility(StreamWriter fileWriter, HeroLabCharacter character, HeroLabSpecial specialAbilities)
        {
            fileWriter.Write($"!HeroLabImporter --name {character.Name} --mode add --addtype abilities");
            fileWriter.Write($" --set name {specialAbilities.Name}");
            fileWriter.Write($" --set type {GetSpecialAbilityType(specialAbilities.Type)}");
            fileWriter.Write($" --set descflag 1");
            fileWriter.Write($" --set description {specialAbilities.Description.Replace('\r', ' ').Replace('\n', ' ')}");
            fileWriter.WriteLine();
        }

        private static object GetSpecialAbilityType(string type)
        {
            switch (type)
            {
                case "Spell-Like Ability": return "Sp";
                case "Supernatural Ability": return "Su";
                case "Extraordinary Ability": return "Ex";
            }
            return type;
        }

        private static void WriteFeat(StreamWriter fileWriter, HeroLabCharacter character, Feat feat)
        {
            fileWriter.Write($"!HeroLabImporter --name {character.Name} --mode add --addtype feats");
            
            fileWriter.Write($" --set name {feat.Name}"); //name
            fileWriter.Write($" --set type {(string.IsNullOrEmpty(feat.CategoryText) ? "general" : feat.CategoryText.ToLower())}"); //type

            var description = feat.Description;
            var prerequisites = string.Empty;
            var benefits = string.Empty;
            var normal = string.Empty;
            var special = string.Empty;
            const string prerequisiteRegex = "Prerequisites?:.+";
            const string benefitRegex = "Benefits?:.+";
            const string normalRegex = "Normal:.+";
            const string specialRegex = "Special:.+";
            if (Regex.IsMatch(description, prerequisiteRegex, RegexOptions.IgnoreCase))
            {
                prerequisites = Regex.Match(description, prerequisiteRegex, RegexOptions.IgnoreCase).Value;
                description = description.Replace(prerequisites, "");
                prerequisites = prerequisites.Split(':')[1];
            }
            if (Regex.IsMatch(description, benefitRegex, RegexOptions.IgnoreCase))
            {
                benefits = Regex.Match(description, benefitRegex, RegexOptions.IgnoreCase).Value;
                description = description.Replace(benefits, "");
                benefits = benefits.Split(':')[1];
            }
            if (Regex.IsMatch(description, normalRegex, RegexOptions.IgnoreCase))
            {
                normal = Regex.Match(description, normalRegex, RegexOptions.IgnoreCase).Value;
                description = description.Replace(normal, "");
                normal = normal.Split(':')[1];
            }
            if (Regex.IsMatch(description, specialRegex, RegexOptions.IgnoreCase))
            {
                special = Regex.Match(description, specialRegex, RegexOptions.IgnoreCase).Value;
                description = description.Replace(special, "");
                special = special.Split(':')[1];
            }
            fileWriter.Write($" --set prerequisites {OneLineString(prerequisites)}"); //prerequisites
            fileWriter.Write($" --set benefits {OneLineString(benefits)}"); //benefits
            fileWriter.Write($" --set normal {OneLineString(normal)}"); //normal
            fileWriter.Write($" --set special {OneLineString(special)}"); //special
            fileWriter.Write(" --set descflag 1"); //descflag
            fileWriter.Write($" --set description {OneLineString(description)}"); // description
            fileWriter.WriteLine();
        }

        private static void WriteNpcSpecialAttack(StreamWriter fileWriter, HeroLabCharacter character, HeroLabSpecial special)
        {
            fileWriter.Write($"!HeroLabImporter --name {character.Name} --mode add --addtype npcatk-special");
            fileWriter.Write($" --set atkname {special.ShortName}");
            var trackedResource = character.TrackedResources.Items.FirstOrDefault(tr => tr.Name == special.Name);
            if (trackedResource != null)
            {
                fileWriter.Write($" --set perday_max {trackedResource.Max}");
            }
            fileWriter.Write($" --set atkdesc {OneLineString(special.Description)}");
            //First, we need to find the item that contains information on how this thing works.
            var checkers = Regex.Matches(special.Name, @"(?<=\().+?(?=\))");
            bool hasAttack = false;
            bool hasRange = false;
            bool hasArea = false;
            bool hasEffect = false;
            bool hasSavingThrow = false;
            bool hasSavingThrowDc = false;
            bool hasDamage = false;
            bool hasDamage2 = false;
            foreach (Match check in checkers)
            {
                if (check.Value.Contains("DC") || check.Value.Contains("ft.") || check.Value.Contains("rounds") ||
                    check.Value.Contains("rnd"))
                {
                    var checkItems = check.Value.Split(new[] { ',', ';' });
                    foreach (var item in checkItems)
                    {
                        if (item.ToLower().Contains("cone") && !hasRange)
                        {
                            fileWriter.Write(" --set rangeflag 1");
                            fileWriter.Write($" --set atkrange {item}");
                            hasRange = true;
                        }
                        else if (item.ToLower().Contains("dc") && !hasSavingThrowDc)
                        {
                            var dc = Regex.Match(item, @"\d+");
                            if (dc.Success)
                            {
                                fileWriter.Write($" --set atkdc {dc.Value}");
                                hasSavingThrowDc = true;
                                if (hasSavingThrow)
                                {
                                    fileWriter.Write(" --set atksaveflag 1");
                                }
                            }
                            if ((item.ToLower().Contains("half") || item.ToLower().Contains("partial") ||
                                 item.ToLower().Contains("negates")) && !hasSavingThrow)
                            {
                                var fixedItem = Regex.Replace(item.ToLower(), @"DC \d+", "").Trim();
                                hasSavingThrow = true;
                                fileWriter.Write($" --set atksave {fixedItem}");
                                if (hasSavingThrowDc)
                                {
                                    fileWriter.Write(" --set atksaveflag 1");
                                }
                            }
                        }
                        else if (item.ToLower().Contains("feet") && !hasRange)
                        {
                            fileWriter.Write(" --set rangeflag 1");
                            fileWriter.Write($" --set atkrange {item}");
                            hasRange = true;
                        }
                        else if ((item.ToLower().Contains("fire") || item.ToLower().Contains("sonic") ||
                                  item.ToLower().Contains("acid") || item.ToLower().Contains("cold") ||
                                  item.ToLower().Contains("sonic")) && !hasDamage)
                        {
                            var possibleDamage = Regex.Match(item.ToLower(), @"\d+d\d+[+-]?\d+?");
                            if (possibleDamage.Success)
                            {
                                fileWriter.Write(" --set dmgflag 1");
                                fileWriter.Write($" --set dmgbase {possibleDamage.Value}");
                                fileWriter.Write(
                                    $" --set dmgtype {item.ToLower().Replace(possibleDamage.Value, "").Trim()}");
                                hasDamage = true;

                            }

                        }
                        else if ((item.ToLower().Contains("fire") || item.ToLower().Contains("sonic") ||
                                  item.ToLower().Contains("acid") || item.ToLower().Contains("cold") ||
                                  item.ToLower().Contains("sonic")) && !hasDamage2)
                        {
                            var possibleDamage = Regex.Match(item.ToLower(), @"\d+d\d+[+-]?\d+?");
                            if (possibleDamage.Success)
                            {
                                fileWriter.Write(" --set dmgflag2 1");
                                fileWriter.Write($" --set dmgbase2 {possibleDamage.Value}");
                                fileWriter.Write(
                                    $" --set dmgtype2 {item.ToLower().Replace(possibleDamage.Value, "").Trim()}");
                                hasDamage2 = true;
                            }
                        }
                        else if ((item.ToLower().Contains("half") || item.ToLower().Contains("partial") ||
                                  item.ToLower().Contains("negates")) && !hasSavingThrow)
                        {
                            var fixedItem = Regex.Replace(item.ToLower(), @"dc \d+", "").Trim();
                            hasSavingThrow = true;
                            fileWriter.Write($" --set atksave {fixedItem}");
                            if (hasSavingThrowDc)
                            {
                                fileWriter.Write(" --set atksaveflag 1");
                            }
                        }
                        else if (item.ToLower().Contains("every") && !hasEffect)
                        {
                            var finalItem = item.ToLower();
                            var replacements = Regex.Matches(item.ToLower(), @"\d+d\d+[+-]?\d+?");
                            foreach (Match match in replacements)
                            {
                                finalItem = finalItem.Replace(match.Value, $"[[{match.Value}]]");
                            }
                            fileWriter.Write($" --set atkeffect {finalItem}");
                            fileWriter.Write(" --set effectflag 1");
                            hasEffect = true;
                        }
                    }
                }
            }
            fileWriter.WriteLine();
        }

        

        private static void WriteNpcMeleeWeapon(StreamWriter fileWriter, string type, HeroLabCharacter character, Weapon melee)
        {
            fileWriter.Write($"!HeroLabImporter --name {character.Name} --mode add -addtype npcatk-{type}");
            fileWriter.Write($" --set atkname {melee.Name}");
            if (melee.Attack.Contains("/"))
            {
                fileWriter.Write($" --set multipleatk_flag 1");
                var attackNumber = 1;
                foreach (var attack in melee.Attack.Split('/'))
                {
                    fileWriter.Write(attackNumber == 1
                        ? $" --set atkmod {attack}"
                        : $" --set atkmod{attackNumber} {attack}");
                    attackNumber++;
                }
            }
            else
            {
                fileWriter.Write(" --set multipleatk_flag 0");
                fileWriter.Write($" --set atkmod {melee.Attack}");
            }
            WriteNpcCrit(fileWriter, melee.Crit);
            
            if (string.IsNullOrEmpty(melee.Damage))
            {
                fileWriter.Write(" --set dmgflag 0");
            }
            else if (!melee.Damage.Contains(" "))
            {
                fileWriter.Write(" --set dmgflag 1");
                fileWriter.Write($" --set dmgbase {melee.Damage}");
            }
            else
            {
                var damage = melee.Damage.Split(' ')[0];
                var plus = melee.Damage.Split(new[] { " plus " }, StringSplitOptions.None)[1];
                if (Regex.IsMatch(plus, @"\d+d\d+"))
                {
                    var plusDamage = Regex.Match(plus, @"\d+d\d+").Value;
                    var plusExtra = plus.Replace(plusDamage, "").Trim();
                    fileWriter.Write($" --set dmgbase {damage}");
                    fileWriter.Write(" --set dmgflag 1");
                    fileWriter.Write($" --set dmg2base {plusDamage}");
                    fileWriter.Write($" --set dmg2type {plusExtra}");
                    fileWriter.Write(" --set dmg2flag 1");
                }
                else
                {
                    fileWriter.Write(" --set dmgflag 1");
                    fileWriter.Write($" --set dmgbase {damage}");
                    fileWriter.Write($" --set dmgtype {plus}");
                }
            }
            if (type == "ranged" && melee.RangedAttack != null)
            {
                fileWriter.Write($" --set atkrange {melee.RangedAttack.RangeIncrementText}");
            }
            fileWriter.WriteLine();
        }

        private static void WriteNpcCrit(StreamWriter fileWriter, string meleeCrit)
        {
            switch (meleeCrit)
            {
                case "15-20/x2":
                    fileWriter.Write(" --set atkcritrange 15");
                    fileWriter.Write(" --set dmgcritmulti 2");
                    break;
                case "17-20/x2":
                    fileWriter.Write(" --set atkcritrange 17");
                    fileWriter.Write(" --set dmgcritmulti 2");
                    break;
                case "18-20/x2":
                    fileWriter.Write(" --set atkcritrange 18");
                    fileWriter.Write(" --set dmgcritmulti 2");
                    break;
                case "19-20/x2":
                    fileWriter.Write(" --set atkcritrange 19");
                    fileWriter.Write(" --set dmgcritmulti 2");
                    break;

                case "":
                    fileWriter.Write(" --set atkcritrange 20");
                    fileWriter.Write(" --set dmgcritmulti 2");
                    break;
                case "x3":
                    fileWriter.Write(" --set atkcritrange 20");
                    fileWriter.Write(" --set dmgcritmulti 3");
                    break;
                case "x4":
                    fileWriter.Write(" --set atkcritrange 20");
                    fileWriter.Write(" --set dmgcritmulti 4");
                    break;
                case "x5":
                    fileWriter.Write(" --set atkcritrange 20");
                    fileWriter.Write(" --set dmgcritmulti 5");
                    break;

                case "15-20/x3":
                    fileWriter.Write(" --set atkcritrange 15");
                    fileWriter.Write(" --set dmgcritmulti 3");
                    break;
                case "17-20/x3":
                    fileWriter.Write(" --set atkcritrange 17");
                    fileWriter.Write(" --set dmgcritmulti 3");
                    break;
                case "18-20/x3":
                    fileWriter.Write(" --set atkcritrange 18");
                    fileWriter.Write(" --set dmgcritmulti 3");
                    break;
                case "19-20/x3":
                    fileWriter.Write(" --set atkcritrange 19");
                    fileWriter.Write(" --set dmgcritmulti 3");
                    break;

                case "15-20/x4":
                    fileWriter.Write(" --set atkcritrange 15");
                    fileWriter.Write(" --set dmgcritmulti 4");
                    break;
                case "17-20/x4":
                    fileWriter.Write(" --set atkcritrange 17");
                    fileWriter.Write(" --set dmgcritmulti 4");
                    break;
                case "18-20/x4":
                    fileWriter.Write(" --set atkcritrange 18");
                    fileWriter.Write(" --set dmgcritmulti 4");
                    break;
                case "19-20/x4":
                    fileWriter.Write(" --set atkcritrange 19");
                    fileWriter.Write(" --set dmgcritmulti 4");
                    break;

                case "15-20/x5":
                    fileWriter.Write(" --set atkcritrange 15");
                    fileWriter.Write(" --set dmgcritmulti 5");
                    break;
                case "17-20/x5":
                    fileWriter.Write(" --set atkcritrange 17");
                    fileWriter.Write(" --set dmgcritmulti 5");
                    break;
                case "18-20/x5":
                    fileWriter.Write(" --set atkcritrange 18");
                    fileWriter.Write(" --set dmgcritmulti 5");
                    break;
                case "19-20/x5":
                    fileWriter.Write(" --set atkcritrange 19");
                    fileWriter.Write(" --set dmgcritmulti 5");
                    break;
            }
        }

        private static void WriteNpcStatistics(StreamWriter fileWriter, HeroLabCharacter character)
        {
            foreach (var attribute in character.Attributes.Items)
            {
                fileWriter.Write($" --set {attribute.Name.ToLower()} {attribute.Value.Modified}");
            }

            fileWriter.Write($" --set bab {character.Attack.BaseAttack}");
            fileWriter.Write($" --set cmb_mod {character.Maneuvers.CombatManeuverBonus}");
            fileWriter.Write($" --set cmd_mod {character.Maneuvers.CombatManeuverDefense}");
            var cmbText = "";
            var cmdText = "";
            var immuneCmds = new List<string>();
            foreach (var maneuver in character.Maneuvers.Items)
            {
                if (maneuver.CombatManeuverDefense == "Immune") { immuneCmds.Add(maneuver.Name); }
                else if (maneuver.CombatManeuverDefense != character.Maneuvers.CombatManeuverDefense)
                {
                    if (!string.IsNullOrEmpty(cmdText))
                    {
                        cmdText += ", ";
                    }
                    cmdText += $"{maneuver.CombatManeuverDefense} vs {maneuver.Name}";
                }
                if (maneuver.CombatManeuverBonus != character.Maneuvers.CombatManeuverBonus)
                {
                    if (!string.IsNullOrEmpty(cmbText))
                    {
                        cmbText += ", ";
                    }
                    cmbText += $"{maneuver.CombatManeuverBonus} vs {maneuver.Name}";
                }
            }
            cmdText = !string.IsNullOrEmpty(cmdText) ? $"FF {character.Maneuvers.CombatManeuverDefenseFlatFooted}; {cmdText}" : $"FF {character.Maneuvers.CombatManeuverDefenseFlatFooted}";

            if (immuneCmds.Any())
            {
                cmdText += $"; Immune: {CommaDelimitedList(immuneCmds)}";
            }
            if (!string.IsNullOrEmpty(cmbText))
                fileWriter.Write($" --set cmb_notes {cmbText}");
            fileWriter.Write($" --set cmd_notes {cmdText}");

            //Skills
            foreach (var skill in character.Skills.Items)
            {
                var skillName = skill.Name.ToLower().Replace(' ', '_').Replace("(", "").Replace(")", "");
                fileWriter.Write($" --set {skillName} {skill.Value}");
                if (!string.IsNullOrEmpty(skill.SituationalModifiers?.Text))
                {
                    fileWriter.Write($" --set {skillName}_notes {skill.SituationalModifiers.Text}");
                }
            }
            if (character.Languages?.Items?.Any() == true)
                fileWriter.Write($" --set languages {CommaDelimitedList(character.Languages.Items.Select(l => l.Name))}");
            if (character.OtherSpecials?.Items?.Any() == true)
                fileWriter.Write($" --set sq {CommaDelimitedList(character.OtherSpecials.Items.Select(l => l.ShortName))}");

            var allItems = character.MagicItems.Items
                .Select(mi => $"{mi.Name}*{(GetValue(mi.Quantity) > 1 ? $" x{mi.Quantity}" : "")}").Union(
                    character.Gear.Items.Select(g => $"{g.Name}{(GetValue(g.Quantity) > 1 ? $" x{g.Quantity}" : "")}"));
            fileWriter.Write($" --set combat_gear {CommaDelimitedList(allItems)}");
            //Not supported by HeroLab.
            fileWriter.Write(" --set ecology_flag 0");
            //Special Abilities
            fileWriter.Write(character.OtherSpecials?.Items?.Any() == true
                ? " --set special_abilities_flag 1"
                : " --set special_abilities_flag 0");
        }

        private static void WriteNpcOffense(StreamWriter fileWriter, HeroLabCharacter character)
        {
            fileWriter.Write($" --set npc_speed {character.Movement.BaseSpeed.Value}");
            fileWriter.Write($" --set space {character.Size.Space.Value}");
            fileWriter.Write($" --set reach {character.Size.Reach.Value}");
            fileWriter.Write($" --set meleeattacks_flag {(character.Melee?.Items?.Any() == true ? 1 : 0)}");
            fileWriter.Write($" --set rangedattacks_flag {(character.Ranged?.Items?.Any() == true ? 1 : 0)}");
            fileWriter.Write($" --set specialattacks_flag {(character.OtherSpecials?.Items?.Any() == true ? 1 : 0)}");
            fileWriter.Write(character.SpellLikeAbilities?.Items?.Any() == true
                ? " --set spellabilities_flag 1"
                : " --set spellabilities_flag 0");
            if (character.SpellClasses?.Items?.Any(sc => int.Parse(sc.MaxSpellLevel) > 0) == true)
            {
                fileWriter.Write(" --set spells_flag 1");
                var casterLevel = character.Classes.Classes.Max(c => GetValue(c.CasterLevel));
                var concentration = character.Classes.Classes.Max(c => GetValue(c.ConcentrationCheck));
                fileWriter.Write($" --set caster1_level {casterLevel}");
                fileWriter.Write($" --set caster1_concentration {concentration}");
                var spellsPerDay = new Dictionary<int, int>();
                for (int spellLevel = 1; spellLevel <= 9; spellLevel++)
                {
                    spellsPerDay.Add(spellLevel, 0);
                    foreach (var spellClass in character.SpellClasses.Items)
                    {
                        var curLevel = spellClass.Levels.FirstOrDefault(l => GetValue(l.Level) == spellLevel);
                        if (curLevel != null)
                        {
                            spellsPerDay[spellLevel] += GetValue(curLevel.MaxCasts);
                        }
                    }
                }
                foreach (var spellLevel in spellsPerDay.Where(d => d.Value > 0))
                {
                    fileWriter.Write($" --set caster1_spells_perday_level_{spellLevel.Key} {spellLevel.Value}");
                }
            }
            else
            {
                fileWriter.Write(" --set spells_flag 0");
            }
        }

        private static void WriteNpcDefense(StreamWriter fileWriter, HeroLabCharacter character)
        {
            fileWriter.Write($" --set ac {character.ArmorClass.TotalArmorClass}");
            fileWriter.Write($" --set ac_flatfooted {character.ArmorClass.FlatFootedArmorClass}");
            fileWriter.Write($" --set ac_touch {character.ArmorClass.TouchArmorClass}");
            fileWriter.Write($" --set acnotes {WriteNpcArmorClassNotes(character.ArmorClass)}");
            fileWriter.Write($" --set hp {character.Health.CurrentHp}");
            fileWriter.Write($" --set hp_max {character.Health.HitPoints}");
            fileWriter.Write($" --set hd_roll {character.Health.HitDice}");
            var hd = character.Health.HitDice.Split('d')[0];
            if (character.Health.HitDice.Contains("HD"))
            {
                hd = character.Health.HitDice.Split(' ')[0];
            }
            fileWriter.Write($" --set hd {hd}");
            var fort = character.Saves.Items.First(i => i.Name == "Fortitude Save");
            var will = character.Saves.Items.First(i => i.Name == "Will Save");
            var reflex = character.Saves.Items.First(i => i.Name == "Reflex Save");
            fileWriter.Write($" --set fortitude {fort.Value}");
            fileWriter.Write($" --set will {will.Value}");
            fileWriter.Write($" --set reflex {reflex.Value}");
            var saveMods = "";
            if (!string.IsNullOrEmpty(fort.SituationalModifiers.Text))
                saveMods += fort.SituationalModifiers.Text;
            if (!string.IsNullOrEmpty(will.SituationalModifiers.Text))
            {
                if (!string.IsNullOrEmpty(saveMods)) saveMods += ", ";
                saveMods += will.SituationalModifiers.Text;
            }
            if (!string.IsNullOrEmpty(reflex.SituationalModifiers.Text))
            {
                if (!string.IsNullOrEmpty(saveMods)) saveMods += ", ";
                saveMods += reflex.SituationalModifiers.Text;
            }
            if (!string.IsNullOrEmpty(saveMods))
                fileWriter.Write($" --set saves_modifiers {saveMods}");
            if (character.Resistances?.Items?.Any(r => r.Name.Contains("Spell Resistance")) == true)
            {
                var sr = character.Resistances.Items.First(r => r.Name.Contains("Spell Resistance")).ShortName.Split(' ')[1];
                fileWriter.Write($" --set sr {sr}");
            }
            WriteNpcAbilities(fileWriter, "defensive_abilities", character.Defensive.Items);
            WriteNpcAbilities(fileWriter, "resist", character.Resistances.Items);
            WriteNpcAbilities(fileWriter, "immune", character.Immunities.Items);
            WriteNpcAbilities(fileWriter, "weaknesses", character.Weaknesses.Items);
        }

        private static void WriteNpcMain(StreamWriter fileWriter, HeroLabCharacter character)
        {
            fileWriter.Write($" --set npc_cr {character.ChallengeRating.Value}");
            fileWriter.Write($" --set attr_xp {character.XpAward.Value}");
            fileWriter.Write($" --set npc_alignment {GetAlignment(character.Alignment.Name)}");
            fileWriter.Write($" --set size {character.Size.Name.ToLower()}");
            string raceType = string.Empty;
            if (character.RaceTypes?.Items?.Any() == true)
                raceType += character.RaceTypes.Items.First().Name;
            if (character.SubTypes?.Items?.Any() == true)
            {
                var subTypes = " (";
                foreach (var st in character.SubTypes.Items)
                {
                    if (subTypes.Length > 2) subTypes += ", ";
                    subTypes += st.Name;
                }
                raceType += subTypes + ")";
            }
            fileWriter.Write($" --set npc_type {raceType}");
            if (int.Parse(character.Classes?.Level ?? "0") > 0)
            {
                fileWriter.Write($" --set class {character.Classes.Summary}");
            }
            fileWriter.Write($" --set initiative {character.Initiative.Total}");
            if (!string.IsNullOrEmpty(character.Initiative.SituationalModifiers.Text))
                fileWriter.Write($" --set initiative_notes {character.Initiative.SituationalModifiers.Text}");
            if (character.Senses?.Items?.Any() == true)
            {
                var senses = "";
                foreach (var sense in character.Senses.Items)
                {
                    if (!string.IsNullOrEmpty(senses))
                        senses += ", ";
                    senses += sense.ShortName;
                }
                fileWriter.Write($" --senses {senses}");
            }
        }


        private static void WriteNpcAbilities(StreamWriter fileWriter, string type, HeroLabSpecial[] specials)
        {
            var list = "";
            if (specials != null)
                foreach (var l in specials)
                {
                    if (!string.IsNullOrEmpty(list)) list += ", ";
                    list += l.ShortName;
                }
            if (!string.IsNullOrEmpty(list))
                fileWriter.Write($" --set {type} {list}");
        }

        private static string WriteNpcArmorClassNotes(ArmorClass armorClass)
        {
            var acNotes = "";
            acNotes = GetNpcAcNotes(acNotes, "Dex", armorClass.FromDexterity);
            acNotes = GetNpcAcNotes(acNotes, "Armor", armorClass.FromArmorClass);
            acNotes = GetNpcAcNotes(acNotes, "Shield", armorClass.FromShield);
            acNotes = GetNpcAcNotes(acNotes, "Wis", armorClass.FromWisdom);
            acNotes = GetNpcAcNotes(acNotes, "Cha", armorClass.FromCharisma);
            acNotes = GetNpcAcNotes(acNotes, "Size", armorClass.FromSize);
            acNotes = GetNpcAcNotes(acNotes, "Nat", armorClass.FromNatural);
            acNotes = GetNpcAcNotes(acNotes, "Deflect", armorClass.FromDeflect);
            acNotes = GetNpcAcNotes(acNotes, "Dodge", armorClass.FromDodge);
            acNotes = GetNpcAcNotes(acNotes, "Misc", armorClass.FromMisc);
            return acNotes;
        }

        private static string GetNpcAcNotes(string acNotes, string type, string value)
        {
            if (!string.IsNullOrEmpty(value))
            {
                if (!string.IsNullOrEmpty(acNotes)) acNotes += ", ";
                acNotes += $"{value} {type}";
            }
            return acNotes;
        }
    }
}
