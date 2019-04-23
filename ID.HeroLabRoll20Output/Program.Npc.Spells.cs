using ID.HeroLabRoll20Output.HeroLab.Character;
using ID.HeroLabRoll20Output.Spells;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ID.HeroLabRoll20Output
{
    public partial class Program
    {
        private static string WriteNpcSpell(HeroLabCharacter character, Spell spell)
        {
            var highestSpellClass = (from sc in character.SpellClasses.Items
                                    join clas in character.Classes.Classes on sc.Name equals clas.Name
                                    orderby clas.CasterLevel descending
                                    select clas).FirstOrDefault();
            var spellClass = character.Classes.Classes.FirstOrDefault(c => c.Name == spell.Class) ?? highestSpellClass;
            if (spellClass == null) return null;
            var stringBuilder = new StringBuilder($"!HeroLabImporter --name {character.Name} --mode add --addtype spell-{spell.Level}");
            stringBuilder.Append(" --set options-flag 0");
            stringBuilder.Append($" --set spellprepared {spell.CastsLeft}");
            SpellSource spellSource = null;
            var spellCrop = spell.Name;
            if (spellCrop.Contains("(")) spellCrop = spellCrop.Split('(')[0].Trim(); ;
            stringBuilder.Append($" --set spelldisplay {spellCrop}");
            stringBuilder.Append($" --set spellname {spellCrop}");
            while (spellSource == null && !string.IsNullOrEmpty(spellCrop))
            {
                spellSource = SpellDatabase.Instance.FirstOrDefault(ss => ss.Name.Equals(spellCrop, StringComparison.OrdinalIgnoreCase));
                if (spellSource == null && spellCrop.Contains(' '))
                {
                    spellCrop = spellCrop.Replace(spellCrop.Split(' ')[0], "").Trim();
                }
                else if (spellSource == null)
                {
                    spellCrop = null;
                }
            }
            if (spellSource == null)
            {
                stringBuilder.Append($" --set spelldesc {OneLineString(spell.Description)}");
            }
            WriteNpcSpellInfo(stringBuilder, character, spellSource, 1);
            return stringBuilder.ToString();
        }

        private static void WriteNpcSpellInfo(StringBuilder stringBuilder, HeroLabCharacter character, SpellSource spell, int casterLevel)
        {
            //Search for the like spell to attach all of the information from.
            //Thanks, HeroLab for not exporting all of the information for Spell-Like Abilities properly.
            if (spell != null)
            {
                if (!string.IsNullOrEmpty(spell.School)) stringBuilder.Append($" --set spellschool {spell.School}"); //spellschool
                if (!string.IsNullOrEmpty(spell.SpellLevel)) stringBuilder.Append($" --set spellclasslevel {spell.SpellLevel}"); //spellclasslevel
                if (!string.IsNullOrEmpty(spell.CastingTime)) stringBuilder.Append($" --set spellcastingtime {spell.CastingTime}"); //spellcastingtime
                if (!string.IsNullOrEmpty(spell.Components)) stringBuilder.Append($" --set spellcomponent {spell.Components}"); //spellcomponent
                SetNpcSpellRange(stringBuilder, spell.Range, casterLevel); //spellrange
                SetNpcSpellArea(stringBuilder, spell.Area, casterLevel); //spellarea
                SetNpcSpellTarget(stringBuilder, spell.Targets, casterLevel); //spelltargets
                SetNpcSpellEffect(stringBuilder, spell.Effect, casterLevel); //spelleffect
                SetNpcSpellDuration(stringBuilder, spell.Duration, casterLevel); //spellduration
                SetNpcSpellSavingThrow(stringBuilder, character, spell.SavingThrow, spell.SlaLevel); //spellsaveflag; spellsave, spelldc_mod
                SetNpcSpellSpellResistance(stringBuilder, character, spell.SpellResistence); //spellresistanceflag; spellresistance
                SetNpcSpellAttack(stringBuilder, character, spell.Description); //spellatkflag; spellatkmod, spellatkcritrange, spelldmgcritmulti
                //SpellDamage isn't supported... unfortunately.
                //spelldmgflag
                // spelldmg
                // spelldmgtype
                //spelldmgflag2
                // spelldmg2
                // spelldmgtype2
                stringBuilder.Append(" --set spelldescflag {{descflag=1}}"); //spelldescflag
                stringBuilder.Append($" --set spelldesc {spell.Description.Replace('\r', ' ').Replace('\n', ' ')}"); //spelldesc

                SetNpcSpellRollContent(stringBuilder, spell);
            }
        }

        private static void SetNpcSpellRollContent(StringBuilder stringBuilder, SpellSource spell)
        {
            var spellRollContent = "@{whispertype} &{template:npc}{{name=@{spellname}}}{{type=spell}}{{showchar=@{rollshowchar}}}{{charname=@{character_name}}}{{nonlethal=[[1[Nonlethal]]]}}{{level=2}}{{school=@{spellschool}}}{{component=@{spellcomponent}}}";
            if (!string.IsNullOrEmpty(spell.Range)) spellRollContent += "{{range=@{spellrange}}}";
            if (!string.IsNullOrEmpty(spell.Area)) spellRollContent += "{{area=@{spellarea}}}";
            if (!string.IsNullOrEmpty(spell.Targets)) spellRollContent += "{{targets=@{spelltargets}}}";
            if (!string.IsNullOrEmpty(spell.Effect)) spellRollContent += "{{effect=@{spelleffect}}}";
            if (!string.IsNullOrEmpty(spell.Duration)) spellRollContent += "{{duration=@{spellduration}}}";
            if (!string.IsNullOrEmpty(spell.SpellResistence) && !spell.SpellResistence.Equals("no", StringComparison.OrdinalIgnoreCase) && !spell.SpellResistence.Equals("none", StringComparison.OrdinalIgnoreCase)) spellRollContent += "{{sr=1}} {{spellresistance=@{spellresistance}}}";
            if (!string.IsNullOrEmpty(spell.SavingThrow) && !spell.SavingThrow.Equals("no", StringComparison.OrdinalIgnoreCase) && !spell.SavingThrow.Equals("none", StringComparison.OrdinalIgnoreCase)) spellRollContent += "{{save=1}} {{savedc=@{spelldc_mod}}}{{saveeffect=@{spellsave}}}";
            spellRollContent += "{{descflag=[[1]]}} {{desc=@{spelldesc}}}";
            //if I ever get to building the attack query.
            //{{attack=1}}{{roll=[[1d20cs>20+0[]+5[MOD]+(@{attack_bonus})[TEMP]+(@{attack_condition})[CONDITION]+@{rollmod_attack}[QUERY]]]}}{{critconfirm=[[1d20cs20+0[]+5[MOD]+(@{attack_bonus})[TEMP]+(@{attack_condition})[CONDITION]+@{rollmod_attack}[QUERY]]]}}{{damage=1}} {{dmg1flag=1}} {{dmg1=[[4d6 + @{damage_bonus}[TEMP] + @{rollmod_damage}[QUERY]]]}} {{dmg1type=Fire}}{{dmg1crit=[[(4d6 + 4d6) + (@{rollmod_damage} 2)[QUERY]]]}}{{dmg2name=Damage2}}{{damage=1}} {{dmg2flag=1}} {{dmg2=[[1d4 + @{damage_bonus}[TEMP] + @{rollmod_damage}[QUERY]]]}} {{dmg2type=Force}}{{dmg2crit=[[(1d4 + 1d4) + (@{rollmod_damage} 2)[QUERY]]]}}

            stringBuilder.Append(" --set rollcontent " + PreventRollingString(spellRollContent));
        }

        private static void SetNpcSpellAttack(StringBuilder stringBuilder, HeroLabCharacter character, string description)
        {
            if (!description.ToLower().Contains("touch attack")) return;
            bool isRanged = description.ToLower().Contains("ranged touch attack");
            int mod = 0;
            int critRange = 20;
            if (isRanged)
            {
                if (character.Attack.RangedAttack.Contains("/"))
                    mod += GetValue(character.Attack.RangedAttack.Split('/')[0]);
                else
                    mod += GetValue(character.Attack.RangedAttack);
                if (character.Feats?.Items?.Any(f => f.Name.Equals("Improved Critical (ray)", StringComparison.OrdinalIgnoreCase)) == true)
                    critRange = 19;
                if (character.Feats?.Items?.Any(f => f.Name.Equals("Weapon Focus (ray)", StringComparison.OrdinalIgnoreCase)) == true)
                    mod += 1;
            }
            else
            {
                if (character.Attack.MeleeAttack.Contains("/"))
                    mod += GetValue(character.Attack.MeleeAttack.Split('/')[0]);
                else
                    mod += GetValue(character.Attack.MeleeAttack);
                if (character.Feats?.Items?.Any(f => f.Name.Equals("Weapon Focus (touch)", StringComparison.OrdinalIgnoreCase)) == true
                    || character.Feats?.Items?.Any(f => f.Name.Equals("Weapon Focus (touch attack)", StringComparison.OrdinalIgnoreCase)) == true)
                    mod += 1;

                if (character.Feats?.Items?.Any(f => f.Name.Equals("Improved Critical (touch)", StringComparison.OrdinalIgnoreCase)) == true
                || character.Feats?.Items?.Any(f => f.Name.Equals("Improved Critical (touch attack)", StringComparison.OrdinalIgnoreCase)) == true)
                    critRange = 19;
            }
            stringBuilder.Append(PreventRollingString(" --set spellatkflag {{attack=1}}"));
            stringBuilder.Append($" --set spellatkmod {mod}");
            stringBuilder.Append($" --set spellatkcritrange {critRange}");
            stringBuilder.Append(" --set spelldmgcritmulti 2");
        }

        private static void SetNpcSpellSpellResistance(StringBuilder stringBuilder, HeroLabCharacter character, string spellResistence)
        {
            if (string.IsNullOrEmpty(spellResistence)) return;
            if (spellResistence.ToLower() == "no" || spellResistence.ToLower() == "none") return;
            stringBuilder.Append(PreventRollingString(" --set spellresistanceflag {{sr=1}}"));
            var hd = character.Health.HitDice.Split('d')[0];
            if (character.Health.HitDice.Contains("HD"))
            {
                hd = character.Health.HitDice.Split(' ')[0];
            }
            var sr = int.Parse(hd);
            if (character.SpellClasses?.Items?.Any() == true)
            {
                int baseDCSpell = 0;
                foreach (var cl in character.SpellClasses.Items)
                {
                    var characterClass = character.Classes.Classes.FirstOrDefault(c => c.Name == cl.Name);
                    if (GetValue(characterClass.OvercomeSpellResistance) > baseDCSpell)
                    {
                        baseDCSpell = GetValue(characterClass.OvercomeSpellResistance);
                    }
                }
                if (baseDCSpell > 0) { sr = baseDCSpell; }
            }
            stringBuilder.Append(PreventRollingString($" --set spellresistance {spellResistence} (Roll [[d20+{sr}]])"));
        }

        private static void SetNpcSpellSavingThrow(StringBuilder stringBuilder, HeroLabCharacter character, string savingThrow, string slaLevel)
        {
            if (string.IsNullOrEmpty(savingThrow)) return;
            if (savingThrow.ToLower() == "none" || savingThrow.ToLower() == "no") return;
            stringBuilder.Append(PreventRollingString(" --set spellsaveflag {{save=1}}"));
            stringBuilder.Append($" --set spellsave {savingThrow}");
            var hd = character.Health.HitDice.Split('d')[0];
            if (character.Health.HitDice.Contains("HD"))
            {
                hd = character.Health.HitDice.Split(' ')[0];
            }
            var charisma = GetValue(character.Attributes.Items.First(a => a.Name == "Charisma").Bonus.Modified);
            var baseDC = 10 + Math.Floor((decimal)int.Parse(hd) / 2) + charisma;
            if (character.SpellClasses?.Items?.Any() == true)
            {
                int baseDCSpell = 0;
                foreach (var cl in character.SpellClasses.Items)
                {
                    var characterClass = character.Classes.Classes.FirstOrDefault(c => c.Name == cl.Name);
                    if (int.Parse(characterClass.BaseSpellDifficultyClass) > baseDCSpell)
                    {
                        baseDCSpell = int.Parse(characterClass.BaseSpellDifficultyClass);
                    }
                }
                if (baseDCSpell > 0) { baseDC = baseDCSpell; }
            }
            stringBuilder.Append($" --set spelldc_mod {baseDC + int.Parse(slaLevel)}");
        }

        private static void SetNpcSpellEffect(StringBuilder stringBuilder, string spellEffect, int casterLevel)
        {
            if (string.IsNullOrEmpty(spellEffect)) return;
            switch (spellEffect)
            {
                case "1 comet per 4 levels": stringBuilder.Append(PreventRollingString(" --set spelleffect [[{1,floor((@{caster" + casterLevel + "_level}+0)/4)}kh1]] comets")); break;
                case "1 dose of a drug/3 levels": stringBuilder.Append(PreventRollingString(" --set spelleffect [[{1,floor((@{caster" + casterLevel + "_level}+0)/3)}kh1]] doses of a drug")); break;
                case "1 hippocampus plus 1 hippocampus/3 caster levels": stringBuilder.Append(PreventRollingString(" --set spelleffect [[{1,floor((@{caster" + casterLevel + "_level}+0)/3)}kh1+1]] hippocampus")); break;
                case "1 ice spear/4 levels": stringBuilder.Append(PreventRollingString(" --set spelleffect [[{1,floor((@{caster" + casterLevel + "_level}+0)/4)}kh1]] ice spears")); break;
                case "1 legendary character/4 caster levels": stringBuilder.Append(PreventRollingString(" --set spelleffect [[{1,floor((@{caster" + casterLevel + "_level}+0)/4)}kh1]] legendary characters")); break;
                case "10-ft.-by-10-ft. hole, 10 ft. deep/2 levels": stringBuilder.Append(PreventRollingString(" --set spelleffect 10-ft.-by-10-ft. hole, [[{1,floor((@{caster" + casterLevel + "_level}+0)/2)}kh1*10]] deep")); break;
                case "10-ft.-deep wave 10 ft. wide/level and 2 ft. tall/level": stringBuilder.Append(PreventRollingString(" --set spelleffect 10-ft. deep, [[@{caster" + casterLevel + "_level}*10]] wide, [[@{caster" + casterLevel + "_level}*2]] tall wave")); break;
                case "10-ft.-high earthen wall, in a line up to 10 ft. long/2 levels, or a circle with radius of up to 3 ft. + 1 ft./level": stringBuilder.Append(PreventRollingString(" --set spelleffect 10-ft.-high earthen wall, in a line up to [[{1,floor((@{caster" + casterLevel + "_level}+0)/2)}kh1*10]] ft. long, or a circle with a radius up to [[@{caster" + casterLevel + "_level}+3]] ft.")); break;
                case "10-ft.-high vertical sheet of illumination up to 5 ft. long/level": stringBuilder.Append(PreventRollingString(" --set spelleffect 10-ft.-high vertical sheet of illumination up to [[@{caster" + casterLevel + "_level}*5]] ft. long")); break;
                case "10-ft.-high vertical sheet of light up to 5 ft. long/level": stringBuilder.Append(PreventRollingString(" --set spelleffect 10-ft.-high vertical sheet of light up to [[@{caster" + casterLevel + "_level}*5]] ft. long")); break;
                case "1-ft.-diameter/level sphere, centered around a creature": stringBuilder.Append(PreventRollingString(" --set spelleffect [[@{caster" + casterLevel + "_level}]]-ft.diameter sphere, centered around a creature")); break;
                case "1-ft.-diameter/level sphere, centered around creatures or objects": stringBuilder.Append(PreventRollingString(" --set spelleffect [[@{caster" + casterLevel + "_level}]]-ft.diameter sphere, centered around creatures or objects")); break;
                case "20-ft.-by-20-ft. hole, 10 ft. deep/4 levels": stringBuilder.Append(PreventRollingString(" --set spelleffect 20-ft.-by-20-ft. hole, [[{1,floor((@{caster" + casterLevel + "_level}+0)/4)}kh1*10]] ft. deep")); break;
                case "20-ft.-high wall of energy whose area is up to one 10-ft. square/level": stringBuilder.Append(PreventRollingString(" --set spelleffect 20-ft.-high wall or energy whose area is up to [[@{caster" + casterLevel + "_level}]] 10-ft. squares")); break;
                case "5-ft.-by-8-ft. opening, 10 ft. deep plus 5 ft. deep per three additional levels": stringBuilder.Append(PreventRollingString(" --set spelleffect 5-ft.-by-8-ft. opening, [[10+({1,floor((@{caster" + casterLevel + "_level}+0)/3)}kh1*5)]] deep")); break;
                case "5-ft.-wide, 60-ft.-deep extradimensional hole, up to 5 ft. long per level (S)": stringBuilder.Append(PreventRollingString(" --set spelleffect 5-ft.-wide, 60-ft.-deep extradimensional hole, up to [[@{caster" + casterLevel + "_level}*5]] ft. long (S)")); break;
                case "a 5-foot-radius beanstalk that grows to a height of 50 ft./caster level": stringBuilder.Append(PreventRollingString(" --set spelleffect a 5-ft.-radius beanstalk that grows to a height of [[@{caster" + casterLevel + "_level}*50]]")); break;
                case "a low wall 10 feet long per 3 levels (minimum 10 feet) (S)": stringBuilder.Append(PreventRollingString(" --set spelleffect a low wall up to [[{1,floor((@{caster" + casterLevel + "_level}+0)/3)}kh1*10]] ft. long, minimum 10 feet (S)")); break;
                case "a wall of corpses with an area of up to one 5-ft. square/ level (S)": stringBuilder.Append(PreventRollingString(" --set spelleffect a wall of corpses with an area of up to [[@{caster" + casterLevel + "_level}]] 5-ft. squares (S)")); break;
                case "an ironwood object weighing up to 5 lbs./level": stringBuilder.Append(PreventRollingString(" --set spelleffect an ironwood object weighing up to [[@{caster" + casterLevel + "_level}*5]] lbs.")); break;
                case "anchored plane of ice, up to one 10-ft. square/level": stringBuilder.Append(PreventRollingString(" --set spelleffect anchored plane of ice, up to [[@{caster" + casterLevel + "_level}]] 10-ft. squares")); break;
                case "anchored plane of ice, up to one 10-ft. square/level, or hemisphere of ice with a radius of up to 3 ft. + 1 ft./level": stringBuilder.Append(PreventRollingString(" --set spelleffect anchored plane of ice, up to [[@{caster" + casterLevel + "_level}]] 10-ft. squares, or a hemisphere of ice with a radius of [[@{caster" + casterLevel + "_level}+3]]")); break;
                case "anti-magic wall occupying up to two 5 ft. cubes/level (S)": stringBuilder.Append(PreventRollingString(" --set spelleffect anti-magic wall occupying up to [[@{caster" + casterLevel + "_level}*2]] 5-ft. cubes")); break;
                case "ethereal 5-ft.-by-8-ft. opening, 10 ft. deep + 5 ft. deep per three levels": stringBuilder.Append(PreventRollingString(" --set spelleffect ethereal 5-ft.-by-8-ft. opening, [[10+({1,floor((@{caster" + casterLevel + "_level}+0)/3)}kh1*5)]] ft. deep")); break;
                case "extradimensional demiplane, up to 10 10-ft. cubes/level (S)": stringBuilder.Append(PreventRollingString(" --set spelleffect extradimensional demiplane, up to [[@{caster" + casterLevel + "_level}*10]] 10-ft. cubes (S)")); break;
                case "extradimensional demiplane, up to 20 10-ft. cubes/level (S)": stringBuilder.Append(PreventRollingString(" --set spelleffect extradimensional demiplane, up to [[@{caster" + casterLevel + "_level}*20]] 10-ft. cubes (S)")); break;
                case "extradimensional demiplane, up to three 10-ft. cubes/level (S)": stringBuilder.Append(PreventRollingString(" --set spelleffect extradimensional demiplane, up to [[@{caster" + casterLevel + "_level}*3]] 10-ft. cubes (S)")); break;
                case "extradimensional mansion, up to three 10-ft. cubes/level (S)": stringBuilder.Append(PreventRollingString(" --set spelleffect extradimensional mansion, up to [[@{caster" + casterLevel + "_level}*3]] 10-ft. cubes (S)")); break;
                case "extradimensional meadow, up to three 10-ft. cubes/level (S)": stringBuilder.Append(PreventRollingString(" --set spelleffect extradimensional meadow, up to [[@{caster" + casterLevel + "_level}*20]] 10-ft. cubes (S)")); break;
                case "extradimensional space up to 1 cu. ft./level": stringBuilder.Append(PreventRollingString(" --set spelleffect extradimensional space, up to [[@{caster" + casterLevel + "_level}]] cu.ft.")); break;
                case "feast for one creature/level": stringBuilder.Append(PreventRollingString(" --set spelleffect feast for [[@{caster" + casterLevel + "_level}]] creatures")); break;
                case "feast for two creatures/level": stringBuilder.Append(PreventRollingString(" --set spelleffect feast for [[@{caster" + casterLevel + "_level}*2]] creatures")); break;
                case "figment that cannot extend beyond a 20-ft. cube + one 10-ft. cube/level (S)": stringBuilder.Append(PreventRollingString(" --set spelleffect figment that cannot extend beyond a 20-ft. cube + [[@{caster" + casterLevel + "_level}]] 10-ft. cube (S)")); break;
                case "five 5-ft. cubes of temporal possibility plus one additional cube/level": stringBuilder.Append(PreventRollingString(" --set spelleffect [[5+@{caster" + casterLevel + "_level}]] 5-ft. cubes of temporal possibilities")); break;
                case "food and water to sustain three humans or one horse/level for 24 hours": stringBuilder.Append(PreventRollingString(" --set spelleffect food and water to sustain [[@{caster" + casterLevel + "_level}*3]] humans or [[@{caster" + casterLevel + "_level}]] horses for 24 hours")); break;
                case "hemisphere that cannot extend beyond four 10-ft. cubes + one 10-ft. cube/level (S)": stringBuilder.Append(PreventRollingString(" --set spelleffect hemisphere that cannot extend beyond [[4+@{caster" + casterLevel + "_level}]] 10-ft. cubes (S)")); break;
                case "illusory, unattended, nonmagical object of nonliving plant matter, up to 1 cu. ft./level": stringBuilder.Append(PreventRollingString(" --set spelleffect illusory, unattended, nonmagical object of nonliving plant mater, up to [[@{caster" + casterLevel + "_level}]] cu.ft.")); break;
                case "illusory, unattended, nonmagical object, up to 1 cu. ft./level": stringBuilder.Append(PreventRollingString(" --set spelleffect illusory, unattended, nonmagical object, up to [[@{caster" + casterLevel + "_level}]] cu.ft.")); break;
                case "iron wall whose area is up to one 5-ft. square/level; see text": stringBuilder.Append(PreventRollingString(" --set spelleffect iron wall whose area is up to [[@{caster" + casterLevel + "_level}]] 5-ft. squares; see text")); break;
                case "mobile 10-ft.-by-10-ft. hole, 10 ft. deep/2 levels": stringBuilder.Append(PreventRollingString(" --set spelleffect mobile 10-ft.-by-10-ft. hole, [[{1,floor((@{caster" + casterLevel + "_level}+0)/2)}kh1*10]] ft. deep")); break;
                case "one 5-ft. cube of temporal possibility/2 levels": stringBuilder.Append(PreventRollingString(" --set spelleffect [[{1,floor((@{caster" + casterLevel + "_level}+0)/2)}kh1]] 5-ft. cubes of temporal possibility")); break;
                case "one animal whose CR is equal or less than your caster level": stringBuilder.Append(PreventRollingString(" --set spelleffect one animal CR equal to or less than [[@{caster" + casterLevel + "_level}]]")); break;
                case "one insect scout/4 levels": stringBuilder.Append(PreventRollingString(" --set spelleffect [[{1,floor((@{caster" + casterLevel + "_level}+0)/4)}kh1]] insect scouts")); break;
                case "one invisible sailor per level": stringBuilder.Append(PreventRollingString(" --set spelleffect [[@{caster" + casterLevel + "_level}]] invisible sailors")); break;
                case "one poisonous mushroom/level, no two of which can be more than 30 ft. apart": stringBuilder.Append(PreventRollingString(" --set spelleffect [[@{caster" + casterLevel + "_level}]] poisonous mushrooms, no two of which can be more than 30 ft. apart")); break;
                case "one summoned petitioner/caster level": stringBuilder.Append(PreventRollingString(" --set spelleffect [[@{caster" + casterLevel + "_level}]] summoned petitioner")); break;
                case "one swarm of wasps per three levels, each of which must be adjacent to at least one other swarm": stringBuilder.Append(PreventRollingString(" --set spelleffect [[{1,floor((@{caster" + casterLevel + "_level}+0)/3)}kh1]] swarms of wasps, each of which must be adjacent to at least one other swarm")); break;
                case "opaque sheet of ectoplasm up to 10 ft. square/level or a sphere or hemisphere with a radius of up to 1 ft./level": stringBuilder.Append(PreventRollingString(" --set spelleffect opaque sheet of ectoplasm up to [[@{caster" + casterLevel + "_level}*10]] ft. square, or a sphere or hemisphere with a radius of up to [[@{caster" + casterLevel + "_level}]] ft.")); break;
                case "opaque sheet of flame up to 20 ft. long/level or a ring of fire with a radius of up to 5 ft./two levels; either form 20 ft. high": stringBuilder.Append(PreventRollingString(" --set spelleffect opaque sheet of flame up to [[@{caster" + casterLevel + "_level}*20]] ft. long, or a ring of fire with a radius of up to [[{1,floor((@{caster" + casterLevel + "_level}+0)/2)}kh1*5]] ft.; either form 20 ft. high")); break;
                case "opulent mansion, up to 300 feet on a side and one story tall/4 levels": stringBuilder.Append(PreventRollingString(" --set spelleffect opulent mansion, up to 300 feet on a side and [[{1,floor((@{caster" + casterLevel + "_level}+0)/4)}kh1]] stories tall")); break;
                case "raft large enough for caster and one passenger/2 levels": stringBuilder.Append(PreventRollingString(" --set spelleffect raft large enough for caster and [[{1,floor((@{caster" + casterLevel + "_level}+0)/2)}kh1]] passengers")); break;
                case "solid wall of humanoid bones with an area of up to one 5-ft. square/level": stringBuilder.Append(PreventRollingString(" --set spelleffect solid wall of humanoid bones with an area of up to [[@{caster" + casterLevel + "_level}]] 5-ft. squares")); break;
                case "spout of boiling water filling a 5 ft. square and spraying upward 10 ft./2 levels": stringBuilder.Append(PreventRollingString(" --set spelleffect spout of boiling water filling a 5 ft. square and spraying upward [[{1,floor((@{caster" + casterLevel + "_level}+0)/2)}kh1*10]] ft.")); break;
                case "stone wall whose area is up to one 5-ft. square/level (S)": stringBuilder.Append(PreventRollingString(" --set spelleffect stone wall whose area is up to [[@{caster" + casterLevel + "_level}]] 5-ft. squares (S)")); break;
                case "translucent wall 20 ft. long/level or a translucent ring with a radius of up to 5 ft./two levels; either form 20 ft. high": stringBuilder.Append(PreventRollingString(" --set spelleffect translucent wall [[@{caster" + casterLevel + "_level}*20]] ft. long, or a translucent ring with a radius of up to [[{1,floor((@{caster" + casterLevel + "_level}+0)/2)}kh1]] ft.; either form 20 ft. high")); break;
                case "translucent wall of sound up to 20 ft. long/level or a ring of sound with a radius of up to 5 ft./two levels; either form 20 ft. high": stringBuilder.Append(PreventRollingString(" --set spelleffect translucent wall of sound up to [[@{caster" + casterLevel + "_level}*20]] ft. long, or a ring of sound with a radius of up to [[{1,floor((@{caster" + casterLevel + "_level}+0)/2)}kh1]] ft.; either form 20 ft. high")); break;
                case "transparent wall 20 ft. high by up to 20 ft. long/level": stringBuilder.Append(PreventRollingString(" --set spelleffect transparent wall 20 ft. high by up to [[@{caster" + casterLevel + "_level}*20]] ft. long")); break;
                case "transparent wall whose area is up to one 10-ft. square/level": stringBuilder.Append(PreventRollingString(" --set spelleffect transparent wall whose area is up to [[@{caster" + casterLevel + "_level}]] 20-ft. squares")); break;
                case "unattended, nonmagical object of nonliving plant matter, up to 1 cu. ft./level": stringBuilder.Append(PreventRollingString(" --set spelleffect unattended, nonmagical object of nonliving plant matter, up to [[@{caster" + casterLevel + "_level}]] cu.ft.")); break;
                case "up to 1 draft of the waters of Lamashtu per 2 levels": stringBuilder.Append(PreventRollingString(" --set spelleffect up to [[{1,floor((@{caster" + casterLevel + "_level}+0)/2)}kh1]] drafts of the waters of Lamashtu")); break;
                case "up to 1 flask of the waters of Lamashtu per 2 levels": stringBuilder.Append(PreventRollingString(" --set spelleffect up to [[{1,floor((@{caster" + casterLevel + "_level}+0)/2)}kh1]] flasks of the waters of Lamashtu")); break;
                case "up to 2 gallons of water/level": stringBuilder.Append(PreventRollingString(" --set spelleffect up to [[@{caster" + casterLevel + "_level}*2]] gallons of water")); break;
                case "up to one insect spy/4 levels": stringBuilder.Append(PreventRollingString(" --set spelleffect up to [[{1,floor((@{caster" + casterLevel + "_level}+0)/4)}kh1]] insect spies")); break;
                case "up to one sword per 3 levels": stringBuilder.Append(PreventRollingString(" --set spelleffect up to [[{1,floor((@{caster" + casterLevel + "_level}+0)/3)}kh1]] swords")); break;
                case "up to three 10-ft. cubes/level (S)": stringBuilder.Append(PreventRollingString(" --set spelleffect up to [[@{caster" + casterLevel + "_level}*3]] 10-ft. cubes (S)")); break;
                case "visual figment that cannot extend beyond a 20-ft. cube + one 10-ft. cube/level (S)": stringBuilder.Append(PreventRollingString(" --set spelleffect visual figment that cannot extend beyond a 20-ft. cube and [[@{caster" + casterLevel + "_level}*10]] 10-ft. cubes (S)")); break;
                case "visual figment that cannot extend beyond four 10-ft. cubes + one 10-ft. cube/level (S)": stringBuilder.Append(PreventRollingString(" --set spelleffect visual figment that cannot extend beyond a 20-ft. cube and [[@{caster" + casterLevel + "_level}*10]] 10-ft. cubes (S)")); break;
                case "wall 4 ft./level wide, 2 ft./level high": stringBuilder.Append(PreventRollingString(" --set spelleffect wall [[4*@{caster" + casterLevel + "_level}]] wide, [[2*@{caster" + casterLevel + "_level}]] high")); break;
                case "wall of blades up to 10 ft. long/level and 10 ft. tall": stringBuilder.Append(PreventRollingString(" --set spelleffect wall of blades up to [[10*@{caster" + casterLevel + "_level}]] ft. long and 10 ft. tall")); break;
                case "wall of bronze clockworks whose area is up to one 5-ft.-square/level (S)": stringBuilder.Append(PreventRollingString(" --set spelleffect wall of bronze clockworks whose area is up to [[@{caster" + casterLevel + "_level}]] 5-ft.-squares (S)")); break;
                case "wall of thorny brush, up to one 10-ft. cube/level (S)": stringBuilder.Append(PreventRollingString(" --set spelleffect wall of thorny brush, up to [[@{caster" + casterLevel + "_level}]] 10-ft. cubes (S)")); break;
                case "wall of whirling blades up to 20 ft. long/level, or a ringed wall of whirling blades with a radius of up to 5 ft. per two levels; either form is 20 ft. high": stringBuilder.Append(PreventRollingString(" --set spelleffect wall of whirling blades up to [[20*@{caster" + casterLevel + "_level}]], or a ringed wall of whirling blades with a radius of up to [[{1,floor((@{caster" + casterLevel + "_level}+0)/2)}kh1*5]] ft.; either form is 20 ft. high")); break;
                case "wall up to 10 ft./level long and 5 ft./level high (S)": stringBuilder.Append(PreventRollingString(" --set spelleffect wall up to [[10*@{caster" + casterLevel + "_level}]] ft. long and [[5*@{caster" + casterLevel + "_level}]] ft. high (S)")); break;
                case "wall whose area is up to one 10-ft. square/level": stringBuilder.Append(PreventRollingString(" --set spelleffect wall whose area is up to [[@{caster" + casterLevel + "_level}]] 10-ft. squares")); break;
                case "you plus up to one willing creature per 3 levels": stringBuilder.Append(PreventRollingString(" --set spelleffect you plus up to [[{1,floor((@{caster" + casterLevel + "_level}+0)/3)}kh1]] willing creatures")); break;
                default: stringBuilder.Append(PreventRollingString($" --set spelleffect {spellEffect}")); break;
            }
        }

        private static void SetNpcSpellDuration(StringBuilder stringBuilder, string spellDuration, int casterLevel)
        {
            if (string.IsNullOrEmpty(spellDuration)) return;
            switch (spellDuration)
            {
                case "1 day/level":
                    stringBuilder.Append(PreventRollingString(" --set spellduration [[@{caster" + casterLevel + "_level}]] days"));
                    break;
                case "1 day/level (D)":
                    stringBuilder.Append(PreventRollingString(" --set spellduration [[@{caster" + casterLevel + "_level}]] days (D)"));
                    break;
                case "1 day/level or until discharged (see text)":
                    stringBuilder.Append(PreventRollingString(" --set spellduration [[@{caster" + casterLevel + "_level}]] days or until discharged (see text)"));
                    break;
                case "1 day/level or until fulfilled":
                    stringBuilder.Append(PreventRollingString(" --set spellduration [[@{caster" + casterLevel + "_level}]] days or until fulfilled"));
                    break;
                case "1 day/level; see text":
                    stringBuilder.Append(PreventRollingString(" --set spellduration [[@{caster" + casterLevel + "_level}]] days; see text"));
                    break;
                case "1 hour/level":
                    stringBuilder.Append(PreventRollingString(" --set spellduration [[@{caster" + casterLevel + "_level}]] hours"));
                    break;
                case "1 hour/level (D)":
                    stringBuilder.Append(PreventRollingString(" --set spellduration [[@{caster" + casterLevel + "_level}]] hours (D)"));
                    break;
                case "1 hour/level ; see text":
                    stringBuilder.Append(PreventRollingString(" --set spellduration [[@{caster" + casterLevel + "_level}]] hours; see text"));
                    break;
                case "1 hour/level or 1 day/level (see text)":
                    stringBuilder.Append(PreventRollingString(
                        " --set spellduration [[@{caster" + casterLevel + "_level}]] hours or [[@{caster" + casterLevel + "_level}]] days; see text"));
                    break;
                case "1 hour/level or instantaneous (see text)":
                    stringBuilder.Append(PreventRollingString(" --set spellduration [[@{caster" + casterLevel + "_level}]] hours or instantaneous; see text"));
                    break;
                case "1 hour/level or until completed":
                    stringBuilder.Append(PreventRollingString(" --set spellduration [[@{caster" + casterLevel + "_level}]] hours or until completed"));
                    break;
                case "1 min./level":
                    stringBuilder.Append(PreventRollingString(" --set spellduration [[@{caster" + casterLevel + "_level}]] minutes"));
                    break;
                case "1 min./level (D)":
                    stringBuilder.Append(PreventRollingString(" --set spellduration [[@{caster" + casterLevel + "_level}]] minutes (D)"));
                    break;
                case "1 minute/level":
                    stringBuilder.Append(PreventRollingString(" --set spellduration [[@{caster" + casterLevel + "_level}]] minutes"));
                    break;
                case "1 minute/level (D)":
                    stringBuilder.Append(PreventRollingString(" --set spellduration [[@{caster" + casterLevel + "_level}]] minutes (D)"));
                    break;
                case "1 minute/level (D), special see below":
                    stringBuilder.Append(PreventRollingString(" --set spellduration [[@{caster" + casterLevel + "_level}]] minutes (D)); special, see text"));
                    break;
                case "1 minute/level (see text)":
                    stringBuilder.Append(PreventRollingString(" --set spellduration [[@{caster" + casterLevel + "_level}]] minutes; see text"));
                    break;
                case "1 minute/level; see text":
                    stringBuilder.Append(PreventRollingString(" --set spellduration [[@{caster" + casterLevel + "_level}]] minutes; see text"));
                    break;
                case "1 round/3 levels":
                    stringBuilder.Append(PreventRollingString(" --set spellduration [[{1,floor((@{caster" + casterLevel + "_level}+0)/3)}kh1]] rounds"));
                    break;
                case "1 round/level":
                    stringBuilder.Append(PreventRollingString(" --set spellduration [[@{caster" + casterLevel + "_level}]] rounds"));
                    break;
                case "1 round/level (D)":
                    stringBuilder.Append(PreventRollingString(" --set spellduration [[@{caster" + casterLevel + "_level}]] rounds (D)"));
                    break;
                case "1 round/level (see text)":
                    stringBuilder.Append(PreventRollingString(" --set spellduration [[@{caster" + casterLevel + "_level}]] rounds; see text"));
                    break;
                case "1 round/level or 1 hour/level; see text":
                    stringBuilder.Append(PreventRollingString(
                        " --set spellduration [[@{caster" + casterLevel + "_level}]] rounds or [[@{caster" + casterLevel + "_level}]] hours; see text"));
                    break;
                case "1 round/level or 1 round; see text":
                    stringBuilder.Append(PreventRollingString(" --set spellduration [[@{caster" + casterLevel + "_level}]] rounds or 1 round; see text"));
                    break;
                case "1 round/level or 1 round; see text for cause fear":
                    stringBuilder.Append(PreventRollingString(" --set spellduration [[@{caster" + casterLevel + "_level}]] rounds or 1 round; see text"));
                    break;
                case "1 round/level or permanent; see text":
                    stringBuilder.Append(PreventRollingString(" --set spellduration [[@{caster" + casterLevel + "_level}]] rounds or permanent; see text"));
                    break;
                case "1 round/level or until discharged":
                    stringBuilder.Append(PreventRollingString(" --set spellduration [[@{caster" + casterLevel + "_level}]] rounds or until discharged"));
                    break;
                case "1 round/level or until discharged (D)":
                    stringBuilder.Append(PreventRollingString(" --set spellduration [[@{caster" + casterLevel + "_level}]] rounds or until discharged (D)"));
                    break;
                case "1 round/level; see text":
                    stringBuilder.Append(PreventRollingString(" --set spellduration [[@{caster" + casterLevel + "_level}]] rounds; see text"));
                    break;
                case "10 min./level":
                    stringBuilder.Append(PreventRollingString(" --set spellduration [[@{caster" + casterLevel + "_level}*10]] minutes"));
                    break;
                case "10 min./level (D)":
                    stringBuilder.Append(PreventRollingString(" --set spellduration [[@{caster" + casterLevel + "_level}]] minutes (D)"));
                    break;
                case "10 minutes or concentration (up to 1 round/level)); see text":
                    stringBuilder.Append(PreventRollingString(
                        " --set spellduration 10 minutes or concentration (up to [[@{caster" + casterLevel + "_level}]] rounds)); see text"));
                    break;
                case "10 minutes/level":
                    stringBuilder.Append(PreventRollingString(" --set spellduration [[@{caster" + casterLevel + "_level}*10]] minutes"));
                    break;
                case "10 minutes/level (D)":
                    stringBuilder.Append(PreventRollingString(" --set spellduration [[@{caster" + casterLevel + "_level}*10]] minutes (D)"));
                    break;
                case "10 minutes/level, then 1 hour/level or until completed (D)); see text":
                    stringBuilder.Append(PreventRollingString(
                        " --set spellduration [[@{caster" + casterLevel + "_level}*10]] minutes, then [[@{caster" + casterLevel + "_level}]] hours or until completed (D)); see text"));
                    break;
                case "2 hours/level":
                    stringBuilder.Append(PreventRollingString(" --set spellduration [[@{caster" + casterLevel + "_level}*2]] hours"));
                    break;
                case "2 hours/level; see text":
                    stringBuilder.Append(PreventRollingString(" --set spellduration [[@{caster" + casterLevel + "_level}*2]] hours; see text"));
                    break;
                case "4d4 rounds (D)":
                    stringBuilder.Append(PreventRollingString(" --set spellduration [[4d4]] rounds (D)"));
                    break;
                case "concentration + 1 round/level":
                    stringBuilder.Append(PreventRollingString(" --set spellduration concentration + [[@{caster" + casterLevel + "_level}]] rounds"));
                    break;
                case "concentration + 1 round/level (D)":
                    stringBuilder.Append(PreventRollingString(" --set spellduration concentration + [[@{caster" + casterLevel + "_level}]] rounds (D)"));
                    break;
                case "concentration, up to 1 min./level (D)":
                    stringBuilder.Append(PreventRollingString(" --set spellduration concentration, up to [[@{caster" + casterLevel + "_level}]] minutes (D)"));
                    break;
                case "concentration, up to 1 round/level":
                    stringBuilder.Append(PreventRollingString(" --set spellduration concentration, up to [[@{caster" + casterLevel + "_level}]] rounds"));
                    break;
                case "instantaneous and 1 round/level":
                    stringBuilder.Append(PreventRollingString(" --set spellduration instantaneous and [[@{caster" + casterLevel + "_level}]] rounds"));
                    break;
                case "instantaneous or 1 hour/level (D)":
                    stringBuilder.Append(PreventRollingString(" --set spellduration instantanous or [[@{caster" + casterLevel + "_level}]] hours (D)"));
                    break;
                case "instantaneous or 10 min./level; see text":
                    stringBuilder.Append(PreventRollingString(" --set spellduration instantanous or [[@{caster" + casterLevel + "_level}*10]] minutes; see text"));
                    break;
                case "until landing or 1 round/level":
                    stringBuilder.Append(PreventRollingString(" --set spellduration until landing or [[@{caster" + casterLevel + "_level}]] rounds"));
                    break;
                case "up to 1 round/level (see text)":
                    stringBuilder.Append(PreventRollingString(" --set spellduration up to [[@{caster" + casterLevel + "_level}]] rounds; see text"));
                    break;
                default:
                    stringBuilder.Append(PreventRollingString($" --set spellduration {spellDuration}"));
                    break;
            }
        }

        public static void SetNpcSpellTarget(StringBuilder stringBuilder, string spellTargets, int casterLevel)
        {
            if (string.IsNullOrEmpty(spellTargets)) return;
            //Believe me, this took a while.
            switch (spellTargets)
            {
                case "1 ally/level":
                    stringBuilder.Append(PreventRollingString(" --set spelltargets [[@{caster" + casterLevel + "_level}]] allies"));
                    break;
                case "1 creature/3 levels no two of which can be more than 30 ft. apart":
                    stringBuilder.Append(PreventRollingString(
                        " --set spelltargets [[{1,floor((@{caster" + casterLevel + "_level}+0)/3)}kh1]] allies, no two of which can be more than 30 ft. apart"));
                    break;
                case "1 creature/level":
                    stringBuilder.Append(PreventRollingString(" --set spelltargets [[@{caster" + casterLevel + "_level}]] creatures"));
                    break;
                case "1 creature/level, no two of which can be more than 30 feet apart":
                    stringBuilder.Append(PreventRollingString(
                        " --set spelltargets [[@{caster" + casterLevel + "_level}]] creatures, no two of which can be more than 30 ft. apart"));
                    break;
                case "1 creature/level, no two of which may be more than 30 ft. apart":
                    stringBuilder.Append(PreventRollingString(
                        " --set spelltargets [[@{caster" + casterLevel + "_level}]] creatures, no two of which can be more than 30 ft. apart"));
                    break;
                case "1 creature/level; see text":
                    stringBuilder.Append(PreventRollingString(" --set spelltargets [[@{caster" + casterLevel + "_level}]] creatures; see text"));
                    break;
                case "1 cu. ft./2 levels of liquid (see text)":
                    stringBuilder.Append(PreventRollingString(
                        " --set spelltargets [[{1,floor((@{caster" + casterLevel + "_level}+0)/2)}kh1]] cu. ft. of liquid (see text)"));
                    break;
                case "1 cu. ft./level of contaminated food and water":
                    stringBuilder.Append(PreventRollingString(" --set spelltargets [[@{caster" + casterLevel + "_level}]] cu. ft. of contaminated food and water"));
                    break;
                case "1 cu. ft./level of food and water or one potion; see text":
                    stringBuilder.Append(PreventRollingString(
                        " --set spelltargets [[@{caster" + casterLevel + "_level}]] cu. ft. of contaminated food and water or one potion; see text"));
                    break;
                case "1 gallon of water/level":
                    stringBuilder.Append(PreventRollingString(" --set spelltargets [[@{caster" + casterLevel + "_level}]] gallon of water"));
                    break;
                case "1 improvised weapon/level":
                    stringBuilder.Append(PreventRollingString(" --set spelltargets [[@{caster" + casterLevel + "_level}]] improvised weapons"));
                    break;
                case "1 object of up to 1 cubic ft./level":
                    stringBuilder.Append(PreventRollingString(" --set spelltargets 1 object of up to [[@{caster" + casterLevel + "_level}]] cu. ft."));
                    break;
                case "1 object weighing up to 1 pound/level":
                    stringBuilder.Append(PreventRollingString(" --set spelltargets 1 object weighing up to [[@{caster" + casterLevel + "_level}]] pounds"));
                    break;
                case "1 pint of water/level":
                    stringBuilder.Append(PreventRollingString(" --set spelltargets [[@{caster" + casterLevel + "_level}]] pints of water"));
                    break;
                case "1 potion touched/level":
                    stringBuilder.Append(PreventRollingString(" --set spelltargets [[@{caster" + casterLevel + "_level}]] potions touched"));
                    break;
                case "1 Small wooden object/level, all within a 20-ft. radius":
                    stringBuilder.Append(PreventRollingString(
                        " --set spelltargets [[@{caster" + casterLevel + "_level}]] Small wooden objects, all within a 20-ft. radius; see text"));
                    break;
                case "1 Small wooden object/level, all within a 20-ft. radius; see text":
                    stringBuilder.Append(PreventRollingString(
                        " --set spelltargets [[@{caster" + casterLevel + "_level}]] Small wooden objects, all within a 20-ft. radius"));
                    break;
                case "1 weapon, suit or armor, shield, tool, or skill kit touched/5 levels":
                    stringBuilder.Append(PreventRollingString(
                        " --set spelltargets [[{1,floor((@{caster" + casterLevel + "_level}+0)/5)}kh1]] weapons, suits of armor, shields, tools, or skill kit touched"));
                    break;
                case "5-ft.-wide, 60-ft.-deep extradimensional hole, up to 5  ft. long per level (S)":
                    stringBuilder.Append(PreventRollingString(
                        " --set spelltargets 5-ft.-wide, 60-ft.-deep extradimensional hole up to [[@{caster" + casterLevel + "_level}*5]] ft. long (S)"));
                    break;
                case "a creature or object weighing up to 100 lbs./level":
                    stringBuilder.Append(PreventRollingString(
                        " --set spelltargets a creature or object weighing up to [[@{caster" + casterLevel + "_level}*100]]"));
                    break;
                case "area of river up to 5 ft. wide/2 levels and 10 ft. long/level":
                    stringBuilder.Append(PreventRollingString(
                        " --set spelltargets area of river up to [[({1,floor((@{caster" + casterLevel + "_level}+0)/2)}kh1)*5]] ft. wide and [[@{caster" + casterLevel + "_level}*10]] ft. long"));
                    break;
                case "corpse of creature whose total number of HD does not exceed your caster level":
                    stringBuilder.Append(PreventRollingString(
                        " --set spelltargets corpse of creature whose total number of HD does not exceed [[@{caster" + casterLevel + "_level}]]"));
                    break;
                case "creature one creature/level, no two of which can be more than 30 ft. apart":
                    stringBuilder.Append(PreventRollingString(
                        " --set spelltargets [[@{caster" + casterLevel + "_level}]] creatures, no two of which can be more than 30 ft. apart"));
                    break;
                case "creature or creatures touched (up to one per level)":
                    stringBuilder.Append(PreventRollingString(" --set spelltargets [[@{caster" + casterLevel + "_level}]] creatures touched"));
                    break;
                case "creature or creatures touched (up to one/level)":
                    stringBuilder.Append(PreventRollingString(" --set spelltargets [[@{caster" + casterLevel + "_level}]] creatures touched"));
                    break;
                case "creature or object of up to 1 cu. ft./level touched":
                    stringBuilder.Append(PreventRollingString(
                        " --set spelltargets creature or object of up to [[@{caster" + casterLevel + "_level}]] cu. ft. touched"));
                    break;
                case "creatures touched, up to one/level":
                    stringBuilder.Append(PreventRollingString(" --set spelltargets [[@{caster" + casterLevel + "_level}]] creatures touched"));
                    break;
                case "door, chest, or portal touched, up to 30 sq. ft./level in size":
                    stringBuilder.Append(PreventRollingString(
                        " --set spelltargets door, chest, or portal touched, up to [[@{caster" + casterLevel + "_level}*30]] sq. ft. in size"));
                    break;
                case "lava wall whose area is up to one 5-ft. square/level (S)":
                    stringBuilder.Append(PreventRollingString(
                        " --set spelltargets lava wall whose area is up to [[@{caster" + casterLevel + "_level}]] 5-ft. squares"));
                    break;
                case "living creatures touched (up to one per level)":
                    stringBuilder.Append(PreventRollingString(" --set spelltargets up to [[@{caster" + casterLevel + "_level}]] living creatures touched"));
                    break;
                case
                "metal equipment of one creature per two levels, no two of which can be more than 30 ft. apart; or 25 lbs. of metal/level, all of which must be within a 30-ft. circle"
                :
                    stringBuilder.Append(PreventRollingString(
                        " --set spelltargets metal equipment of [[{1,floor((@{caster" + casterLevel + "_level}+0)/2)}kh1]] creatures, no two of which can be more than 30 ft. apart; or [[@{caster" + casterLevel + "_level}*25]] lbs. of metal, all of which must be within a 30-ft. circle"));
                    break;
                case
                "metal equipment of one creature per two levels, no two of which can be more than 30 ft. apart; or 25 lbs. of metal/level, none of which can be more than 30 ft. away from any of the rest"
                :
                    stringBuilder.Append(PreventRollingString(
                        " --set spelltargets metal equipment of [[{1,floor((@{caster" + casterLevel + "_level}+0)/2)}kh1]] creatures, no two of which can be more than 30 ft. apart; or [[@{caster" + casterLevel + "_level}*25]] lbs. of metal, all of which must be within a 30-ft. circle"));
                    break;
                case "multiple objects totaling up to 1 cubic ft./level, no two of which can be more than 30 ft. apart":
                    stringBuilder.Append(PreventRollingString(
                        " --set spelltargets multiple objects totaling up to [[@{caster" + casterLevel + "_level}]] cu. ft., no two of which can be more than 30 ft. apart"));
                    break;
                case "object touched or up to 5 sq. ft./level":
                    stringBuilder.Append(PreventRollingString(" --set spelltargets object touched, or up to [[@{caster" + casterLevel + "_level}*5]] sq.ft."));
                    break;
                case "object touched, up to 1 cubic foot per level":
                    stringBuilder.Append(PreventRollingString(" --set spelltargets object touched, or up to [[@{caster" + casterLevel + "_level}*1]] cu.ft."));
                    break;
                case "object touched, weighing up to 5 lbs./level":
                    stringBuilder.Append(PreventRollingString(" --set spelltargets object touched, weighing up to [[@{caster" + casterLevel + "_level}*5]] lbs."));
                    break;
                case "one animal/level, no two of which can be more than 30 ft. apart":
                    stringBuilder.Append(PreventRollingString(
                        " --set spelltargets [[@{caster" + casterLevel + "_level}]] animals, no two of which can be more than 30 ft. apart"));
                    break;
                case "one broken object of up to 2 lbs./level":
                    stringBuilder.Append(PreventRollingString(" --set spelltargets one broken object of up to [[@{caster" + casterLevel + "_level}*2]] lbs."));
                    break;
                case "one chamber and up to 10 cu. ft. of goods/caster level":
                    stringBuilder.Append(PreventRollingString(
                        " --set spelltargets one chamber and up to [[@{caster" + casterLevel + "_level}*10]] cu.ft. of goods"));
                    break;
                case "one chest and up to 1 cu. ft. of goods/caster level":
                    stringBuilder.Append(PreventRollingString(" --set spelltargets one chest and up to [[@{caster" + casterLevel + "_level}]] cu.ft. of goods"));
                    break;
                case "one cloud-like effect, up to one 10-ft. cube/level":
                    stringBuilder.Append(PreventRollingString(
                        " --set spelltargets one cloud-like effect, up to [[@{caster" + casterLevel + "_level}]] 10-ft. cubes"));
                    break;
                case "one creature or object/level touched":
                    stringBuilder.Append(PreventRollingString(" --set spelltargets one creature, or [[@{caster" + casterLevel + "_level}]] objects touched"));
                    break;
                case "one creature or object/level, no two of which can be more than 30 ft. apart":
                    stringBuilder.Append(PreventRollingString(
                        " --set spelltargets one creature, or [[@{caster" + casterLevel + "_level}]] objects, no two of which can be more than 30 ft. apart"));
                    break;
                case "one creature or one object up to 5 lbs./level":
                    stringBuilder.Append(PreventRollingString(
                        " --set spelltargets one creature or one object up to [[@{caster" + casterLevel + "_level}*5]] lbs."));
                    break;
                case "one creature per level":
                    stringBuilder.Append(PreventRollingString(" --set spelltargets [[@{caster" + casterLevel + "_level}]] creatures"));
                    break;
                case "one creature per level, no two of which can be more than 30 ft. apart":
                    stringBuilder.Append(PreventRollingString(
                        " --set spelltargets [[@{caster" + casterLevel + "_level}]] creatures, no two of which can be more than 30 ft. apart"));
                    break;
                case "one creature per three levels, no two of which may be more than 30 ft. apart":
                    stringBuilder.Append(PreventRollingString(
                        " --set spelltargets [[{1,floor((@{caster" + casterLevel + "_level}+0)/3)}kh1]] creatures, no two of which may be more than 30 ft. apart"));
                    break;
                case "one creature per two levels, no two of which can be more than 30 ft. apart":
                    stringBuilder.Append(PreventRollingString(
                        " --set spelltargets [[{1,floor((@{caster" + casterLevel + "_level}+0)/2)}kh1]] creatures, no two of which can be more than 30 ft. apart"));
                    break;
                case
                "one creature plus one additional creature per 4 levels, no two of which can be more than 30 ft. apart":
                    stringBuilder.Append(PreventRollingString(
                        " --set spelltargets one creature plus [[{1,floor((@{caster" + casterLevel + "_level}+0)/4)}kh1]] additional creatures, no two of which can be more than 30 ft. apart"));
                    break;
                case
                "one creature plus one additional creature per four levels, no two of which can be more than 30 ft. apart"
                :
                    stringBuilder.Append(PreventRollingString(
                        " --set spelltargets one creature plus [[{1,floor((@{caster" + casterLevel + "_level}+0)/4)}kh1]] additional creatures, no two of which can be more than 30 ft. apart"));
                    break;
                case
                "one creature summoned by a spell or spell-like ability/level, no two of which can be more than 30 ft. apart"
                :
                    stringBuilder.Append(PreventRollingString(
                        " --set spelltargets [[@{caster" + casterLevel + "_level}]] creatures summoned by a spell or spell-like ability, no two of which can be more than 30 ft. apart"));
                    break;
                case "one creature touched/2 levels":
                    stringBuilder.Append(PreventRollingString(" --set spelltargets [[{1,floor((@{caster" + casterLevel + "_level}+0)/2)}kh1]] creatures touched"));
                    break;
                case "one creature touched/level":
                    stringBuilder.Append(PreventRollingString(" --set spelltargets [[@{caster" + casterLevel + "_level}]] creatures touched"));
                    break;
                case "one creature, or one nonmagical object of up to 100 cu. ft./level":
                    stringBuilder.Append(PreventRollingString(
                        " --set spelltargets one creature, or one nonmagical object of up to [[@{caster" + casterLevel + "_level}*100]] cu.ft."));
                    break;
                case "one creature, or one object weighing up to 20 lbs./level":
                    stringBuilder.Append(PreventRollingString(
                        " --set spelltargets one creature, or one object weighing up to [[@{caster" + casterLevel + "_level}*20]] lbs."));
                    break;
                case "one creature, plus one additional creature for every 3  caster levels":
                    stringBuilder.Append(PreventRollingString(
                        " --set spelltargets one creature plus [[{1,floor((@{caster" + casterLevel + "_level}+0)/3)}kh1]] additional creatures"));
                    break;
                case "one creature/2 levels":
                    stringBuilder.Append(PreventRollingString(" --set spelltargets [[{1,floor((@{caster" + casterLevel + "_level}+0)/2)}kh1]] creatures"));
                    break;
                case "one creature/2 levels (no two of which may be more than 30 ft. apart)":
                    stringBuilder.Append(PreventRollingString(
                        " --set spelltargets [[{1,floor((@{caster" + casterLevel + "_level}+0)/2)}kh1]] creatures, no two of which can be more than 30 ft. apart"));
                    break;
                case "one creature/2 levels, no two of which can be more than 30 feet apart":
                    stringBuilder.Append(PreventRollingString(
                        " --set spelltargets [[{1,floor((@{caster" + casterLevel + "_level}+0)/2)}kh1]] creatures, no two of which can be more than 30 ft. apart"));
                    break;
                case "one creature/2 levels, no two of which can be more than 30 ft. apart":
                    stringBuilder.Append(PreventRollingString(
                        " --set spelltargets [[{1,floor((@{caster" + casterLevel + "_level}+0)/2)}kh1]] creatures, no two of which can be more than 30 ft. apart"));
                    break;
                case "one creature/2 levels, no two of which may be more than 30 ft. apart (see text)":
                    stringBuilder.Append(PreventRollingString(
                        " --set spelltargets [[{1,floor((@{caster" + casterLevel + "_level}+0)/2)}kh1]] creatures, no two of which can be more than 30 ft. apart; see text"));
                    break;
                case "one creature/3 levels":
                    stringBuilder.Append(PreventRollingString(" --set spelltargets [[{1,floor((@{caster" + casterLevel + "_level}+0)/3)}kh1]] creatures"));
                    break;
                case "one creature/3 levels, no two of which can be more than 30 ft. apart":
                    stringBuilder.Append(PreventRollingString(
                        " --set spelltargets [[{1,floor((@{caster" + casterLevel + "_level}+0)/3)}kh1]] creatures, no two of which can be more than 30 ft. apart; see text"));
                    break;
                case "one creature/4 levels":
                    stringBuilder.Append(PreventRollingString(
                        " --set spelltargets [[{1,floor((@{caster" + casterLevel + "_level}+0)/4)}kh1]] creatures, no two of which can be more than 30 ft. apart; see text"));
                    break;
                case "one creature/4 levels, no two of which can be more than 30 ft. apart":
                    stringBuilder.Append(PreventRollingString(
                        " --set spelltargets [[{1,floor((@{caster" + casterLevel + "_level}+0)/4)}kh1]] creatures, no two of which can be more than 30 ft. apart; see text"));
                    break;
                case "one creature/level":
                    stringBuilder.Append(PreventRollingString(" --set spelltargets [[@{caster" + casterLevel + "_level}]] creatures"));
                    break;
                case "one creature/level (all of which must be within 30 feet)":
                    stringBuilder.Append(PreventRollingString(
                        " --set spelltargets [[@{caster" + casterLevel + "_level}]] creatures, no two of which can be more than 30 ft. apart"));
                    break;
                case "one creature/level in a 20-ft.-radius burst centered  on you":
                    stringBuilder.Append(PreventRollingString(
                        " --set spelltargets [[@{caster" + casterLevel + "_level}]] creatures in a 20-ft.-radius burst centered on you"));
                    break;
                case "one creature/level in a 20-ft.-radius burst centered on you":
                    stringBuilder.Append(PreventRollingString(
                        " --set spelltargets [[@{caster" + casterLevel + "_level}]] creatures in a 20-ft.-radius burst centered on you"));
                    break;
                case "one creature/level touched":
                    stringBuilder.Append(PreventRollingString(" --set spelltargets [[@{caster" + casterLevel + "_level}]] creatures touched"));
                    break;
                case "one creature/level, no two of which can be more  than 30 ft. apart":
                    stringBuilder.Append(PreventRollingString(
                        " --set spelltargets [[@{caster" + casterLevel + "_level}]] creatures, no two of which can be more than 30 ft. apart"));
                    break;
                case "one creature/level, no two of which can be more than 20 feet apart":
                    stringBuilder.Append(PreventRollingString(
                        " --set spelltargets [[@{caster" + casterLevel + "_level}]] creatures, no two of which can be more than 20 ft. apart"));
                    break;
                case "one creature/level, no two of which can be more than 30 ft. apart":
                    stringBuilder.Append(PreventRollingString(
                        " --set spelltargets [[@{caster" + casterLevel + "_level}]] creatures, no two of which can be more than 30 ft. apart"));
                    break;
                case "one creature/level, no two of which can be more than 30 ft. apart.":
                    stringBuilder.Append(PreventRollingString(
                        " --set spelltargets [[@{caster" + casterLevel + "_level}]] creatures, no two of which can be more than 30 ft. apart"));
                    break;
                case "one creature/level, no two of which may be more than 30 ft. apart":
                    stringBuilder.Append(PreventRollingString(
                        " --set spelltargets [[@{caster" + casterLevel + "_level}]] creatures, no two of which can be more than 30 ft. apart"));
                    break;
                case "one daemon per 4 caster levels":
                    stringBuilder.Append(PreventRollingString(" --set spelltargets [[{1,floor((@{caster" + casterLevel + "_level}+0)/4)}kh1]] daemons"));
                    break;
                case "one destroyed construct of up to 2 HD/level":
                    stringBuilder.Append(PreventRollingString(" --set spelltargets one destroyed construct of up to [[@{caster" + casterLevel + "_level}*2]] HD"));
                    break;
                case "one door, box, or chest with an area of up to 10 sq. ft./level":
                    stringBuilder.Append(PreventRollingString(
                        " --set spelltargets one door, box, or chest with an area of up to [[@{caster" + casterLevel + "_level}*10]] sq.ft."));
                    break;
                case "one Gargantuan, Huge, or Large plant per three caster levels":
                    stringBuilder.Append(PreventRollingString(
                        " --set spelltargets [[{1,floor((@{caster" + casterLevel + "_level}+0)/3)}kh1]] Gargantuan, Huge, or Large plants"));
                    break;
                case "one good-aligned creature/3 levels":
                    stringBuilder.Append(PreventRollingString(
                        " --set spelltargets [[{1,floor((@{caster" + casterLevel + "_level}+0)/3)}kh1]] good-aligned creature"));
                    break;
                case "one humanoid creature per level":
                    stringBuilder.Append(PreventRollingString(" --set spelltargets [[@{caster" + casterLevel + "_level}]] humanoid creatures"));
                    break;
                case "One humanoid creature/level, no two of which can be more than 30 ft. apart":
                    stringBuilder.Append(PreventRollingString(
                        " --set spelltargets [[@{caster" + casterLevel + "_level}]] humanoid creatures, no two of which can be more than 30 ft. apart"));
                    break;
                case
                "one humanoid, magical beast, or monstrous humanoid/ level, no two of which can be more than 30 feet apart"
                :
                    stringBuilder.Append(PreventRollingString(
                        " --set spelltargets [[@{caster" + casterLevel + "_level}]] humanoid, magical beast, or monstrous humanoids, no two of which can be more than 30 ft. apart"));
                    break;
                case "one incorporeal creature/level, no two of which can be more than 30 ft. apart":
                    stringBuilder.Append(PreventRollingString(
                        " --set spelltargets [[@{caster" + casterLevel + "_level}]] incorporeal creatures, no two of which ca be more than 30 ft. apart"));
                    break;
                case "one Large plant per three caster levels":
                    stringBuilder.Append(PreventRollingString(" --set spelltargets [[{1,floor((@{caster" + casterLevel + "_level}+0)/3)}kh1]] Large plants"));
                    break;
                case "one Large plant per three caster levels or all plants within  range; see text":
                    stringBuilder.Append(PreventRollingString(
                        " --set spelltargets [[{1,floor((@{caster" + casterLevel + "_level}+0)/3)}kh1]] Large plants, or all plants within range; see text"));
                    break;
                case "one living creature per three levels, no two of which can be more than 30 ft. apart":
                    stringBuilder.Append(PreventRollingString(
                        " --set spelltargets [[{1,floor((@{caster" + casterLevel + "_level}+0)/3)}kh1]] living creatures, no two of which can be more than 30 ft. apart"));
                    break;
                case "one living creature touched per three levels":
                    stringBuilder.Append(PreventRollingString(
                        " --set spelltargets [[{1,floor((@{caster" + casterLevel + "_level}+0)/4)}kh1]] living creatures touched"));
                    break;
                case "one living creature/2 levels (no two of which may be more than 30 feet apart)":
                    stringBuilder.Append(PreventRollingString(
                        " --set spelltargets [[{1,floor((@{caster" + casterLevel + "_level}+0)/2)}kh1]] living creatures, no two of which may be more than 30 ft. apart"));
                    break;
                case
                "one living creature/level that is a reptile, has the dragon type, or has the reptilian subtype, and also has a natural armor bonus of at least +1"
                :
                    stringBuilder.Append(PreventRollingString(
                        " --set spelltargets [[@{caster" + casterLevel + "_level}]] creatures that is a reptile, has the dragon type, or has the reptilian subtype, and also has a natural armor bonus of at least +1"));
                    break;
                case
                "one living creature/level that is a reptile, has the dragon type, or has the reptilian subtype, and that also has a natural armor bonus of at least +1"
                :
                    stringBuilder.Append(PreventRollingString(
                        " --set spelltargets [[@{caster" + casterLevel + "_level}]] creatures that is a reptile, has the dragon type, or has the reptilian subtype, and also has a natural armor bonus of at least +1"));
                    break;
                case "one living creature/level within a 40-ft.-radius spread":
                    stringBuilder.Append(PreventRollingString(" --set spelltargets [[@{caster" + casterLevel + "_level}]] creatures in a 40-ft.-radius spread"));
                    break;
                case "one living creature/level, no two of which can be more than 30 ft. apart":
                    stringBuilder.Append(PreventRollingString(
                        " --set spelltargets [[@{caster" + casterLevel + "_level}]] living creatures, no two of which can be more than 30 ft. apart"));
                    break;
                case "one living creature/level, no two of which can be more than 60 feet apart":
                    stringBuilder.Append(PreventRollingString(
                        " --set spelltargets [[@{caster" + casterLevel + "_level}]] living creatures, no two of which can be more than 60 ft. apart"));
                    break;
                case "one living creature/level, no two of which may be more than 30 ft. apart":
                    stringBuilder.Append(PreventRollingString(
                        " --set spelltargets [[@{caster" + casterLevel + "_level}]] living creatures, no two of which can be more than 30 ft. apart"));
                    break;
                case "one location (up to a 10-ft. cube/level) or one object":
                    stringBuilder.Append(PreventRollingString(
                        " --set spelltargets one location (up to [[@{caster" + casterLevel + "_level}]] 10-ft. cubes) or one object"));
                    break;
                case "one means of closure/level, no two of which can be more than 30 feet apart":
                    stringBuilder.Append(PreventRollingString(
                        " --set spelltargets [[@{caster" + casterLevel + "_level}]] means of closure, no two of which can be more than 30 ft. apart"));
                    break;
                case
                "one Medium or smaller freefalling object or creature/level, no two of which may be more than 20 ft. apart"
                :
                    stringBuilder.Append(PreventRollingString(
                        " --set spelltargets [[@{caster" + casterLevel + "_level}]] Medium or smaller freefalling objects or creatures, no two of which may be more than 20 ft. apart"));
                    break;
                case "one Medium or smaller object or creature/level, no two of which can be more than 20 ft. apart":
                    stringBuilder.Append(PreventRollingString(
                        " --set spelltargets [[@{caster" + casterLevel + "_level}]] Medium or smaller objects or creatures, no two of which may be more than 20 ft. apart"));
                    break;
                case "One metal weapon/level, no two of which can be more than 30 ft. apart":
                    stringBuilder.Append(PreventRollingString(
                        " --set spelltargets [[@{caster" + casterLevel + "_level}]] metal weapons, no two of which can be more than 30 ft. apart"));
                    break;
                case "one mindless undead creature/level, no two of which can be more than 30 ft. apart":
                    stringBuilder.Append(PreventRollingString(
                        " --set spelltargets [[@{caster" + casterLevel + "_level}]] mindless undead creatures, no two of which can be more than 30 ft. apart"));
                    break;
                case "one non-mutated sahuagin/level, no two of which can be more than 30 ft. apart":
                    stringBuilder.Append(PreventRollingString(
                        " --set spelltargets [[@{caster" + casterLevel + "_level}]] non-mutated sahuagin, no two of which can be more than 30 ft. apart"));
                    break;
                case "one non-mythic creature/3 levels":
                    stringBuilder.Append(PreventRollingString(
                        " --set spelltargets [[{1,floor((@{caster" + casterLevel + "_level}+0)/3)}kh1]] non-mythic creatures"));
                    break;
                case "one object of no more than 10 cu. ft./level":
                    stringBuilder.Append(PreventRollingString(" --set spelltargets one object of no more than [[@{caster" + casterLevel + "_level}*10]] cu.ft."));
                    break;
                case "one object of up to 1 lb./level":
                    stringBuilder.Append(PreventRollingString(" --set spelltargets one object of up to [[@{caster" + casterLevel + "_level}]] lbs."));
                    break;
                case "one object of up to 10 cu. ft./level or one construct creature of any size":
                    stringBuilder.Append(PreventRollingString(
                        " --set spelltargets one object of up to [[@{caster" + casterLevel + "_level}*10]] or one construct creature of any size"));
                    break;
                case "one object of up to 5 lb./level":
                    stringBuilder.Append(PreventRollingString(" --set spelltargets one object of up to [[@{caster" + casterLevel + "_level}*5]] lbs."));
                    break;
                case "one object or creature per caster level":
                    stringBuilder.Append(PreventRollingString(" --set spelltargets [[@{caster" + casterLevel + "_level}]] objects or creatures"));
                    break;
                case "one object touched of up to 100 lbs./level":
                    stringBuilder.Append(PreventRollingString(" --set spelltargets one object touched of up to [[@{caster" + casterLevel + "_level}*100]] lbs."));
                    break;
                case "one object weighing no more than 1 lb./level":
                    stringBuilder.Append(PreventRollingString(" --set spelltargets one object weighing no more than [[@{caster" + casterLevel + "_level}]] lbs."));
                    break;
                case "one object weighing no more than 5 lbs./level":
                    stringBuilder.Append(PreventRollingString(
                        " --set spelltargets one object weighing no more than [[@{caster" + casterLevel + "_level}*5]] lbs."));
                    break;
                case "one object/2 levels or 1 10-ft. square/2 levels; see text":
                    stringBuilder.Append(PreventRollingString(
                        " --set spelltargets [[{1,floor((@{caster" + casterLevel + "_level}+0)/2)}kh1]] objects, or [[{1,floor((@{caster" + casterLevel + "_level}+0)/2)}kh1]] 10-ft. squares; see text"));
                    break;
                case "one or more Medium creatures/level, no two of which can be more than 30 ft. apart":
                    stringBuilder.Append(PreventRollingString(
                        " --set spelltargets [[@{caster" + casterLevel + "_level}]] Medium creatures, no two of which can be more than 30 ft. apart"));
                    break;
                case "one or more objects touched, up to 1 lb./level":
                    stringBuilder.Append(PreventRollingString(
                        " --set spelltargets one or more objects touched, up to [[@{caster" + casterLevel + "_level}]] lbs."));
                    break;
                case "one portal, up to 20 sq. ft./level":
                    stringBuilder.Append(PreventRollingString(" --set spelltargets one portal, up to [[@{caster" + casterLevel + "_level}*20]] sq.ft."));
                    break;
                case
                "one primary target plus one additional target/3 levels  (each of which must be within 15 ft. of the primary target)"
                :
                    stringBuilder.Append(PreventRollingString(
                        " --set spelltargets one primary target, plus [[{1,floor((@{caster" + casterLevel + "_level}+0)/3)}kh1]] additional targets (each of which must be within 15 ft. of the primary target)"));
                    break;
                case
                "one primary target, plus one secondary target/level (each of which must be within 30 ft. of the primary target)"
                :
                    stringBuilder.Append(PreventRollingString(
                        " --set spelltargets one primary target, plus [[{1,floor((@{caster" + casterLevel + "_level}+0)/3)}kh1]] additional targets (each of which must be within 30 ft. of the primary target)"));
                    break;
                case "one ropelike object, length up to 50 ft. + 5 ft./level":
                    stringBuilder.Append(PreventRollingString(
                        " --set spelltargets one ropelike object, length up to [[50+(@{caster" + casterLevel + "_level}*5)]] ft."));
                    break;
                case "one rope-like object, length up to 50 ft. + 5 ft./level; see text":
                    stringBuilder.Append(PreventRollingString(
                        " --set spelltargets one ropelike object, length up to [[50+(@{caster" + casterLevel + "_level}*5)]] ft.; see text"));
                    break;
                case "one Small object per caster level; see text":
                    stringBuilder.Append(PreventRollingString(" --set spelltargets [[@{caster" + casterLevel + "_level}]] Small objects; see text"));
                    break;
                case "one Small or smaller creature/level, no two of which can be more than 30 ft. apart":
                    stringBuilder.Append(PreventRollingString(
                        " --set spelltargets [[@{caster" + casterLevel + "_level}]] Small or smaller creatures, no two of which can be more than 30 ft. apart"));
                    break;
                case "one summoned creature you control/level, no two of which can be more than 30 ft. apart":
                    stringBuilder.Append(PreventRollingString(
                        " --set spelltargets [[@{caster" + casterLevel + "_level}]] summoned creatures you control, no two of which can be more than 30 ft. apart"));
                    break;
                case "one touched creature/level":
                    stringBuilder.Append(PreventRollingString(" --set spelltargets [[@{caster" + casterLevel + "_level}]] touched creatures"));
                    break;
                case "one touched holy symbol per 3 caster levels":
                    stringBuilder.Append(PreventRollingString(
                        " --set spelltargets [[{1,floor((@{caster" + casterLevel + "_level}+0)/3)}kh1]] touched holy symbols"));
                    break;
                case "one touched object of up to 10 pounds/level":
                    stringBuilder.Append(PreventRollingString(" --set spelltargets one touched object of up to [[@{caster" + casterLevel + "_level}]] lbs."));
                    break;
                case "one touched object of up to 2 cu. ft./level":
                    stringBuilder.Append(PreventRollingString(" --set spelltargets one touched object of up to [[@{caster" + casterLevel + "_level}*2]] cu.ft."));
                    break;
                case "one touched object of up to 50 lbs./level and 3 cu. ft./level":
                    stringBuilder.Append(PreventRollingString(
                        " --set spelltargets one touched object of up to [[@{caster" + casterLevel + "_level}*50]] lbs. and [[@{caster" + casterLevel + "_level}*3]] cu.ft."));
                    break;
                case "one touched object weighing up to 5 lbs./level":
                    stringBuilder.Append(PreventRollingString(
                        " --set spelltargets one touched object weighing up to [[@{caster" + casterLevel + "_level}*5]] lbs."));
                    break;
                case "one touched piece of wood no larger than 10 cu. ft. + 1 cu. ft./level":
                    stringBuilder.Append(PreventRollingString(
                        " --set spelltargets one touched piece of wood no larger than [[10+@{caster" + casterLevel + "_level}]] ft."));
                    break;
                case "one undead creature/level, no two of which can be more than 30 ft. apart":
                    stringBuilder.Append(PreventRollingString(
                        " --set spelltargets [[@{caster" + casterLevel + "_level}]] undead creatures, no two of which can be more than 30 ft. apart"));
                    break;
                case "one weapon per level":
                    stringBuilder.Append(PreventRollingString(" --set spelltargets [[@{caster" + casterLevel + "_level}]] weapons"));
                    break;
                case "one weapon/3 levels":
                    stringBuilder.Append(PreventRollingString(" --set spelltargets [[{1,floor((@{caster" + casterLevel + "_level}+0)/3)}kh1]] weapons"));
                    break;
                case "one weapon/level, no two of which can be more than 20 ft. apart":
                    stringBuilder.Append(PreventRollingString(
                        " --set spelltargets [[@{caster" + casterLevel + "_level}]] weapons, no two of which can be more than 20 ft. apart"));
                    break;
                case "one willing ally/5 levels":
                    stringBuilder.Append(PreventRollingString(" --set spelltargets [[{1,floor((@{caster" + casterLevel + "_level}+0)/5)}kh1]] willing allies"));
                    break;
                case "one willing creature or object (up to a 2-ft. cube/level) touched":
                    stringBuilder.Append(PreventRollingString(
                        " --set spelltargets one willing creature or object (up to [[@{caster" + casterLevel + "_level}]] 2-ft. cubes) touched"));
                    break;
                case "one willing creature plus one/2 caster levels, no two of which can be more than 30 ft. apart":
                    stringBuilder.Append(PreventRollingString(
                        " --set spelltargets [[{1,floor((@{caster" + casterLevel + "_level}+0)/2)}kh1]] willing creatures, no two of which can be more than 30 ft. apart"));
                    break;
                case "one willing creature/level":
                    stringBuilder.Append(PreventRollingString(" --set spelltargets [[@{caster" + casterLevel + "_level}]] willing creatures"));
                    break;
                case "one willing living creature per 2 levels, no two of which may be more than 30 ft. apart":
                    stringBuilder.Append(PreventRollingString(
                        " --set spelltargets [[{1,floor((@{caster" + casterLevel + "_level}+0)/2)}kh1]] willing living creatures, no two of which may be more than 30 ft. apart"));
                    break;
                case "one willing living creature per three levels, no two of which may be more than 30 ft. apart":
                    stringBuilder.Append(PreventRollingString(
                        " --set spelltargets [[{1,floor((@{caster" + casterLevel + "_level}+0)/3)}kh1]] willing living creatures, no two of which may be more than 30 ft. apart"));
                    break;
                case "one willing living creature per three levels, no two of which may be more than 30 ft. apart.":
                    stringBuilder.Append(PreventRollingString(
                        " --set spelltargets [[{1,floor((@{caster" + casterLevel + "_level}+0)/3)}kh1]] willing living creatures, no two of which may be more than 30 ft. apart"));
                    break;
                case "one willing living creature/level, no two of which can be more than 30 ft. apart":
                    stringBuilder.Append(PreventRollingString(
                        " --set spelltargets [[{1,floor((@{caster" + casterLevel + "_level}+0)/1)}kh1]] willing living creatures, no two of which may be more than 30 ft. apart"));
                    break;
                case "one willing undead creature per 3 levels, no two of which can be more than 30 ft. apart":
                    stringBuilder.Append(PreventRollingString(
                        " --set spelltargets [[{1,floor((@{caster" + casterLevel + "_level}+0)/3)}kh1]] willing living creatures, no two of which may be more than 30 ft. apart"));
                    break;
                case "snow or snow-sculpted object touched, up to 5 cubic ft.  + 1 cubic ft./level":
                    stringBuilder.Append(PreventRollingString(
                        " --set spelltargets snow or snow-sculpted object touched, up to [[5+@{caster" + casterLevel + "_level}]] cu.ft."));
                    break;
                case "stone or stone object touched, up to 10 cu. ft. + 1 cu. ft./level":
                    stringBuilder.Append(PreventRollingString(
                        " --set spelltargets stone or stone object touched, up to [[10+@{caster" + casterLevel + "_level}]] cu.ft."));
                    break;
                case "touched nonmagical circle of vine, rope, or thong with a 2 ft. diameter + 2 ft./level":
                    stringBuilder.Append(PreventRollingString(
                        " --set spelltargets touched nonmagical circle of vine, rope, or thong with a [[2+(@{caster" + casterLevel + "_level}*2)]]"));
                    break;
                case
                "two willing creatures plus another creature per 6 levels, no two of which can be more than 30 feet apart"
                :
                    stringBuilder.Append(PreventRollingString(
                        " --set spelltargets [[{1,floor((@{caster" + casterLevel + "_level}+0)/6)}kh1*2]] willing creatures, no two of which can be more than 30 ft. apart"));
                    break;
                case "up to 1 HD/level of vermin, no two of which can be more than 30 ft. apart":
                    stringBuilder.Append(PreventRollingString(
                        " --set spelltargets up to [[@{caster" + casterLevel + "_level}]] HD/level of vermin, no two of which can be more than 30 ft. apart"));
                    break;
                case "up to 10 cu. ft./level; see text":
                    stringBuilder.Append(PreventRollingString(" --set spelltargets up to [[@{caster" + casterLevel + "_level}*10]] cu.ft.; see text"));
                    break;
                case "up to 2 HD/level of plant creatures, no two of which can be more than 30 ft. apart":
                    stringBuilder.Append(PreventRollingString(
                        " --set spelltargets up to [[@{caster" + casterLevel + "_level}*2]] of plant creatures, no two of which can be more than 30 ft. apart"));
                    break;
                case "up to 2 HD/level of undead creatures, no two of which can be more than 30 ft. apart":
                    stringBuilder.Append(PreventRollingString(
                        " --set spelltargets up to [[@{caster" + casterLevel + "_level}*2]] HD of undead creatures, no two of which can be more than 30 ft. apart"));
                    break;
                case "up to one creature per 3 caster levels":
                    stringBuilder.Append(PreventRollingString(" --set spelltargets up to [[{1,floor((@{caster" + casterLevel + "_level}+0)/3)}kh1]] creatures"));
                    break;
                case "up to one creature per level, all within 30 ft. of each other":
                    stringBuilder.Append(PreventRollingString(
                        " --set spelltargets [[@{caster" + casterLevel + "_level}]] creatures, no two of which can be more than 30 ft. apart"));
                    break;
                case "up to one creature touched per four levels":
                    stringBuilder.Append(PreventRollingString(" --set spelltargets up to [[{1,floor((@{caster" + casterLevel + "_level}+0)/4)}kh1]] creatures"));
                    break;
                case "up to one creature/3 levels, no two of which can be more than 30 ft. apart":
                    stringBuilder.Append(PreventRollingString(
                        " --set spelltargets up to [[{1,floor((@{caster" + casterLevel + "_level}+0)/3)}kh1]] creatures, no two of which can be more than 30 ft. apart"));
                    break;
                case "up to one creature/level within 180 feet":
                    stringBuilder.Append(PreventRollingString(" --set spelltargets up to [[@{caster" + casterLevel + "_level}]] creatures within 180 ft."));
                    break;
                case "up to one creature/level, no two of which can be more than 30 feet apart":
                    stringBuilder.Append(PreventRollingString(
                        " --set spelltargets up to [[@{caster" + casterLevel + "_level}]] creatures, no two of which can be more than 30 ft. apart"));
                    break;
                case "up to one creature/level, no two of which can be more than 30 ft. apart":
                    stringBuilder.Append(PreventRollingString(
                        " --set spelltargets up to [[@{caster" + casterLevel + "_level}]] creatures, no two of which can be more than 30 ft. apart"));
                    break;
                case "up to one living creature/level, no two of which can be more than 30 feet apart":
                    stringBuilder.Append(PreventRollingString(
                        " --set spelltargets up to [[@{caster" + casterLevel + "_level}]] living creatures, no two of which can be more than 30 ft. apart"));
                    break;
                case "up to one natural or manufactured weapon per 3 caster levels":
                    stringBuilder.Append(PreventRollingString(
                        " --set spelltargets up to [[{1,floor((@{caster" + casterLevel + "_level}+0)/3)}kh1]] natural or manufactured weapons"));
                    break;
                case
                "up to one object per 3 caster levels, each weighing 10 lbs. or less whose longest dimension is 6 ft. or less"
                :
                    stringBuilder.Append(PreventRollingString(
                        " --set spelltargets up to [[{1,floor((@{caster" + casterLevel + "_level}+0)/3)}kh1]] objects, each weighing 10 lbs. or less whose longest dimension is 6 ft. or less"));
                    break;
                case "up to one touched creature/level":
                    stringBuilder.Append(PreventRollingString(" --set spelltargets up to [[@{caster" + casterLevel + "_level}]] touched creatures"));
                    break;
                case "up to one touched object per level weighing up to 5 lbs. each":
                    stringBuilder.Append(PreventRollingString(
                        " --set spelltargets up to [[@{caster" + casterLevel + "_level}]] touched objects per level, weighing up to 5 lbs. each"));
                    break;
                case "up to one weapon/level, no two of which can be more than 30 ft. apart":
                    stringBuilder.Append(PreventRollingString(
                        " --set spelltargets up to [[@{caster" + casterLevel + "_level}]] weapons, no two of which can be more than 30 ft. apart"));
                    break;
                case "up to one willing creature per level, all within 30 ft. of each other.":
                    stringBuilder.Append(PreventRollingString(
                        " --set spelltargets up to [[@{caster" + casterLevel + "_level}]] willing creatures, no two of which can be more than 30 ft. apart"));
                    break;
                case "willing, living creature or creatures touched or one creature/level (see text)":
                    stringBuilder.Append(PreventRollingString(
                        " --set spelltargets willing, living creature or creatures touched or [[@{caster" + casterLevel + "_level}]] creatures; see text"));
                    break;
                case "you and one ally/level, no two of which can be more than 30 ft. apart (see text)":
                    stringBuilder.Append(PreventRollingString(
                        " --set spelltargets you and [[@{caster" + casterLevel + "_level}]] allies, no two of which can be more than 30 ft. apart; see text"));
                    break;
                case "you and one creature/level":
                    stringBuilder.Append(PreventRollingString(" --set spelltargets you and [[@{caster" + casterLevel + "_level}]] creatures"));
                    break;
                case "you and one other touched creature per three levels":
                    stringBuilder.Append(PreventRollingString(
                        " --set spelltargets you and [[{1,floor((@{caster" + casterLevel + "_level}+0)/3)}kh1]] touched creatures"));
                    break;
                case "you and one touched creature per three levels":
                    stringBuilder.Append(PreventRollingString(
                        " --set spelltargets you and [[{1,floor((@{caster" + casterLevel + "_level}+0)/3)}kh1]] touched creatures"));
                    break;
                case "you and one willing creature/2 levels, all of which must be within 30 feet of you":
                    stringBuilder.Append(PreventRollingString(
                        " --set spelltargets you and [[{1,floor((@{caster" + casterLevel + "_level}+0)/2)}kh1]] willing creatures, all of which must be within 30 ft. of you"));
                    break;
                case "you and up to 1 willing creature/2 caster levels, no two of which can be more than 30 ft. apart":
                    stringBuilder.Append(PreventRollingString(
                        " --set spelltargets you and up to [[{1,floor((@{caster" + casterLevel + "_level}+0)/2)}kh1]] willing creatures, no two of which can be more than 30 ft. apart"));
                    break;
                case "you or a creature or object weighing no more than 100 lbs./level":
                    stringBuilder.Append(PreventRollingString(
                        " --set spelltargets you or a creature or object weighting no more than [[@{caster" + casterLevel + "_level}*100]]"));
                    break;
                case "you or one willing creature or one object (total weight up to 100 lbs./level)":
                    stringBuilder.Append(PreventRollingString(
                        " --set spelltargets you or one willing creature or one object (total weight up to [[@{caster" + casterLevel + "_level}*100]] lbs.)"));
                    break;
                case "you plus one additional willing creature touched per two caster levels":
                    stringBuilder.Append(PreventRollingString(
                        " --set spelltargets you plus [[{1,floor((@{caster" + casterLevel + "_level}+0)/2)}kh1]] additional willing creatures touched"));
                    break;
                case "you plus one creature/level, no two of which can be more than 30 ft. apart":
                    stringBuilder.Append(PreventRollingString(
                        " --set spelltargets you plus [[@{caster" + casterLevel + "_level}]] creatures, no two of which can be more than 30 ft. apart"));
                    break;
                case "you plus one touched creature/3 levels":
                    stringBuilder.Append(PreventRollingString(
                        " --set spelltargets you plus [[{1,floor((@{caster" + casterLevel + "_level}+0)/3)}kh1]] touched creatures"));
                    break;
                case "you plus one willing creature per 2 levels, no two of which can be more than 30 ft. apart":
                    stringBuilder.Append(PreventRollingString(
                        " --set spelltargets you plus [[{1,floor((@{caster" + casterLevel + "_level}+0)/2)}kh1]] willing creatures, no two of which can be more than 30 ft. apart"));
                    break;
                case "you plus one willing creature per 3 levels, no two of which can be more than 30 ft. part":
                    stringBuilder.Append(PreventRollingString(
                        " --set spelltargets you plus [[{1,floor((@{caster" + casterLevel + "_level}+0)/3)}kh1]] willing creatures, no two of which can be more than 30 ft. apart"));
                    break;
                case "you plus one willing creature per three levels, no two of which can be more than 30 ft. apart":
                    stringBuilder.Append(PreventRollingString(
                        " --set spelltargets you plus [[{1,floor((@{caster" + casterLevel + "_level}+0)/3)}kh1]] willing creatures, no two of which can be more than 30 ft. apart"));
                    break;
                case "you plus one willing creature/3 levels, no two of which can be more than 30 ft. apart":
                    stringBuilder.Append(PreventRollingString(
                        " --set spelltargets you plus [[{1,floor((@{caster" + casterLevel + "_level}+0)/3)}kh1]] willing creatures, no two of which can be more than 30 ft. apart"));
                    break;
                case "you plus one willing living creature per 3 levels, no two of which can be more than 30 ft. apart":
                    stringBuilder.Append(PreventRollingString(
                        " --set spelltargets you plus [[{1,floor((@{caster" + casterLevel + "_level}+0)/3)}kh1]] willing creatures, no two of which can be more than 30 ft. apart"));
                    break;
                default:
                    stringBuilder.Append(PreventRollingString($" --set spelltargets {spellTargets}"));
                    break;
            }
        }

        private static void SetNpcSpellArea(StringBuilder stringBuilder, string spellArea, int casterLevel)
        {
            if (string.IsNullOrEmpty(spellArea)) return;
            switch (spellArea)
            {
                case "1 creature/level":
                    stringBuilder.Append(PreventRollingString(" --set spellarea [[@{caster" + casterLevel + "_level}]] creatures"));
                    break;
                case "10-ft. cube/level (S)":
                    stringBuilder.Append(PreventRollingString(" --set spellarea [[@{caster" + casterLevel + "_level}]] 10-ft. cubes"));
                    break;
                case "10-ft. square/level; see text":
                    stringBuilder.Append(PreventRollingString(" --set spellarea [[@{caster" + casterLevel + "_level}]] 10-ft. squares; see text"));
                    break;
                case "30-ft. cube/level":
                    stringBuilder.Append(PreventRollingString(" --set spellarea [[@{caster" + casterLevel + "_level}]] 30-ft. cubes"));
                    break;
                case "40 ft./level radius cylinder 40 ft. high":
                    stringBuilder.Append(PreventRollingString(" --set spellarea [[@{caster" + casterLevel + "_level}*40]] radius cylinder 40 ft. high"));
                    break;
                case "60-ft. cube/level":
                    stringBuilder.Append(PreventRollingString(" --set spellarea [[@{caster" + casterLevel + "_level}]] cube"));
                    break;
                case "circle centered on you with a radius of 400 ft. + 40 ft./level":
                    stringBuilder.Append(PreventRollingString(
                        " --set spellarea circle centered on you, with a radius of [[400+(@{caster" + casterLevel + "_level}*40)]] ft."));
                    break;
                case "circle centered on you, with a radius of 100 feet + 10 feet per level":
                    stringBuilder.Append(PreventRollingString(
                        " --set spellarea circle centered on you, with a radius of [[100+(@{caster" + casterLevel + "_level}*10)]] ft."));
                    break;
                case "circle, centered on you, with a radius of 1 mile/level":
                    stringBuilder.Append(PreventRollingString(
                        " --set spellarea circle centered on you, with a radius of [[@{caster" + casterLevel + "_level}]] miles"));
                    break;
                case "circle, centered on you, with a radius of 400 ft. + 40 ft./level":
                    stringBuilder.Append(PreventRollingString(
                        " --set spellarea circle centered on you with a radius of [[400+(@{caster" + casterLevel + "_level}*40)]] ft."));
                    break;
                case "contiguous area consisting of one 10-foot cube/level (S)":
                    stringBuilder.Append(PreventRollingString(
                        " --set spellarea contiguous area consisting of [[@{caster" + casterLevel + "_level}]] 10-ft. cubes"));
                    break;
                case "contiguous area up to one 5-foot cube/caster level (S)":
                    stringBuilder.Append(PreventRollingString(" --set spellarea contiguous area up to [[@{caster" + casterLevel + "_level}]] 5-ft. cubes"));
                    break;
                case "cylinder (5-ft./level radius, 40 ft. high)":
                    stringBuilder.Append(PreventRollingString(
                        " --set spellarea cylinder ([[@{caster" + casterLevel + "_level}*5]] ft. radius, 40 ft. high)"));
                    break;
                case "cylinder of water (5-ft. radius/level, 30 ft. deep)":
                    stringBuilder.Append(PreventRollingString(
                        " --set spellarea cylinder of water ([[@{caster" + casterLevel + "_level}*5]] ft. radius, 30 ft. deep)"));
                    break;
                case "dust, earth, sand, or water in one 5-ft. square/level":
                    stringBuilder.Append(PreventRollingString(
                        " --set spellarea dust, earth, sand, or water in [[@{caster" + casterLevel + "_level}]] 5-ft. squares"));
                    break;
                case "four 5-ft. squares/level (see text)":
                    stringBuilder.Append(PreventRollingString(" --set spellarea [[@{caster" + casterLevel + "_level}*4]] 5-ft. squares"));
                    break;
                case "object touched or up to 5 sq. ft./level":
                    stringBuilder.Append(PreventRollingString(" --set spellarea object touched or up to [[@{caster" + casterLevel + "_level}*5]] sq. ft."));
                    break;
                case "one 10-ft. cube per level":
                    stringBuilder.Append(PreventRollingString(" --set spellarea [[@{caster" + casterLevel + "_level}]] 10-ft. cubes"));
                    break;
                case "one 10-ft. cube/level":
                    stringBuilder.Append(PreventRollingString(" --set spellarea [[@{caster" + casterLevel + "_level}]] 10-ft. cubes"));
                    break;
                case "one 10-ft. cube/level (S)":
                    stringBuilder.Append(PreventRollingString(" --set spellarea [[@{caster" + casterLevel + "_level}]] 10-ft. cubes (S)"));
                    break;
                case "one 1-mile cube/level":
                    stringBuilder.Append(PreventRollingString(" --set spellarea [[@{caster" + casterLevel + "_level}]] 1-mile cubes"));
                    break;
                case "one 20-ft. cube/level":
                    stringBuilder.Append(PreventRollingString(" --set spellarea [[@{caster" + casterLevel + "_level}]] 20-ft. cubes"));
                    break;
                case "one 20-ft. cube/level (S)":
                    stringBuilder.Append(PreventRollingString(" --set spellarea [[@{caster" + casterLevel + "_level}]] 20-ft. cubes (S)"));
                    break;
                case "one 20-ft. square/level":
                    stringBuilder.Append(PreventRollingString(" --set spellarea [[@{caster" + casterLevel + "_level}]] 20-ft. squares (S)"));
                    break;
                case "one 30-ft. cube/level":
                    stringBuilder.Append(PreventRollingString(" --set spellarea [[@{caster" + casterLevel + "_level}]] 30-ft. cubes"));
                    break;
                case "one 30-ft. cube/level (S)":
                    stringBuilder.Append(PreventRollingString(" --set spellarea [[@{caster" + casterLevel + "_level}]] 30-ft. cubes (S)"));
                    break;
                case "one 40-ft. cube/level (S)":
                    stringBuilder.Append(PreventRollingString(" --set spellarea [[@{caster" + casterLevel + "_level}]] 40-ft. cubes (S)"));
                    break;
                case "one 5-ft. cube/level (S)":
                    stringBuilder.Append(PreventRollingString(" --set spellarea [[@{caster" + casterLevel + "_level}]] 5-ft. cubes (S)"));
                    break;
                case "one 5-ft. square/2 levels":
                    stringBuilder.Append(PreventRollingString(" --set spellarea [[{1,floor((@{caster" + casterLevel + "_level}+0)/2)}kh1]] 5-ft. cubes"));
                    break;
                case "one 5-ft. square/level (S)); the area must be a stone surface":
                    stringBuilder.Append(PreventRollingString(
                        " --set spellarea [[@{caster" + casterLevel + "_level}]] 5-ft. cubes (S)); the area must be a stone surface"));
                    break;
                case "one creature/level, no two of which can be more than 30 ft. apart":
                    stringBuilder.Append(PreventRollingString(
                        " --set spellarea [[@{caster" + casterLevel + "_level}]] creatures, no two of which can be more than 30 ft. apart"));
                    break;
                case "radius spread of up to 10 ft./level, 90 ft. high":
                    stringBuilder.Append(PreventRollingString(
                        " --set spellarea radius spread of up to [[@{caster" + casterLevel + "_level}*10]] ft., 90 ft. high"));
                    break;
                case "ten 10-foot cubes/level":
                    stringBuilder.Append(PreventRollingString(" --set spellarea [[@{caster" + casterLevel + "_level}*10]] 10-ft. cubes"));
                    break;
                case "two 10-ft. cubes per level (S)":
                    stringBuilder.Append(PreventRollingString(" --set spellarea [[@{caster" + casterLevel + "_level}*2]] 10-ft. cubes (S)"));
                    break;
                case "up to 10-ft.-radius/level emanation centered on you":
                    stringBuilder.Append(PreventRollingString(
                        " --set spellarea up to [[@{caster" + casterLevel + "_level}*10]]-ft.-radius emanation centered on you"));
                    break;
                case "up to 200 sq. ft./level":
                    stringBuilder.Append(PreventRollingString(" --set spellarea up to [[@{caster" + casterLevel + "_level}*200]] sq. ft."));
                    break;
                case "up to one 10-ft. cube/level (S)":
                    stringBuilder.Append(PreventRollingString(" --set spellarea up to [[@{caster" + casterLevel + "_level}]] 10-ft. cubes (S)"));
                    break;
                case "up to one creature/level, no two of which may be more than 30 ft. apart":
                    stringBuilder.Append(PreventRollingString(
                        " --set spellarea up to [[@{caster" + casterLevel + "_level}]] creatures, no two of which may be more than 30 ft. apart"));
                    break;
                case "up to two 10-ft. cubes/level":
                    stringBuilder.Append(PreventRollingString(" --set spellarea up to [[@{caster" + casterLevel + "_level}*2]]-ft. cubes"));
                    break;
                case "water in a volume of 10 ft./level by 10 ft./level by 2 ft./level":
                    stringBuilder.Append(PreventRollingString(
                        " --set spellarea water in a volume of [[@{caster" + casterLevel + "_level}*10]] ft. by [[@{caster" + casterLevel + "_level}*10]] ft. by [[@{caster" + casterLevel + "_level}*2]] ft."));
                    break;
                default:
                    stringBuilder.Append(PreventRollingString($" --set spellarea {spellArea}"));
                    break;
            }
        }

        private static void SetNpcSpellRange(StringBuilder stringBuilder, string spellRange, int casterLevel)
        {
            if (string.IsNullOrEmpty(spellRange)) return;
            switch (spellRange)
            {
                case "long (400 ft. + 40 ft./level)":
                    stringBuilder.Append(PreventRollingString(" --set spellrange Long ([[400+(40*@{caster" + casterLevel + "_level})]])"));
                    break;
                case "medium (100 ft. + 10 ft./level)":
                    stringBuilder.Append(PreventRollingString(" --set spellrange Medium ([[100+(10*@{caster" + casterLevel + "_level})]])"));
                    break;
                case "close (25 ft. + 5 ft./2 levels)":
                    stringBuilder.Append(PreventRollingString(" --set spellrange Close ([[({1,floor((@{caster" + casterLevel + "_level}+0)/2)}kh1*5)+25]] ft.)"));
                    break;
                default:
                    stringBuilder.Append($" --set spellrange {spellRange}");
                    break;
            }
        }
    }
}
