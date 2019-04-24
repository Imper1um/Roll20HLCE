using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
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
            var items = new List<string>();
            var mainCharacter = new StringBuilder("!HeroLabImporter --mode clear --set npc 1 --set options-flag-npc 0");
            mainCharacter.Append($" --name {character.Name}");

            WriteNpcMain(mainCharacter, character);
            WriteNpcDefense(mainCharacter, character);
            WriteNpcOffense(mainCharacter, character);
            WriteNpcStatistics(mainCharacter, character);
            items.Add(mainCharacter.ToString());

            //Melee Weapons
            if (character.Melee?.Items?.Any() == true)
                foreach (var melee in character.Melee.Items)
                {
                    var item = WriteNpcWeapon("melee", character, melee);
                    if (!string.IsNullOrEmpty(item)) items.Add(item);
                }

            //Ranged Weapons
            if (character.Ranged?.Items?.Any() == true)
                foreach (var ranged in character.Ranged.Items)
                {
                    var item = WriteNpcWeapon("ranged", character, ranged);
                    if (!string.IsNullOrEmpty(item)) items.Add(item);

                }

            //Special Attacks
            if (character.Attack?.Items?.Any() == true)
                foreach (var sa in character.Attack.Items)
                {
                    var item = WriteNpcSpecialAttack(character, sa);
                    if (!string.IsNullOrEmpty(item)) items.Add(item);
                }

            //Spell-Like Abilities
            if (character.SpellLikeAbilities?.Items?.Any() == true)
                foreach (var spellLikeAbility in character.SpellLikeAbilities.Items)
                {
                    var item = WriteNpcSpellLikeAbility(character, spellLikeAbility);
                    if (!string.IsNullOrEmpty(item)) items.Add(item);
                }

            //Spells
            if (character.SpellsMemorized?.Items?.Any() == true)
                foreach (var spell in character.SpellsMemorized.Items)
                {
                    var item = WriteNpcSpell(character, spell);
                    if (!string.IsNullOrEmpty(item)) items.Add(item);
                }

            //Feats
            if (character.Feats?.Items?.Any() == true)
                foreach (var feat in character.Feats.Items)
                {
                    var item = WriteFeat(character, feat);
                    if (!string.IsNullOrEmpty(item)) items.Add(item);
                }

            //Special Abilities
            if (character.OtherSpecials?.Items?.Any() == true)
                foreach (var specialAbilities in character.OtherSpecials.Items)
                {
                    var item = WriteSpecialAbility(character, specialAbilities);
                    if (!string.IsNullOrEmpty(item)) items.Add(item);
                }

            using (var file = File.Open($"C:\\PathfinderExporter\\{character.Name}_NPC.txt", FileMode.Create, FileAccess.Write, FileShare.Read))
            using (var fileWriter = new StreamWriter(file) { AutoFlush = true })
            {
                foreach (var line in items)
                    if (line == items.Last())
                        fileWriter.Write(line);
                    else
                        fileWriter.WriteLine(line);
            }
        }

        private static string WriteSpecialAbility(HeroLabCharacter character, HeroLabSpecial specialAbilities)
        {
            var stringBuilder = new StringBuilder($"!HeroLabImporter --name {character.Name} --mode add --addtype abilities");
            stringBuilder.Append($" --set name {specialAbilities.Name}");
            stringBuilder.Append($" --set type {GetSpecialAbilityType(specialAbilities.Type)}");
            stringBuilder.Append($" --set descflag 1");
            stringBuilder.Append($" --set options-flag 0");
            stringBuilder.Append($" --set description {OneLineString(specialAbilities.Description.Replace('\r', ' ').Replace('\n', ' ').Trim())}");
            return stringBuilder.ToString();
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

        private static string WriteFeat(HeroLabCharacter character, Feat feat)
        {

            var stringBuilder = new StringBuilder($"!HeroLabImporter --name {character.Name} --mode add --addtype feats");
            
            stringBuilder.Append($" --set name {feat.Name}"); //name
            stringBuilder.Append($" --set type {(string.IsNullOrEmpty(feat.CategoryText) ? "general" : feat.CategoryText.ToLower())}"); //type
            stringBuilder.Append($" --set options-flag 0");

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
            if (!string.IsNullOrEmpty(prerequisites.Trim()) && prerequisites.Length > 1)
                stringBuilder.Append($" --set prerequisites {OneLineString(prerequisites)}"); //prerequisites
            if (!string.IsNullOrEmpty(benefits.Trim()) && benefits.Length > 1)
                stringBuilder.Append($" --set benefits {OneLineString(benefits)}"); //benefits
            if (!string.IsNullOrEmpty(normal.Trim()) && normal.Length > 1)
                stringBuilder.Append($" --set normal {OneLineString(normal)}"); //normal
            if (!string.IsNullOrEmpty(special.Trim()) && special.Length > 1)
                stringBuilder.Append($" --set special {OneLineString(special)}"); //special
            if (!string.IsNullOrEmpty(description.Trim()) && description.Length > 1)
            {
                stringBuilder.Append(" --set descflag 1"); //descflag
                stringBuilder.Append($" --set description {OneLineString(description)}"); // description
            }
            return stringBuilder.ToString();
        }

        private static string WriteNpcSpecialAttack(HeroLabCharacter character, HeroLabSpecial special)
        {
            var stringBuilder = new StringBuilder($"!HeroLabImporter --name {character.Name} --mode add --addtype npcatk-special");
            stringBuilder.Append($" --set atkname {special.ShortName}");
            stringBuilder.Append($" --set atkdisplay {special.ShortName}");
            stringBuilder.Append($" --set options-flag 0");
            var trackedResource = character.TrackedResources?.Items?.FirstOrDefault(tr => tr.Name == special.Name);
            if (trackedResource != null)
            {
                stringBuilder.Append($" --set perday 0");
                stringBuilder.Append($" --set perday!max {trackedResource.Max}");
            }
            stringBuilder.Append($" --set atkdesc {OneLineString(special.Description)}");
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
            string saveDc = null;
            string saveText = null;
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
                            stringBuilder.Append(" --set rangeflag " + PreventRollingString("{{range=@{atkrange}}}"));
                            stringBuilder.Append($" --set atkrange {item}");
                            hasRange = true;
                        }
                        else if (item.ToLower().Contains("dc") && !hasSavingThrowDc)
                        {
                            var dc = Regex.Match(item, @"\d+");
                            if (dc.Success)
                            {
                                saveDc = dc.Value;
                                stringBuilder.Append($" --set atkdc {dc.Value}");
                                hasSavingThrowDc = true;
                                if (hasSavingThrow)
                                {
                                    stringBuilder.Append(" --set atksaveflag " + PreventRollingString("{{save=1}}{{savedc=@{atkdc}}}{{saveeffect=@{atksave}}}"));
                                }
                            }
                            if (!hasSavingThrow)
                            {
                                var desc = special.Description.ToLower();
                                var modDesc = "";
                                if (desc.Contains("half") || desc.Contains("halves"))
                                {
                                    modDesc = "half";
                                }
                                else if (desc.Contains("partial"))
                                {
                                    modDesc = "partial";
                                }
                                else if (desc.Contains("negate"))
                                {
                                    modDesc = "negates";
                                }
                                var checkDesc = "";
                                if (desc.Contains("fortitude")) checkDesc = "Fortitude";
                                if (desc.Contains("reflex")) checkDesc = "Reflex";
                                if (desc.Contains("will")) checkDesc = "Will";
                                if (!string.IsNullOrEmpty(modDesc) && !string.IsNullOrEmpty(checkDesc))
                                {
                                    stringBuilder.Append($" --set atksave {checkDesc} {modDesc}");
                                    saveText = $"{checkDesc} {modDesc}";
                                    hasSavingThrow = true;
                                    if (hasSavingThrowDc)
                                    {
                                        stringBuilder.Append(" --set atksaveflag " + PreventRollingString("{{save=1}}{{savedc=@{atkdc}}}{{saveeffect=@{atksave}}}"));
                                    }
                                }
                            }
                        }
                        else if (item.ToLower().Contains("feet") && !hasRange)
                        {
                            stringBuilder.Append(" --set rangeflag " + PreventRollingString("{{range=@{atkrange}}}"));
                            stringBuilder.Append($" --set atkrange {item}");
                            hasRange = true;
                        }
                        else if ((item.ToLower().Contains("fire") || item.ToLower().Contains("sonic") ||
                                  item.ToLower().Contains("acid") || item.ToLower().Contains("cold") ||
                                  item.ToLower().Contains("sonic")) && !hasDamage)
                        {
                            var possibleDamage = Regex.Match(item.ToLower(), @"\d+d\d+[+-]?\d*");
                            if (possibleDamage.Success)
                            {
                                stringBuilder.Append(" --set dmgflag1 1");
                                stringBuilder.Append(" --set dmgflag " + PreventRollingString("{{damage=1}}{{dmg1flag=1}}{{dmg1=[[@{dmgbase}[MOD]+@{rollmod_damage}[QUERY]]]}}{{dmg1type=@{dmgtype}}}{{dmg1crit=[[(@{dmgbase}[MOD]+@{rollmod_damage}[QUERY])*@{dmgcritmulti}]]}}"));
                                //stringBuilder.Append(" --set dmgflag "+ PreventRollingString("{{damage=1}}{{dmg1flag=1}}{{dmg1=[[" + possibleDamage.Value + "]]}}{{dmg1type=" + item.ToLower().Replace(possibleDamage.Value, "").Trim() + "}}{{dmg1crit=[[(" + possibleDamage.Value + ")*2]]}}"));
                                stringBuilder.Append($" --set dmgbase {possibleDamage.Value}");
                                stringBuilder.Append(
                                    $" --set dmgtype {item.ToLower().Replace(possibleDamage.Value, "").Trim()}");
                                hasDamage = true;

                            }

                        }
                        else if ((item.ToLower().Contains("fire") || item.ToLower().Contains("sonic") ||
                                  item.ToLower().Contains("acid") || item.ToLower().Contains("cold") ||
                                  item.ToLower().Contains("sonic")) && !hasDamage2)
                        {
                            var possibleDamage = Regex.Match(item.ToLower(), @"\d+d\d+[+-]?\d*");
                            if (possibleDamage.Success)
                            {
                                stringBuilder.Append(" --set dmgflag2 " + PreventRollingString("{{damage=1}}{{dmg2flag=1}}{{dmg2=[[@{dmg2base}[MOD]+@{rollmod_damage}[QUERY]]]}}{{dmg2type=@{dmg2type}}}{{dmg1crit=[[(@{dmg2base}[MOD]+@{rollmod_damage}[QUERY])*@{dmg2critmulti}]]}}"));
                                //stringBuilder.Append(" --set dmgflag2 " + PreventRollingString("{{damage=1}}{{dmg2flag=1}}{{dmg2=[[" + possibleDamage.Value + "]]}}{{dmg2type=" + item.ToLower().Replace(possibleDamage.Value, "").Trim() + "}}{{dmg2crit=[[(" + possibleDamage.Value + ")]]}}"));
                                stringBuilder.Append($" --set dmgbase2 {possibleDamage.Value}");
                                stringBuilder.Append(
                                    $" --set dmgtype2 {item.ToLower().Replace(possibleDamage.Value, "").Trim()}");
                                hasDamage2 = true;
                            }
                        }
                        else if ((item.ToLower().Contains("half") || item.ToLower().Contains("partial") ||
                                  item.ToLower().Contains("negates")) && !hasSavingThrow)
                        {
                            var fixedItem = Regex.Replace(item.ToLower(), @"dc \d*", "").Trim();
                            hasSavingThrow = true;
                            stringBuilder.Append($" --set atksave {fixedItem}");
                            saveText = fixedItem;
                            if (hasSavingThrowDc)
                            {
                                stringBuilder.Append(" --set atksaveflag "+PreventRollingString("{{save=1}}{{savedc=@{atkdc}}}{{saveeffect=@{atksave}}}"));
                            }
                        }
                        else if (item.ToLower().Contains("every") && !hasEffect)
                        {
                            var finalItem = item.ToLower();
                            var replacements = Regex.Matches(item.ToLower(), @"\d+d\d+[+-]?\d*");
                            foreach (Match match in replacements)
                            {
                                finalItem = finalItem.Replace(match.Value, $"[[{match.Value}]]");
                            }
                            stringBuilder.Append($" --set atkeffect {finalItem}");
                            stringBuilder.Append(" --set effectflag " + PreventRollingString("{{effect=@{atkeffect}}}"));
                            hasEffect = true;
                        }
                    }
                }
            }
            stringBuilder.Append(" --set options-flag 0");
            return stringBuilder.ToString();
        }

        private static string GetNpcWeaponMultipleAttackFlag(Weapon weapon)
        {
            //This is a little complicated, because we're trying to build out a roll template for the attack.
            const string baseDamageKey = "#basedamage#"; // = Damage roll without crit. "1d4"
            const string critDamageKey = "#critdamage#"; // = Damage roll with a crit. "2d4"
            const string critMultiplierKey = "#critmultiplier#"; //#critmultiplier# = Multiplier integer "2"
            const string bonusDamageKey = "#bonusdamage#"; // = Bonus damage roll without crit. "1d6"
            const string bonusCritDamageKey = "#bonuscritdamage#"; // = Bonus crit damage roll with crit "2d6"
            const string bonusCritDamageMultiplierKey = "#bonuscritdamagemultipler#"; // = Bonus damage multiplier integer "2"
            const string rollNumberKey = "#rollnumber#"; // = Subsequent roll number integer.
            const string rollNumberPlusKey = "#rollnumberplus#"; // = Subsequent roll number integer plus 1

            //Base Roll:
            const string baseRollTemplate = "{{roll=[[1d20cs>@{atkcritrange}+@{atkmod}[MOD]+@{rollmod_attack}[QUERY]]]}}{{critconfirm=[[1d20cs20+@{atkmod}[MOD]+@{rollmod_attack}[QUERY]]]}}{{rolldmg1=[[#basedamage# + @{rollmod_damage}[QUERY]]]}}{{rolldmg1type=@{dmgtype}}}{{rolldmg1crit=[[(#critdamage#) + (@{rollmod_damage}[QUERY]*#critmultiplier#)]]}}";
            //Bonus Damage:
            const string bonusDamageTemplate = "{{rolldmg2=[[#bonusdamage# + @{rollmod_damage}[QUERY]]]}} {{rolldmg2type=@{dmg2type}}}";
            //Bonus Damage (if crittable):
            const string bonusCritDamageTemplate = "{{rolldmg2crit=[[(#bonuscritdamage#) + (@{rollmod_damage}[QUERY]*#bonuscritdamagemultiplier#)]]}}";
            //Subsequent Attacks (second attack, rollnumber is 1):
            const string subsequentRollTemplate = "{{roll#rollnumber#=[[1d20cs>@{atkcritrange} + @{atkmod#rollnumberplus#}[MOD] + @{rollmod_attack}[QUERY]]]}} {{critconfirm#rollnumber#=[[1d20cs20 + @{atkmod#rollnumberplus#}[MOD] + @{rollmod_attack}[QUERY]]]}}{{roll#rollnumber#dmg1=[[#basedamage# + @{rollmod_damage}[QUERY]]]}} {{roll#rollnumber#dmg1type=@{dmgtype}}}{{roll#rollnumber#dmg1crit=[[(#critdamage#) + (@{rollmod_damage}[QUERY]2)]]}}";
            //Bonus Damage on Subsequent Attacks:
            const string subsequentBonusDamageTemplate = "{{roll#rollnumber#dmg2=[[#bonusdamage# + @{rollmod_damage}[QUERY]]]}} {{roll#rollnumber#dmg2type=@{dmg2type}}}";
            //Bonus Damage on Subsequent Attacks (if crittable):
            const string subsequentBonusCritDamageTemplate = "{{roll#rollnumber#dmg2crit=[[(#bonuscritdamage#) + (@{rollmod_damage}[QUERY]*#bonuscritdamagemultiplier#)]]}}";

            //Now that we have the keys and the templates, let's start with what we're working with.
            var weaponDamageRoll = "";
            var critWeaponDamageRoll = "";
            var critMultiplier = 2;
            var bonusDamageRoll = "";
            var bonusDamageMultiplier = 1; //This will always be 1 until I figure out a way to support if the bonus damage is crittable. This...usually doesn't happen.
            var bonusCritDamage = "";
            if (string.IsNullOrEmpty(weapon.Damage))
            {
                //No damage, no problem.
                return "0";
            }
            else if (!weapon.Damage.Contains(" "))
            {
                //There's no separation for the weapon damage. Its just a straight-up damage dealer.
                weaponDamageRoll = weapon.Damage;
            }
            else
            {
                //Its got a special thing. Make sure we remove the plus.
                weaponDamageRoll = weapon.Damage.Split(' ')[0];
                var plus = weapon.Damage.Split(new[] { " plus " }, StringSplitOptions.None)[1];
                if (Regex.IsMatch(plus, @"\d+d\d+"))
                {
                    bonusDamageRoll = Regex.Match(plus, @"\d+d\d+").Value;
                }
            }
            //Now, we figure out the crit multiplier
            if (weapon.Crit.Contains("×")) //Its not x, its ×!
            {
                critMultiplier = int.Parse(Regex.Match(weapon.Crit, @"(?<=×)\d").Value);
            }
            for (int i = 0; i < critMultiplier; i++)
            {
                if (!string.IsNullOrEmpty(critWeaponDamageRoll)) critWeaponDamageRoll += " + ";
                critWeaponDamageRoll += weaponDamageRoll;
            }
            for (int i = 0; i < bonusDamageMultiplier; i++)
            {
                if (!string.IsNullOrEmpty(bonusCritDamage)) bonusCritDamage += " + ";
                bonusCritDamage += bonusDamageRoll;
            }
            var numberOfAttacks = weapon.Attack.Contains("/") ? weapon.Attack.Split('/').Length : 1;
            //Alrighty, we got all of the information, now let's stitch it together.
            //First, we start with the initial attack.
            var attackTotal = baseRollTemplate.Replace(baseDamageKey, weaponDamageRoll).Replace(critDamageKey, critWeaponDamageRoll).Replace(critMultiplierKey, critMultiplier.ToString());
            if (!string.IsNullOrEmpty(bonusDamageRoll))
            {
                attackTotal += bonusDamageTemplate.Replace(bonusDamageKey, bonusDamageRoll);
            }
            if (bonusDamageMultiplier > 1)
            {
                attackTotal += bonusCritDamageTemplate.Replace(bonusCritDamageKey, bonusCritDamage).Replace(bonusCritDamageMultiplierKey, bonusDamageMultiplier.ToString());
            }
            //Let's move on to subsequent attacks.
            for (int attack = 1; attack < numberOfAttacks; attack++)
            {
                attackTotal += " ";
                attackTotal += subsequentRollTemplate.Replace(rollNumberKey, attack.ToString()).Replace(rollNumberPlusKey, (attack + 1).ToString()).Replace(baseDamageKey, weaponDamageRoll).Replace(critDamageKey, critWeaponDamageRoll).Replace(critMultiplierKey, critMultiplier.ToString());
                if (!string.IsNullOrEmpty(bonusDamageRoll))
                {
                    attackTotal += subsequentBonusDamageTemplate.Replace(rollNumberKey, attack.ToString()).Replace(rollNumberPlusKey, (attack + 1).ToString()).Replace(bonusDamageKey, bonusDamageRoll);
                }
                if (bonusDamageMultiplier > 1)
                {
                    attackTotal += subsequentBonusCritDamageTemplate.Replace(rollNumberKey, attack.ToString()).Replace(rollNumberPlusKey, (attack + 1).ToString()).Replace(bonusCritDamageKey, bonusCritDamage).Replace(bonusCritDamageMultiplierKey, bonusDamageMultiplier.ToString());
                }
            }
            //aand now we're done with this monstrousity.
            return PreventRollingString(attackTotal);
        }


        private static string WriteNpcWeapon(string type, HeroLabCharacter character, Weapon melee)
        {
            var stringBuilder = new StringBuilder($"!HeroLabImporter --name {character.Name} --mode add --addtype npcatk-{type} --set options-flag on");
            stringBuilder.Append($" --set atkname {melee.Name}");
            stringBuilder.Append($" --set multipleatk {GetNpcWeaponMultipleAttackFlag(melee)}");
            var attackDisplay = melee.Name + " +";
            if (melee.Attack.Contains("/"))
            {
                stringBuilder.Append($" --set multipleatk_flag 1");
                var attackNumber = 1;
                foreach (var attack in melee.Attack.Split('/'))
                {
                    if (attackNumber == 1)
                    {
                        attackDisplay += GetValue(attack);
                    } else
                    {
                        attackDisplay += $"/{GetValue(attack)}";
                    }
                    stringBuilder.Append(attackNumber == 1
                        ? $" --set atkmod {GetValue(attack)}"
                        : $" --set atkmod{attackNumber} {GetValue(attack)}");
                    attackNumber++;
                }
            }
            else
            {
                attackDisplay += GetValue(melee.Attack);
                stringBuilder.Append($" --set atkmod {GetValue(melee.Attack)}");
            }

            var critReplace = melee.Crit?.Replace("×", "x").Replace("/x2","") ?? "";
            if (string.IsNullOrEmpty(melee.Damage))
            {
                stringBuilder.Append(" --set dmgflag 0");
            }
            else if (!melee.Damage.Contains(" "))
            {
                stringBuilder.Append(" --set dmgflag 1");
                stringBuilder.Append($" --set dmgbase {melee.Damage}");
                stringBuilder.Append(WriteNpcCrit(melee.Crit));
                attackDisplay += CritDisplay(melee.Damage, "", critReplace);
            }
            else
            {
                var damage = melee.Damage.Split(' ')[0];
                var plus = melee.Damage.Split(new[] { " plus " }, StringSplitOptions.None)[1];
                if (Regex.IsMatch(plus, @"\d+d\d+"))
                {
                    var plusDamage = Regex.Match(plus, @"\d+d\d+").Value;
                    var plusExtra = plus.Replace(plusDamage, "").Trim();
                    stringBuilder.Append($" --set dmgbase {damage}");
                    stringBuilder.Append(" --set dmgflag 1");
                    stringBuilder.Append($" --set dmg2base {plusDamage}");
                    stringBuilder.Append($" --set dmg2type {plusExtra}");
                    stringBuilder.Append(" --set dmg2flag 1");
                    attackDisplay += CritDisplay(damage, plusDamage, critReplace);
                }
                else
                {
                    stringBuilder.Append(" --set dmgflag 1");
                    stringBuilder.Append($" --set dmgbase {damage}");
                    stringBuilder.Append($" --set dmgtype {plus}");
                    attackDisplay += CritDisplay(damage, plus, critReplace);
                }
                stringBuilder.Append(WriteNpcCrit(melee.Crit));
            }

            stringBuilder.Append($" --set atkdisplay {attackDisplay}");
            

            if (type == "ranged" && melee.RangedAttack != null)
            {
                stringBuilder.Append($" --set atkrange {melee.RangedAttack.RangeIncrementText}");
            }
            stringBuilder.Append(" --set options-flag 0");
            return stringBuilder.ToString();
        }

        private static string CritDisplay(string damage, string plus, string crit)
        {
            return $" ({damage}{(string.IsNullOrEmpty(crit) ? "" : "/")}{crit}{(string.IsNullOrEmpty(plus) ? "" : $" {plus}")})";
            
        }

        private static string WriteNpcCrit(string meleeCrit)
        {
            if (string.IsNullOrEmpty(meleeCrit))
            {
                return " --set atkcritrange 20 --set dmgcritmulti 2";
            }
            if (meleeCrit.Contains("/"))
            {
                var split = meleeCrit.Split('/');
                var range = split[0].Substring(0, 2);
                var multiplier = split[1].Substring(1, 1);
                return $" --set atkcritrange {range} --set dmgcritmulti {multiplier}";
            }
            if (meleeCrit.Contains("-"))
            {
                return $" --set atkcritrange {meleeCrit.Substring(0, 2)} --set dmgcritmulti 2";
            }
            return $" --set atkcritrange 20 --set dmgcritmulti {meleeCrit.Substring(1, 1)}";
        }

        private static void WriteNpcStatistics(StringBuilder stringBuilder, HeroLabCharacter character)
        {
            foreach (var attribute in character.Attributes.Items)
            {
                stringBuilder.Append($" --set {attribute.Name.ToLower()} {attribute.Value.Modified}");
            }

            stringBuilder.Append($" --set bab {character.Attack.BaseAttack}");
            stringBuilder.Append($" --set cmb_mod {GetValue(character.Maneuvers.CombatManeuverBonus)}");
            stringBuilder.Append($" --set cmd_mod {character.Maneuvers.CombatManeuverDefense}");
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
                stringBuilder.Append($" --set cmb_notes {cmbText}");
            stringBuilder.Append($" --set cmd_notes {cmdText}");

            //Skills
            foreach (var skill in character.Skills.Items)
            {
                var skillName = skill.Name.ToLower().Replace(' ', '_').Replace("(", "").Replace(")", "");
                stringBuilder.Append($" --set {skillName} {skill.Value}");
                if (!string.IsNullOrEmpty(skill.SituationalModifiers?.Text))
                {
                    stringBuilder.Append($" --set {skillName}_notes {skill.SituationalModifiers.Text}");
                }
            }
            if (character.Languages?.Items?.Any() == true)
                stringBuilder.Append($" --set languages {CommaDelimitedList(character.Languages.Items.Select(l => l.Name))}");
            if (character.OtherSpecials?.Items?.Any() == true)
                stringBuilder.Append($" --set sq {CommaDelimitedList(character.OtherSpecials.Items.Select(l => l.ShortName))}");

            if (character.MagicItems?.Items != null)
            { 
                var allItems = character.MagicItems.Items
                    .Select(mi => $"{mi.Name}*{(GetValue(mi.Quantity) > 1 ? $" x{mi.Quantity}" : "")}").Union(
                        character.Gear.Items.Select(g => $"{g.Name}{(GetValue(g.Quantity) > 1 ? $" x{g.Quantity}" : "")}"));
                stringBuilder.Append($" --set combat_gear {CommaDelimitedList(allItems)}");
            }
            //Not supported by HeroLab.
            stringBuilder.Append(" --set ecology_flag 0");
            //Special Abilities
            stringBuilder.Append(character.OtherSpecials?.Items?.Any() == true
                ? " --set special_abilities_flag 1"
                : " --set special_abilities_flag 0");
        }

        private static void WriteNpcOffense(StringBuilder stringBuilder, HeroLabCharacter character)
        {
            stringBuilder.Append($" --set npc_speed {character.Movement.BaseSpeed.Value}");
            stringBuilder.Append($" --set space {character.Size.Space.Value}");
            stringBuilder.Append($" --set reach {character.Size.Reach.Value}");
            stringBuilder.Append($" --set meleeattacks_flag {(character.Melee?.Items?.Any() == true ? 1 : 0)}");
            stringBuilder.Append($" --set rangedattacks_flag {(character.Ranged?.Items?.Any() == true ? 1 : 0)}");
            stringBuilder.Append($" --set specialattacks_flag {(character.OtherSpecials?.Items?.Any() == true ? 1 : 0)}");
            stringBuilder.Append(character.SpellLikeAbilities?.Items?.Any() == true
                ? " --set spellabilities_flag 1"
                : " --set spellabilities_flag 0");
            if (character.SpellClasses?.Items?.Any(sc => int.Parse(sc.MaxSpellLevel) > 0) == true)
            {
                stringBuilder.Append(" --set spells_flag 1");
                var casterLevel = character.Classes.Classes.Max(c => GetValue(c.CasterLevel));
                var concentration = character.Classes.Classes.Max(c => GetValue(c.ConcentrationCheck));
                stringBuilder.Append($" --set caster1_level {casterLevel}");
                stringBuilder.Append($" --set caster1_concentration {concentration}");
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
                    stringBuilder.Append($" --set caster1_spells_perday_level_{spellLevel.Key} {spellLevel.Value}");
                }
            }
            else
            {
                stringBuilder.Append(" --set spells_flag 0");
            }
        }

        private static void WriteNpcDefense(StringBuilder stringBuilder, HeroLabCharacter character)
        {
            stringBuilder.Append($" --set ac {character.ArmorClass.TotalArmorClass}");
            stringBuilder.Append($" --set ac_flatfooted {character.ArmorClass.FlatFootedArmorClass}");
            stringBuilder.Append($" --set ac_touch {character.ArmorClass.TouchArmorClass}");
            stringBuilder.Append($" --set ac_notes {WriteNpcArmorClassNotes(character.ArmorClass)}");
            stringBuilder.Append($" --set hp {character.Health.CurrentHp}");
            stringBuilder.Append($" --set hp!max {character.Health.HitPoints}");
            stringBuilder.Append($" --set hp_max {character.Health.HitPoints}");
            stringBuilder.Append($" --set hd_roll {character.Health.HitDice}");
            var hd = character.Health.HitDice.Split('d')[0];
            if (character.Health.HitDice.Contains("HD"))
            {
                hd = character.Health.HitDice.Split(' ')[0];
            }
            stringBuilder.Append($" --set hd {hd}");
            var fort = character.Saves.Items.First(i => i.Name == "Fortitude Save");
            var will = character.Saves.Items.First(i => i.Name == "Will Save");
            var reflex = character.Saves.Items.First(i => i.Name == "Reflex Save");
            stringBuilder.Append($" --set fortitude {GetValue(fort.Value)}");
            stringBuilder.Append($" --set will {GetValue(will.Value)}");
            stringBuilder.Append($" --set reflex {GetValue(reflex.Value)}");
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
                stringBuilder.Append($" --set saves_modifiers {saveMods}");
            if (character.Resistances?.Items?.Any(r => r.Name.Contains("Spell Resistance")) == true)
            {
                var sr = character.Resistances.Items.First(r => r.Name.Contains("Spell Resistance")).ShortName.Split(' ')[1];
                stringBuilder.Append($" --set sr {sr}");
            }
            WriteNpcAbilities(stringBuilder, "defensive_abilities", character.Defensive.Items);
            WriteNpcAbilities(stringBuilder, "resist", character.Resistances.Items);
            WriteNpcAbilities(stringBuilder, "immune", character.Immunities.Items);
            WriteNpcAbilities(stringBuilder, "weaknesses", character.Weaknesses.Items);
        }

        private static void WriteNpcMain(StringBuilder stringBuilder, HeroLabCharacter character)
        {
            stringBuilder.Append($" --set npc_cr {character.ChallengeRating.Value}");
            stringBuilder.Append($" --set xp {character.XpAward.Value}");
            stringBuilder.Append($" --set npc_alignment {GetAlignment(character.Alignment.Name)}");
            stringBuilder.Append($" --set size {character.Size.Name.ToLower()}");
            stringBuilder.Append($" --set size_display {character.Size.Name}");
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
            stringBuilder.Append($" --set npc_type {raceType}");
            if (int.Parse(character.Classes?.Level ?? "0") > 0)
            {
                stringBuilder.Append($" --set class {character.Classes.Summary}");
            }
            stringBuilder.Append($" --set initiative {GetValue(character.Initiative.Total)}");
            if (!string.IsNullOrEmpty(character.Initiative.SituationalModifiers.Text))
                stringBuilder.Append($" --set initiative_notes {character.Initiative.SituationalModifiers.Text}");
            if (character.Senses?.Items?.Any() == true)
            {
                var senses = "";
                foreach (var sense in character.Senses.Items)
                {
                    if (!string.IsNullOrEmpty(senses))
                        senses += ", ";
                    senses += sense.ShortName;
                }
                stringBuilder.Append($" --set senses {senses}");
            }
        }


        private static void WriteNpcAbilities(StringBuilder stringBuilder, string type, HeroLabSpecial[] specials)
        {
            var list = "";
            if (specials != null)
                foreach (var l in specials)
                {
                    if (!string.IsNullOrEmpty(list)) list += ", ";
                    list += l.ShortName;
                }
            if (!string.IsNullOrEmpty(list))
                stringBuilder.Append($" --set {type} {list}");
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
