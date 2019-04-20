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
        private static void WriteNpcSpell(StreamWriter fileWriter, HeroLabCharacter character, Spell spell)
        {
            var spellClass = character.Classes.Classes.FirstOrDefault(c => c.Name == spell.Class);
            if (spellClass == null) return;
            fileWriter.Write($"!HeroLabImporter --name {character.Name} --mode add --addtype spell-{spell.Level}");
            fileWriter.Write($" --set spellname {spell.Name}");
            fileWriter.Write($" --set spelldesc {spell.Description.Replace('\r', ' ').Replace('\n', ' ')}");
            fileWriter.Write($" --set spellprepared {spell.CastsLeft}");
            var spellSource = SpellDatabase.Instance.FirstOrDefault(ss => ss.Name == spell.Name);
            WriteNpcSpellInfo(fileWriter, character, spellSource);
        }

        private static void WriteNpcSpellInfo(StreamWriter fileWriter, HeroLabCharacter character, SpellSource spell)
        {
            //Search for the like spell to attach all of the information from.
            //Thanks, HeroLab for not exporting all of the information for Spell-Like Abilities properly.
            if (spell != null)
            {
                if (!string.IsNullOrEmpty(spell.School)) fileWriter.Write($" --set spellschool {spell.School}"); //spellschool
                if (!string.IsNullOrEmpty(spell.SpellLevel)) fileWriter.Write($" --set spellclasslevel {spell.SpellLevel}"); //spellclasslevel
                if (!string.IsNullOrEmpty(spell.CastingTime)) fileWriter.Write($" --set spellcastingtime {spell.CastingTime}"); //spellcastingtime
                SetNpcSpellRange(fileWriter, spell.Range); //spellrange
                SetNpcSpellArea(fileWriter, spell.Area); //spellarea
                SetNpcSpellTarget(fileWriter, spell.Targets); //spelltargets
                SetNpcSpellEffect(fileWriter, spell.Effect); //spelleffect
                SetNpcSpellDuration(fileWriter, spell.Duration); //spellduration
                SetNpcSpellSavingThrow(fileWriter, character, spell.SavingThrow, spell.SlaLevel); //spellsaveflag; spellsave, spelldc_mod
                SetNpcSpellSpellResistance(fileWriter, character, spell.SpellResistence); //spellresistanceflag; spellresistance
                SetNpcSpellAttack(fileWriter, character, spell.Description); //spellatkflag; spellatkmod, spellatkcritrange, spelldmgcritmulti
                //SpellDamage isn't supported... unfortunately.
                //spelldmgflag
                // spelldmg
                // spelldmgtype
                //spelldmgflag2
                // spelldmg2
                // spelldmgtype2
                fileWriter.Write(" --set spelldescflag 1"); //spelldescflag
                fileWriter.Write($" --set spelldesc {spell.Description.Replace('\r', ' ').Replace('\n', ' ')}"); //spelldesc
                fileWriter.WriteLine();
            }
        }

        private static void SetNpcSpellAttack(StreamWriter fileWriter, HeroLabCharacter character, string description)
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
            fileWriter.Write(" --set spellatkflag 1");
            fileWriter.Write($" --set spellatkmod {mod}");
            fileWriter.Write($" --set spellatkcritrange {critRange}");
            fileWriter.Write(" --set spelldmgcritmulti 2");
        }

        private static void SetNpcSpellSpellResistance(StreamWriter fileWriter, HeroLabCharacter character, string spellResistence)
        {
            if (string.IsNullOrEmpty(spellResistence)) return;
            if (spellResistence.ToLower() == "no" || spellResistence.ToLower() == "none") return;
            fileWriter.Write(" --set spellresistanceflag 1");
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
            fileWriter.Write($" --set spellresistance {spellResistence} (Roll [[d20+{sr}]])");
        }

        private static void SetNpcSpellSavingThrow(StreamWriter fileWriter, HeroLabCharacter character, string savingThrow, string slaLevel)
        {
            if (string.IsNullOrEmpty(savingThrow)) return;
            if (savingThrow.ToLower() == "none" || savingThrow.ToLower() == "no") return;
            fileWriter.Write(" --set spellsaveflag 1");
            fileWriter.Write($" --set spellsave {savingThrow}");
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
            fileWriter.Write($" --set spelldc_mod {baseDC + int.Parse(slaLevel)}");
        }

        private static void SetNpcSpellEffect(StreamWriter fileWriter, string spellEffect)
        {
            if (string.IsNullOrEmpty(spellEffect)) return;
            switch (spellEffect)
            {
                case "1 comet per 4 levels": fileWriter.Write(" --set spelleffect [[{1,floor((@{caster2_level}+0)/4)}kh1]] comets"); break;
                case "1 dose of a drug/3 levels": fileWriter.Write(" --set spelleffect [[{1,floor((@{caster2_level}+0)/3)}kh1]] doses of a drug"); break;
                case "1 hippocampus plus 1 hippocampus/3 caster levels": fileWriter.Write(" --set spelleffect [[{1,floor((@{caster2_level}+0)/3)}kh1+1]] hippocampus"); break;
                case "1 ice spear/4 levels": fileWriter.Write(" --set spelleffect [[{1,floor((@{caster2_level}+0)/4)}kh1]] ice spears"); break;
                case "1 legendary character/4 caster levels": fileWriter.Write(" --set spelleffect [[{1,floor((@{caster2_level}+0)/4)}kh1]] legendary characters"); break;
                case "10-ft.-by-10-ft. hole, 10 ft. deep/2 levels": fileWriter.Write(" --set spelleffect 10-ft.-by-10-ft. hole, [[{1,floor((@{caster2_level}+0)/2)}kh1*10]] deep"); break;
                case "10-ft.-deep wave 10 ft. wide/level and 2 ft. tall/level": fileWriter.Write(" --set spelleffect 10-ft. deep, [[@{caster2_level}*10]] wide, [[@{caster2_level}*2]] tall wave"); break;
                case "10-ft.-high earthen wall, in a line up to 10 ft. long/2 levels, or a circle with radius of up to 3 ft. + 1 ft./level": fileWriter.Write(" --set spelleffect 10-ft.-high earthen wall, in a line up to [[{1,floor((@{caster2_level}+0)/2)}kh1*10]] ft. long, or a circle with a radius up to [[@{caster2_level}+3]] ft."); break;
                case "10-ft.-high vertical sheet of illumination up to 5 ft. long/level": fileWriter.Write(" --set spelleffect 10-ft.-high vertical sheet of illumination up to [[@{caster2_level}*5]] ft. long"); break;
                case "10-ft.-high vertical sheet of light up to 5 ft. long/level": fileWriter.Write(" --set spelleffect 10-ft.-high vertical sheet of light up to [[@{caster2_level}*5]] ft. long"); break;
                case "1-ft.-diameter/level sphere, centered around a creature": fileWriter.Write(" --set spelleffect [[@{caster2_level}]]-ft.diameter sphere, centered around a creature"); break;
                case "1-ft.-diameter/level sphere, centered around creatures or objects": fileWriter.Write(" --set spelleffect [[@{caster2_level}]]-ft.diameter sphere, centered around creatures or objects"); break;
                case "20-ft.-by-20-ft. hole, 10 ft. deep/4 levels": fileWriter.Write(" --set spelleffect 20-ft.-by-20-ft. hole, [[{1,floor((@{caster2_level}+0)/4)}kh1*10]] ft. deep"); break;
                case "20-ft.-high wall of energy whose area is up to one 10-ft. square/level": fileWriter.Write(" --set spelleffect 20-ft.-high wall or energy whose area is up to [[@{caster2_level}]] 10-ft. squares"); break;
                case "5-ft.-by-8-ft. opening, 10 ft. deep plus 5 ft. deep per three additional levels": fileWriter.Write(" --set spelleffect 5-ft.-by-8-ft. opening, [[10+({1,floor((@{caster2_level}+0)/3)}kh1*5)]] deep"); break;
                case "5-ft.-wide, 60-ft.-deep extradimensional hole, up to 5 ft. long per level (S)": fileWriter.Write(" --set spelleffect 5-ft.-wide, 60-ft.-deep extradimensional hole, up to [[@{caster2_level}*5]] ft. long (S)"); break;
                case "a 5-foot-radius beanstalk that grows to a height of 50 ft./caster level": fileWriter.Write(" --set spelleffect a 5-ft.-radius beanstalk that grows to a height of [[@{caster2_level}*50]]"); break;
                case "a low wall 10 feet long per 3 levels (minimum 10 feet) (S)": fileWriter.Write(" --set spelleffect a low wall up to [[{1,floor((@{caster2_level}+0)/3)}kh1*10]] ft. long, minimum 10 feet (S)"); break;
                case "a wall of corpses with an area of up to one 5-ft. square/ level (S)": fileWriter.Write(" --set spelleffect a wall of corpses with an area of up to [[@{caster2_level}]] 5-ft. squares (S)"); break;
                case "an ironwood object weighing up to 5 lbs./level": fileWriter.Write(" --set spelleffect an ironwood object weighing up to [[@{caster2_level}*5]] lbs."); break;
                case "anchored plane of ice, up to one 10-ft. square/level": fileWriter.Write(" --set spelleffect anchored plane of ice, up to [[@{caster2_level}]] 10-ft. squares"); break;
                case "anchored plane of ice, up to one 10-ft. square/level, or hemisphere of ice with a radius of up to 3 ft. + 1 ft./level": fileWriter.Write(" --set spelleffect anchored plane of ice, up to [[@{caster2_level}]] 10-ft. squares, or a hemisphere of ice with a radius of [[@{caster2_level}+3]]"); break;
                case "anti-magic wall occupying up to two 5 ft. cubes/level (S)": fileWriter.Write(" --set spelleffect anti-magic wall occupying up to [[@{caster2_level}*2]] 5-ft. cubes"); break;
                case "ethereal 5-ft.-by-8-ft. opening, 10 ft. deep + 5 ft. deep per three levels": fileWriter.Write(" --set spelleffect ethereal 5-ft.-by-8-ft. opening, [[10+({1,floor((@{caster2_level}+0)/3)}kh1*5)]] ft. deep"); break;
                case "extradimensional demiplane, up to 10 10-ft. cubes/level (S)": fileWriter.Write(" --set spelleffect extradimensional demiplane, up to [[@{caster2_level}*10]] 10-ft. cubes (S)"); break;
                case "extradimensional demiplane, up to 20 10-ft. cubes/level (S)": fileWriter.Write(" --set spelleffect extradimensional demiplane, up to [[@{caster2_level}*20]] 10-ft. cubes (S)"); break;
                case "extradimensional demiplane, up to three 10-ft. cubes/level (S)": fileWriter.Write(" --set spelleffect extradimensional demiplane, up to [[@{caster2_level}*3]] 10-ft. cubes (S)"); break;
                case "extradimensional mansion, up to three 10-ft. cubes/level (S)": fileWriter.Write(" --set spelleffect extradimensional mansion, up to [[@{caster2_level}*3]] 10-ft. cubes (S)"); break;
                case "extradimensional meadow, up to three 10-ft. cubes/level (S)": fileWriter.Write(" --set spelleffect extradimensional meadow, up to [[@{caster2_level}*20]] 10-ft. cubes (S)"); break;
                case "extradimensional space up to 1 cu. ft./level": fileWriter.Write(" --set spelleffect extradimensional space, up to [[@{caster2_level}]] cu.ft."); break;
                case "feast for one creature/level": fileWriter.Write(" --set spelleffect feast for [[@{caster2_level}]] creatures"); break;
                case "feast for two creatures/level": fileWriter.Write(" --set spelleffect feast for [[@{caster2_level}*2]] creatures"); break;
                case "figment that cannot extend beyond a 20-ft. cube + one 10-ft. cube/level (S)": fileWriter.Write(" --set spelleffect figment that cannot extend beyond a 20-ft. cube + [[@{caster2_level}]] 10-ft. cube (S)"); break;
                case "five 5-ft. cubes of temporal possibility plus one additional cube/level": fileWriter.Write(" --set spelleffect [[5+@{caster2_level}]] 5-ft. cubes of temporal possibilities"); break;
                case "food and water to sustain three humans or one horse/level for 24 hours": fileWriter.Write(" --set spelleffect food and water to sustain [[@{caster2_level}*3]] humans or [[@{caster2_level}]] horses for 24 hours"); break;
                case "hemisphere that cannot extend beyond four 10-ft. cubes + one 10-ft. cube/level (S)": fileWriter.Write(" --set spelleffect hemisphere that cannot extend beyond [[4+@{caster2_level}]] 10-ft. cubes (S)"); break;
                case "illusory, unattended, nonmagical object of nonliving plant matter, up to 1 cu. ft./level": fileWriter.Write(" --set spelleffect illusory, unattended, nonmagical object of nonliving plant mater, up to [[@{caster2_level}]] cu.ft."); break;
                case "illusory, unattended, nonmagical object, up to 1 cu. ft./level": fileWriter.Write(" --set spelleffect illusory, unattended, nonmagical object, up to [[@{caster2_level}]] cu.ft."); break;
                case "iron wall whose area is up to one 5-ft. square/level; see text": fileWriter.Write(" --set spelleffect iron wall whose area is up to [[@{caster2_level}]] 5-ft. squares; see text"); break;
                case "mobile 10-ft.-by-10-ft. hole, 10 ft. deep/2 levels": fileWriter.Write(" --set spelleffect mobile 10-ft.-by-10-ft. hole, [[{1,floor((@{caster2_level}+0)/2)}kh1*10]] ft. deep"); break;
                case "one 5-ft. cube of temporal possibility/2 levels": fileWriter.Write(" --set spelleffect [[{1,floor((@{caster2_level}+0)/2)}kh1]] 5-ft. cubes of temporal possibility"); break;
                case "one animal whose CR is equal or less than your caster level": fileWriter.Write(" --set spelleffect one animal CR equal to or less than [[@{caster2_level}]]"); break;
                case "one insect scout/4 levels": fileWriter.Write(" --set spelleffect [[{1,floor((@{caster2_level}+0)/4)}kh1]] insect scouts"); break;
                case "one invisible sailor per level": fileWriter.Write(" --set spelleffect [[@{caster2_level}]] invisible sailors"); break;
                case "one poisonous mushroom/level, no two of which can be more than 30 ft. apart": fileWriter.Write(" --set spelleffect [[@{caster2_level}]] poisonous mushrooms, no two of which can be more than 30 ft. apart"); break;
                case "one summoned petitioner/caster level": fileWriter.Write(" --set spelleffect [[@{caster2_level}]] summoned petitioner"); break;
                case "one swarm of wasps per three levels, each of which must be adjacent to at least one other swarm": fileWriter.Write(" --set spelleffect [[{1,floor((@{caster2_level}+0)/3)}kh1]] swarms of wasps, each of which must be adjacent to at least one other swarm"); break;
                case "opaque sheet of ectoplasm up to 10 ft. square/level or a sphere or hemisphere with a radius of up to 1 ft./level": fileWriter.Write(" --set spelleffect opaque sheet of ectoplasm up to [[@{caster2_level}*10]] ft. square, or a sphere or hemisphere with a radius of up to [[@{caster2_level}]] ft."); break;
                case "opaque sheet of flame up to 20 ft. long/level or a ring of fire with a radius of up to 5 ft./two levels; either form 20 ft. high": fileWriter.Write(" --set spelleffect opaque sheet of flame up to [[@{caster2_level}*20]] ft. long, or a ring of fire with a radius of up to [[{1,floor((@{caster2_level}+0)/2)}kh1*5]] ft.; either form 20 ft. high"); break;
                case "opulent mansion, up to 300 feet on a side and one story tall/4 levels": fileWriter.Write(" --set spelleffect opulent mansion, up to 300 feet on a side and [[{1,floor((@{caster2_level}+0)/4)}kh1]] stories tall"); break;
                case "raft large enough for caster and one passenger/2 levels": fileWriter.Write(" --set spelleffect raft large enough for caster and [[{1,floor((@{caster2_level}+0)/2)}kh1]] passengers"); break;
                case "solid wall of humanoid bones with an area of up to one 5-ft. square/level": fileWriter.Write(" --set spelleffect solid wall of humanoid bones with an area of up to [[@{caster2_level}]] 5-ft. squares"); break;
                case "spout of boiling water filling a 5 ft. square and spraying upward 10 ft./2 levels": fileWriter.Write(" --set spelleffect spout of boiling water filling a 5 ft. square and spraying upward [[{1,floor((@{caster2_level}+0)/2)}kh1*10]] ft."); break;
                case "stone wall whose area is up to one 5-ft. square/level (S)": fileWriter.Write(" --set spelleffect stone wall whose area is up to [[@{caster2_level}]] 5-ft. squares (S)"); break;
                case "translucent wall 20 ft. long/level or a translucent ring with a radius of up to 5 ft./two levels; either form 20 ft. high": fileWriter.Write(" --set spelleffect translucent wall [[@{caster2_level}*20]] ft. long, or a translucent ring with a radius of up to [[{1,floor((@{caster2_level}+0)/2)}kh1]] ft.; either form 20 ft. high"); break;
                case "translucent wall of sound up to 20 ft. long/level or a ring of sound with a radius of up to 5 ft./two levels; either form 20 ft. high": fileWriter.Write(" --set spelleffect translucent wall of sound up to [[@{caster2_level}*20]] ft. long, or a ring of sound with a radius of up to [[{1,floor((@{caster2_level}+0)/2)}kh1]] ft.; either form 20 ft. high"); break;
                case "transparent wall 20 ft. high by up to 20 ft. long/level": fileWriter.Write(" --set spelleffect transparent wall 20 ft. high by up to [[@{caster2_level}*20]] ft. long"); break;
                case "transparent wall whose area is up to one 10-ft. square/level": fileWriter.Write(" --set spelleffect transparent wall whose area is up to [[@{caster2_level}]] 20-ft. squares"); break;
                case "unattended, nonmagical object of nonliving plant matter, up to 1 cu. ft./level": fileWriter.Write(" --set spelleffect unattended, nonmagical object of nonliving plant matter, up to [[@{caster2_level}]] cu.ft."); break;
                case "up to 1 draft of the waters of Lamashtu per 2 levels": fileWriter.Write(" --set spelleffect up to [[{1,floor((@{caster2_level}+0)/2)}kh1]] drafts of the waters of Lamashtu"); break;
                case "up to 1 flask of the waters of Lamashtu per 2 levels": fileWriter.Write(" --set spelleffect up to [[{1,floor((@{caster2_level}+0)/2)}kh1]] flasks of the waters of Lamashtu"); break;
                case "up to 2 gallons of water/level": fileWriter.Write(" --set spelleffect up to [[@{caster2_level}*2]] gallons of water"); break;
                case "up to one insect spy/4 levels": fileWriter.Write(" --set spelleffect up to [[{1,floor((@{caster2_level}+0)/4)}kh1]] insect spies"); break;
                case "up to one sword per 3 levels": fileWriter.Write(" --set spelleffect up to [[{1,floor((@{caster2_level}+0)/3)}kh1]] swords"); break;
                case "up to three 10-ft. cubes/level (S)": fileWriter.Write(" --set spelleffect up to [[@{caster2_level}*3]] 10-ft. cubes (S)"); break;
                case "visual figment that cannot extend beyond a 20-ft. cube + one 10-ft. cube/level (S)": fileWriter.Write(" --set spelleffect visual figment that cannot extend beyond a 20-ft. cube and [[@{caster2_level}*10]] 10-ft. cubes (S)"); break;
                case "visual figment that cannot extend beyond four 10-ft. cubes + one 10-ft. cube/level (S)": fileWriter.Write(" --set spelleffect visual figment that cannot extend beyond a 20-ft. cube and [[@{caster2_level}*10]] 10-ft. cubes (S)"); break;
                case "wall 4 ft./level wide, 2 ft./level high": fileWriter.Write(" --set spelleffect wall [[4*@{caster2_level}]] wide, [[2*@{caster2_level}]] high"); break;
                case "wall of blades up to 10 ft. long/level and 10 ft. tall": fileWriter.Write(" --set spelleffect wall of blades up to [[10*@{caster2_level}]] ft. long and 10 ft. tall"); break;
                case "wall of bronze clockworks whose area is up to one 5-ft.-square/level (S)": fileWriter.Write(" --set spelleffect wall of bronze clockworks whose area is up to [[@{caster2_level}]] 5-ft.-squares (S)"); break;
                case "wall of thorny brush, up to one 10-ft. cube/level (S)": fileWriter.Write(" --set spelleffect wall of thorny brush, up to [[@{caster2_level}]] 10-ft. cubes (S)"); break;
                case "wall of whirling blades up to 20 ft. long/level, or a ringed wall of whirling blades with a radius of up to 5 ft. per two levels; either form is 20 ft. high": fileWriter.Write(" --set spelleffect wall of whirling blades up to [[20*@{caster2_level}]], or a ringed wall of whirling blades with a radius of up to [[{1,floor((@{caster2_level}+0)/2)}kh1*5]] ft.; either form is 20 ft. high"); break;
                case "wall up to 10 ft./level long and 5 ft./level high (S)": fileWriter.Write(" --set spelleffect wall up to [[10*@{caster2_level}]] ft. long and [[5*@{caster2_level}]] ft. high (S)"); break;
                case "wall whose area is up to one 10-ft. square/level": fileWriter.Write(" --set spelleffect wall whose area is up to [[@{caster2_level}]] 10-ft. squares"); break;
                case "you plus up to one willing creature per 3 levels": fileWriter.Write(" --set spelleffect you plus up to [[{1,floor((@{caster2_level}+0)/3)}kh1]] willing creatures"); break;
                default: fileWriter.Write($" --set spelleffect {spellEffect}"); break;
            }
        }

        private static void SetNpcSpellDuration(StreamWriter fileWriter, string spellDuration)
        {
            if (string.IsNullOrEmpty(spellDuration)) return;
            switch (spellDuration)
            {
                case "1 day/level":
                    fileWriter.Write(" --set spellduration [[@{caster2_level}]] days");
                    break;
                case "1 day/level (D)":
                    fileWriter.Write(" --set spellduration [[@{caster2_level}]] days (D)");
                    break;
                case "1 day/level or until discharged (see text)":
                    fileWriter.Write(" --set spellduration [[@{caster2_level}]] days or until discharged (see text)");
                    break;
                case "1 day/level or until fulfilled":
                    fileWriter.Write(" --set spellduration [[@{caster2_level}]] days or until fulfilled");
                    break;
                case "1 day/level; see text":
                    fileWriter.Write(" --set spellduration [[@{caster2_level}]] days; see text");
                    break;
                case "1 hour/level":
                    fileWriter.Write(" --set spellduration [[@{caster2_level}]] hours");
                    break;
                case "1 hour/level (D)":
                    fileWriter.Write(" --set spellduration [[@{caster2_level}]] hours (D)");
                    break;
                case "1 hour/level ; see text":
                    fileWriter.Write(" --set spellduration [[@{caster2_level}]] hours; see text");
                    break;
                case "1 hour/level or 1 day/level (see text)":
                    fileWriter.Write(
                        " --set spellduration [[@{caster2_level}]] hours or [[@{caster2_level}]] days; see text");
                    break;
                case "1 hour/level or instantaneous (see text)":
                    fileWriter.Write(" --set spellduration [[@{caster2_level}]] hours or instantaneous; see text");
                    break;
                case "1 hour/level or until completed":
                    fileWriter.Write(" --set spellduration [[@{caster2_level}]] hours or until completed");
                    break;
                case "1 min./level":
                    fileWriter.Write(" --set spellduration [[@{caster2_level}]] minutes");
                    break;
                case "1 min./level (D)":
                    fileWriter.Write(" --set spellduration [[@{caster2_level}]] minutes (D)");
                    break;
                case "1 minute/level":
                    fileWriter.Write(" --set spellduration [[@{caster2_level}]] minutes");
                    break;
                case "1 minute/level (D)":
                    fileWriter.Write(" --set spellduration [[@{caster2_level}]] minutes (D)");
                    break;
                case "1 minute/level (D), special see below":
                    fileWriter.Write(" --set spellduration [[@{caster2_level}]] minutes (D); special, see text");
                    break;
                case "1 minute/level (see text)":
                    fileWriter.Write(" --set spellduration [[@{caster2_level}]] minutes; see text");
                    break;
                case "1 minute/level; see text":
                    fileWriter.Write(" --set spellduration [[@{caster2_level}]] minutes; see text");
                    break;
                case "1 round/3 levels":
                    fileWriter.Write(" --set spellduration [[{1,floor((@{caster2_level}+0)/3)}kh1]] rounds");
                    break;
                case "1 round/level":
                    fileWriter.Write(" --set spellduration [[@{caster2_level}]] rounds");
                    break;
                case "1 round/level (D)":
                    fileWriter.Write(" --set spellduration [[@{caster2_level}]] rounds (D)");
                    break;
                case "1 round/level (see text)":
                    fileWriter.Write(" --set spellduration [[@{caster2_level}]] rounds; see text");
                    break;
                case "1 round/level or 1 hour/level; see text":
                    fileWriter.Write(
                        " --set spellduration [[@{caster2_level}]] rounds or [[@{caster2_level}]] hours; see text");
                    break;
                case "1 round/level or 1 round; see text":
                    fileWriter.Write(" --set spellduration [[@{caster2_level}]] rounds or 1 round; see text");
                    break;
                case "1 round/level or 1 round; see text for cause fear":
                    fileWriter.Write(" --set spellduration [[@{caster2_level}]] rounds or 1 round; see text");
                    break;
                case "1 round/level or permanent; see text":
                    fileWriter.Write(" --set spellduration [[@{caster2_level}]] rounds or permanent; see text");
                    break;
                case "1 round/level or until discharged":
                    fileWriter.Write(" --set spellduration [[@{caster2_level}]] rounds or until discharged");
                    break;
                case "1 round/level or until discharged (D)":
                    fileWriter.Write(" --set spellduration [[@{caster2_level}]] rounds or until discharged (D)");
                    break;
                case "1 round/level; see text":
                    fileWriter.Write(" --set spellduration [[@{caster2_level}]] rounds; see text");
                    break;
                case "10 min./level":
                    fileWriter.Write(" --set spellduration [[@{caster2_level}*10]] minutes");
                    break;
                case "10 min./level (D)":
                    fileWriter.Write(" --set spellduration [[@{caster2_level}]] minutes (D)");
                    break;
                case "10 minutes or concentration (up to 1 round/level); see text":
                    fileWriter.Write(
                        " --set spellduration 10 minutes or concentration (up to [[@{caster2_level}]] rounds); see text");
                    break;
                case "10 minutes/level":
                    fileWriter.Write(" --set spellduration [[@{caster2_level}*10]] minutes");
                    break;
                case "10 minutes/level (D)":
                    fileWriter.Write(" --set spellduration [[@{caster2_level}*10]] minutes (D)");
                    break;
                case "10 minutes/level, then 1 hour/level or until completed (D); see text":
                    fileWriter.Write(
                        " --set spellduration [[@{caster2_level}*10]] minutes, then [[@{caster2_level}]] hours or until completed (D); see text");
                    break;
                case "2 hours/level":
                    fileWriter.Write(" --set spellduration [[@{caster2_level}*2]] hours");
                    break;
                case "2 hours/level; see text":
                    fileWriter.Write(" --set spellduration [[@{caster2_level}*2]] hours; see text");
                    break;
                case "4d4 rounds (D)":
                    fileWriter.Write(" --set spellduration [[4d4]] rounds (D)");
                    break;
                case "concentration + 1 round/level":
                    fileWriter.Write(" --set spellduration concentration + [[@{caster2_level}]] rounds");
                    break;
                case "concentration + 1 round/level (D)":
                    fileWriter.Write(" --set spellduration concentration + [[@{caster2_level}]] rounds (D)");
                    break;
                case "concentration, up to 1 min./level (D)":
                    fileWriter.Write(" --set spellduration concentration, up to [[@{caster2_level}]] minutes (D)");
                    break;
                case "concentration, up to 1 round/level":
                    fileWriter.Write(" --set spellduration concentration, up to [[@{caster2_level}]] rounds");
                    break;
                case "instantaneous and 1 round/level":
                    fileWriter.Write(" --set spellduration instantaneous and [[@{caster2_level}]] rounds");
                    break;
                case "instantaneous or 1 hour/level (D)":
                    fileWriter.Write(" --set spellduration instantanous or [[@{caster2_level}]] hours (D)");
                    break;
                case "instantaneous or 10 min./level; see text":
                    fileWriter.Write(" --set spellduration instantanous or [[@{caster2_level}*10]] minutes; see text");
                    break;
                case "until landing or 1 round/level":
                    fileWriter.Write(" --set spellduration until landing or [[@{caster2_level}]] rounds");
                    break;
                case "up to 1 round/level (see text)":
                    fileWriter.Write(" --set spellduration up to [[@{caster2_level}]] rounds; see text");
                    break;
                default:
                    fileWriter.Write($" --set spellduration {spellDuration}");
                    break;
            }
        }

        public static void SetNpcSpellTarget(StreamWriter fileWriter, string spellTargets)
        {
            if (string.IsNullOrEmpty(spellTargets)) return;
            //Believe me, this took a while.
            switch (spellTargets)
            {
                case "1 ally/level":
                    fileWriter.Write(" --set spelltargets [[@{caster2_level}]] allies");
                    break;
                case "1 creature/3 levels no two of which can be more than 30 ft. apart":
                    fileWriter.Write(
                        " --set spelltargets [[{1,floor((@{caster2_level}+0)/3)}kh1]] allies, no two of which can be more than 30 ft. apart");
                    break;
                case "1 creature/level":
                    fileWriter.Write(" --set spelltargets [[@{caster2_level}]] creatures");
                    break;
                case "1 creature/level, no two of which can be more than 30 feet apart":
                    fileWriter.Write(
                        " --set spelltargets [[@{caster2_level}]] creatures, no two of which can be more than 30 ft. apart");
                    break;
                case "1 creature/level, no two of which may be more than 30 ft. apart":
                    fileWriter.Write(
                        " --set spelltargets [[@{caster2_level}]] creatures, no two of which can be more than 30 ft. apart");
                    break;
                case "1 creature/level; see text":
                    fileWriter.Write(" --set spelltargets [[@{caster2_level}]] creatures; see text");
                    break;
                case "1 cu. ft./2 levels of liquid (see text)":
                    fileWriter.Write(
                        " --set spelltargets [[{1,floor((@{caster2_level}+0)/2)}kh1]] cu. ft. of liquid (see text)");
                    break;
                case "1 cu. ft./level of contaminated food and water":
                    fileWriter.Write(" --set spelltargets [[@{caster2_level}]] cu. ft. of contaminated food and water");
                    break;
                case "1 cu. ft./level of food and water or one potion; see text":
                    fileWriter.Write(
                        " --set spelltargets [[@{caster2_level}]] cu. ft. of contaminated food and water or one potion; see text");
                    break;
                case "1 gallon of water/level":
                    fileWriter.Write(" --set spelltargets [[@{caster2_level}]] gallon of water");
                    break;
                case "1 improvised weapon/level":
                    fileWriter.Write(" --set spelltargets [[@{caster2_level}]] improvised weapons");
                    break;
                case "1 object of up to 1 cubic ft./level":
                    fileWriter.Write(" --set spelltargets 1 object of up to [[@{caster2_level}]] cu. ft.");
                    break;
                case "1 object weighing up to 1 pound/level":
                    fileWriter.Write(" --set spelltargets 1 object weighing up to [[@{caster2_level}]] pounds");
                    break;
                case "1 pint of water/level":
                    fileWriter.Write(" --set spelltargets [[@{caster2_level}]] pints of water");
                    break;
                case "1 potion touched/level":
                    fileWriter.Write(" --set spelltargets [[@{caster2_level}]] potions touched");
                    break;
                case "1 Small wooden object/level, all within a 20-ft. radius":
                    fileWriter.Write(
                        " --set spelltargets [[@{caster2_level}]] Small wooden objects, all within a 20-ft. radius; see text");
                    break;
                case "1 Small wooden object/level, all within a 20-ft. radius; see text":
                    fileWriter.Write(
                        " --set spelltargets [[@{caster2_level}]] Small wooden objects, all within a 20-ft. radius");
                    break;
                case "1 weapon, suit or armor, shield, tool, or skill kit touched/5 levels":
                    fileWriter.Write(
                        " --set spelltargets [[{1,floor((@{caster2_level}+0)/5)}kh1]] weapons, suits of armor, shields, tools, or skill kit touched");
                    break;
                case "5-ft.-wide, 60-ft.-deep extradimensional hole, up to 5  ft. long per level (S)":
                    fileWriter.Write(
                        " --set spelltargets 5-ft.-wide, 60-ft.-deep extradimensional hole up to [[@{caster2_level}*5]] ft. long (S)");
                    break;
                case "a creature or object weighing up to 100 lbs./level":
                    fileWriter.Write(
                        " --set spelltargets a creature or object weighing up to [[@{caster2_level}*100]]");
                    break;
                case "area of river up to 5 ft. wide/2 levels and 10 ft. long/level":
                    fileWriter.Write(
                        " --set spelltargets area of river up to [[({1,floor((@{caster2_level}+0)/2)}kh1)*5]] ft. wide and [[@{caster2_level}*10]] ft. long");
                    break;
                case "corpse of creature whose total number of HD does not exceed your caster level":
                    fileWriter.Write(
                        " --set spelltargets corpse of creature whose total number of HD does not exceed [[@{caster2_level}]]");
                    break;
                case "creature one creature/level, no two of which can be more than 30 ft. apart":
                    fileWriter.Write(
                        " --set spelltargets [[@{caster2_level}]] creatures, no two of which can be more than 30 ft. apart");
                    break;
                case "creature or creatures touched (up to one per level)":
                    fileWriter.Write(" --set spelltargets [[@{caster2_level}]] creatures touched");
                    break;
                case "creature or creatures touched (up to one/level)":
                    fileWriter.Write(" --set spelltargets [[@{caster2_level}]] creatures touched");
                    break;
                case "creature or object of up to 1 cu. ft./level touched":
                    fileWriter.Write(
                        " --set spelltargets creature or object of up to [[@{caster2_level}]] cu. ft. touched");
                    break;
                case "creatures touched, up to one/level":
                    fileWriter.Write(" --set spelltargets [[@{caster2_level}]] creatures touched");
                    break;
                case "door, chest, or portal touched, up to 30 sq. ft./level in size":
                    fileWriter.Write(
                        " --set spelltargets door, chest, or portal touched, up to [[@{caster2_level}*30]] sq. ft. in size");
                    break;
                case "lava wall whose area is up to one 5-ft. square/level (S)":
                    fileWriter.Write(
                        " --set spelltargets lava wall whose area is up to [[@{caster2_level}]] 5-ft. squares");
                    break;
                case "living creatures touched (up to one per level)":
                    fileWriter.Write(" --set spelltargets up to [[@{caster2_level}]] living creatures touched");
                    break;
                case
                "metal equipment of one creature per two levels, no two of which can be more than 30 ft. apart; or 25 lbs. of metal/level, all of which must be within a 30-ft. circle"
                :
                    fileWriter.Write(
                        " --set spelltargets metal equipment of [[{1,floor((@{caster2_level}+0)/2)}kh1]] creatures, no two of which can be more than 30 ft. apart; or [[@{caster2_level}*25]] lbs. of metal, all of which must be within a 30-ft. circle");
                    break;
                case
                "metal equipment of one creature per two levels, no two of which can be more than 30 ft. apart; or 25 lbs. of metal/level, none of which can be more than 30 ft. away from any of the rest"
                :
                    fileWriter.Write(
                        " --set spelltargets metal equipment of [[{1,floor((@{caster2_level}+0)/2)}kh1]] creatures, no two of which can be more than 30 ft. apart; or [[@{caster2_level}*25]] lbs. of metal, all of which must be within a 30-ft. circle");
                    break;
                case "multiple objects totaling up to 1 cubic ft./level, no two of which can be more than 30 ft. apart":
                    fileWriter.Write(
                        " --set spelltargets multiple objects totaling up to [[@{caster2_level}]] cu. ft., no two of which can be more than 30 ft. apart");
                    break;
                case "object touched or up to 5 sq. ft./level":
                    fileWriter.Write(" --set spelltargets object touched, or up to [[@{caster2_level}*5]] sq.ft.");
                    break;
                case "object touched, up to 1 cubic foot per level":
                    fileWriter.Write(" --set spelltargets object touched, or up to [[@{caster2_level}*1]] cu.ft.");
                    break;
                case "object touched, weighing up to 5 lbs./level":
                    fileWriter.Write(" --set spelltargets object touched, weighing up to [[@{caster2_level}*5]] lbs.");
                    break;
                case "one animal/level, no two of which can be more than 30 ft. apart":
                    fileWriter.Write(
                        " --set spelltargets [[@{caster2_level}]] animals, no two of which can be more than 30 ft. apart");
                    break;
                case "one broken object of up to 2 lbs./level":
                    fileWriter.Write(" --set spelltargets one broken object of up to [[@{caster2_level}*2]] lbs.");
                    break;
                case "one chamber and up to 10 cu. ft. of goods/caster level":
                    fileWriter.Write(
                        " --set spelltargets one chamber and up to [[@{caster2_level}*10]] cu.ft. of goods");
                    break;
                case "one chest and up to 1 cu. ft. of goods/caster level":
                    fileWriter.Write(" --set spelltargets one chest and up to [[@{caster2_level}]] cu.ft. of goods");
                    break;
                case "one cloud-like effect, up to one 10-ft. cube/level":
                    fileWriter.Write(
                        " --set spelltargets one cloud-like effect, up to [[@{caster2_level}]] 10-ft. cubes");
                    break;
                case "one creature or object/level touched":
                    fileWriter.Write(" --set spelltargets one creature, or [[@{caster2_level}]] objects touched");
                    break;
                case "one creature or object/level, no two of which can be more than 30 ft. apart":
                    fileWriter.Write(
                        " --set spelltargets one creature, or [[@{caster2_level}]] objects, no two of which can be more than 30 ft. apart");
                    break;
                case "one creature or one object up to 5 lbs./level":
                    fileWriter.Write(
                        " --set spelltargets one creature or one object up to [[@{caster2_level}*5]] lbs.");
                    break;
                case "one creature per level":
                    fileWriter.Write(" --set spelltargets [[@{caster2_level}]] creatures");
                    break;
                case "one creature per level, no two of which can be more than 30 ft. apart":
                    fileWriter.Write(
                        " --set spelltargets [[@{caster2_level}]] creatures, no two of which can be more than 30 ft. apart");
                    break;
                case "one creature per three levels, no two of which may be more than 30 ft. apart":
                    fileWriter.Write(
                        " --set spelltargets [[{1,floor((@{caster2_level}+0)/3)}kh1]] creatures, no two of which may be more than 30 ft. apart");
                    break;
                case "one creature per two levels, no two of which can be more than 30 ft. apart":
                    fileWriter.Write(
                        " --set spelltargets [[{1,floor((@{caster2_level}+0)/2)}kh1]] creatures, no two of which can be more than 30 ft. apart");
                    break;
                case
                "one creature plus one additional creature per 4 levels, no two of which can be more than 30 ft. apart":
                    fileWriter.Write(
                        " --set spelltargets one creature plus [[{1,floor((@{caster2_level}+0)/4)}kh1]] additional creatures, no two of which can be more than 30 ft. apart");
                    break;
                case
                "one creature plus one additional creature per four levels, no two of which can be more than 30 ft. apart"
                :
                    fileWriter.Write(
                        " --set spelltargets one creature plus [[{1,floor((@{caster2_level}+0)/4)}kh1]] additional creatures, no two of which can be more than 30 ft. apart");
                    break;
                case
                "one creature summoned by a spell or spell-like ability/level, no two of which can be more than 30 ft. apart"
                :
                    fileWriter.Write(
                        " --set spelltargets [[@{caster2_level}]] creatures summoned by a spell or spell-like ability, no two of which can be more than 30 ft. apart");
                    break;
                case "one creature touched/2 levels":
                    fileWriter.Write(" --set spelltargets [[{1,floor((@{caster2_level}+0)/2)}kh1]] creatures touched");
                    break;
                case "one creature touched/level":
                    fileWriter.Write(" --set spelltargets [[@{caster2_level}]] creatures touched");
                    break;
                case "one creature, or one nonmagical object of up to 100 cu. ft./level":
                    fileWriter.Write(
                        " --set spelltargets one creature, or one nonmagical object of up to [[@{caster2_level}*100]] cu.ft.");
                    break;
                case "one creature, or one object weighing up to 20 lbs./level":
                    fileWriter.Write(
                        " --set spelltargets one creature, or one object weighing up to [[@{caster2_level}*20]] lbs.");
                    break;
                case "one creature, plus one additional creature for every 3  caster levels":
                    fileWriter.Write(
                        " --set spelltargets one creature plus [[{1,floor((@{caster2_level}+0)/3)}kh1]] additional creatures");
                    break;
                case "one creature/2 levels":
                    fileWriter.Write(" --set spelltargets [[{1,floor((@{caster2_level}+0)/2)}kh1]] creatures");
                    break;
                case "one creature/2 levels (no two of which may be more than 30 ft. apart)":
                    fileWriter.Write(
                        " --set spelltargets [[{1,floor((@{caster2_level}+0)/2)}kh1]] creatures, no two of which can be more than 30 ft. apart");
                    break;
                case "one creature/2 levels, no two of which can be more than 30 feet apart":
                    fileWriter.Write(
                        " --set spelltargets [[{1,floor((@{caster2_level}+0)/2)}kh1]] creatures, no two of which can be more than 30 ft. apart");
                    break;
                case "one creature/2 levels, no two of which can be more than 30 ft. apart":
                    fileWriter.Write(
                        " --set spelltargets [[{1,floor((@{caster2_level}+0)/2)}kh1]] creatures, no two of which can be more than 30 ft. apart");
                    break;
                case "one creature/2 levels, no two of which may be more than 30 ft. apart (see text)":
                    fileWriter.Write(
                        " --set spelltargets [[{1,floor((@{caster2_level}+0)/2)}kh1]] creatures, no two of which can be more than 30 ft. apart; see text");
                    break;
                case "one creature/3 levels":
                    fileWriter.Write(" --set spelltargets [[{1,floor((@{caster2_level}+0)/3)}kh1]] creatures");
                    break;
                case "one creature/3 levels, no two of which can be more than 30 ft. apart":
                    fileWriter.Write(
                        " --set spelltargets [[{1,floor((@{caster2_level}+0)/3)}kh1]] creatures, no two of which can be more than 30 ft. apart; see text");
                    break;
                case "one creature/4 levels":
                    fileWriter.Write(
                        " --set spelltargets [[{1,floor((@{caster2_level}+0)/4)}kh1]] creatures, no two of which can be more than 30 ft. apart; see text");
                    break;
                case "one creature/4 levels, no two of which can be more than 30 ft. apart":
                    fileWriter.Write(
                        " --set spelltargets [[{1,floor((@{caster2_level}+0)/4)}kh1]] creatures, no two of which can be more than 30 ft. apart; see text");
                    break;
                case "one creature/level":
                    fileWriter.Write(" --set spelltargets [[@{caster2_level}]] creatures");
                    break;
                case "one creature/level (all of which must be within 30 feet)":
                    fileWriter.Write(
                        " --set spelltargets [[@{caster2_level}]] creatures, no two of which can be more than 30 ft. apart");
                    break;
                case "one creature/level in a 20-ft.-radius burst centered  on you":
                    fileWriter.Write(
                        " --set spelltargets [[@{caster2_level}]] creatures in a 20-ft.-radius burst centered on you");
                    break;
                case "one creature/level in a 20-ft.-radius burst centered on you":
                    fileWriter.Write(
                        " --set spelltargets [[@{caster2_level}]] creatures in a 20-ft.-radius burst centered on you");
                    break;
                case "one creature/level touched":
                    fileWriter.Write(" --set spelltargets [[@{caster2_level}]] creatures touched");
                    break;
                case "one creature/level, no two of which can be more  than 30 ft. apart":
                    fileWriter.Write(
                        " --set spelltargets [[@{caster2_level}]] creatures, no two of which can be more than 30 ft. apart");
                    break;
                case "one creature/level, no two of which can be more than 20 feet apart":
                    fileWriter.Write(
                        " --set spelltargets [[@{caster2_level}]] creatures, no two of which can be more than 20 ft. apart");
                    break;
                case "one creature/level, no two of which can be more than 30 ft. apart":
                    fileWriter.Write(
                        " --set spelltargets [[@{caster2_level}]] creatures, no two of which can be more than 30 ft. apart");
                    break;
                case "one creature/level, no two of which can be more than 30 ft. apart.":
                    fileWriter.Write(
                        " --set spelltargets [[@{caster2_level}]] creatures, no two of which can be more than 30 ft. apart");
                    break;
                case "one creature/level, no two of which may be more than 30 ft. apart":
                    fileWriter.Write(
                        " --set spelltargets [[@{caster2_level}]] creatures, no two of which can be more than 30 ft. apart");
                    break;
                case "one daemon per 4 caster levels":
                    fileWriter.Write(" --set spelltargets [[{1,floor((@{caster2_level}+0)/4)}kh1]] daemons");
                    break;
                case "one destroyed construct of up to 2 HD/level":
                    fileWriter.Write(" --set spelltargets one destroyed construct of up to [[@{caster2_level}*2]] HD");
                    break;
                case "one door, box, or chest with an area of up to 10 sq. ft./level":
                    fileWriter.Write(
                        " --set spelltargets one door, box, or chest with an area of up to [[@{caster2_level}*10]] sq.ft.");
                    break;
                case "one Gargantuan, Huge, or Large plant per three caster levels":
                    fileWriter.Write(
                        " --set spelltargets [[{1,floor((@{caster2_level}+0)/3)}kh1]] Gargantuan, Huge, or Large plants");
                    break;
                case "one good-aligned creature/3 levels":
                    fileWriter.Write(
                        " --set spelltargets [[{1,floor((@{caster2_level}+0)/3)}kh1]] good-aligned creature");
                    break;
                case "one humanoid creature per level":
                    fileWriter.Write(" --set spelltargets [[@{caster2_level}]] humanoid creatures");
                    break;
                case "One humanoid creature/level, no two of which can be more than 30 ft. apart":
                    fileWriter.Write(
                        " --set spelltargets [[@{caster2_level}]] humanoid creatures, no two of which can be more than 30 ft. apart");
                    break;
                case
                "one humanoid, magical beast, or monstrous humanoid/ level, no two of which can be more than 30 feet apart"
                :
                    fileWriter.Write(
                        " --set spelltargets [[@{caster2_level}]] humanoid, magical beast, or monstrous humanoids, no two of which can be more than 30 ft. apart");
                    break;
                case "one incorporeal creature/level, no two of which can be more than 30 ft. apart":
                    fileWriter.Write(
                        " --set spelltargets [[@{caster2_level}]] incorporeal creatures, no two of which ca be more than 30 ft. apart");
                    break;
                case "one Large plant per three caster levels":
                    fileWriter.Write(" --set spelltargets [[{1,floor((@{caster2_level}+0)/3)}kh1]] Large plants");
                    break;
                case "one Large plant per three caster levels or all plants within  range; see text":
                    fileWriter.Write(
                        " --set spelltargets [[{1,floor((@{caster2_level}+0)/3)}kh1]] Large plants, or all plants within range; see text");
                    break;
                case "one living creature per three levels, no two of which can be more than 30 ft. apart":
                    fileWriter.Write(
                        " --set spelltargets [[{1,floor((@{caster2_level}+0)/3)}kh1]] living creatures, no two of which can be more than 30 ft. apart");
                    break;
                case "one living creature touched per three levels":
                    fileWriter.Write(
                        " --set spelltargets [[{1,floor((@{caster2_level}+0)/4)}kh1]] living creatures touched");
                    break;
                case "one living creature/2 levels (no two of which may be more than 30 feet apart)":
                    fileWriter.Write(
                        " --set spelltargets [[{1,floor((@{caster2_level}+0)/2)}kh1]] living creatures, no two of which may be more than 30 ft. apart");
                    break;
                case
                "one living creature/level that is a reptile, has the dragon type, or has the reptilian subtype, and also has a natural armor bonus of at least +1"
                :
                    fileWriter.Write(
                        " --set spelltargets [[@{caster2_level}]] creatures that is a reptile, has the dragon type, or has the reptilian subtype, and also has a natural armor bonus of at least +1");
                    break;
                case
                "one living creature/level that is a reptile, has the dragon type, or has the reptilian subtype, and that also has a natural armor bonus of at least +1"
                :
                    fileWriter.Write(
                        " --set spelltargets [[@{caster2_level}]] creatures that is a reptile, has the dragon type, or has the reptilian subtype, and also has a natural armor bonus of at least +1");
                    break;
                case "one living creature/level within a 40-ft.-radius spread":
                    fileWriter.Write(" --set spelltargets [[@{caster2_level}]] creatures in a 40-ft.-radius spread");
                    break;
                case "one living creature/level, no two of which can be more than 30 ft. apart":
                    fileWriter.Write(
                        " --set spelltargets [[@{caster2_level}]] living creatures, no two of which can be more than 30 ft. apart");
                    break;
                case "one living creature/level, no two of which can be more than 60 feet apart":
                    fileWriter.Write(
                        " --set spelltargets [[@{caster2_level}]] living creatures, no two of which can be more than 60 ft. apart");
                    break;
                case "one living creature/level, no two of which may be more than 30 ft. apart":
                    fileWriter.Write(
                        " --set spelltargets [[@{caster2_level}]] living creatures, no two of which can be more than 30 ft. apart");
                    break;
                case "one location (up to a 10-ft. cube/level) or one object":
                    fileWriter.Write(
                        " --set spelltargets one location (up to [[@{caster2_level}]] 10-ft. cubes) or one object");
                    break;
                case "one means of closure/level, no two of which can be more than 30 feet apart":
                    fileWriter.Write(
                        " --set spelltargets [[@{caster2_level}]] means of closure, no two of which can be more than 30 ft. apart");
                    break;
                case
                "one Medium or smaller freefalling object or creature/level, no two of which may be more than 20 ft. apart"
                :
                    fileWriter.Write(
                        " --set spelltargets [[@{caster2_level}]] Medium or smaller freefalling objects or creatures, no two of which may be more than 20 ft. apart");
                    break;
                case "one Medium or smaller object or creature/level, no two of which can be more than 20 ft. apart":
                    fileWriter.Write(
                        " --set spelltargets [[@{caster2_level}]] Medium or smaller objects or creatures, no two of which may be more than 20 ft. apart");
                    break;
                case "One metal weapon/level, no two of which can be more than 30 ft. apart":
                    fileWriter.Write(
                        " --set spelltargets [[@{caster2_level}]] metal weapons, no two of which can be more than 30 ft. apart");
                    break;
                case "one mindless undead creature/level, no two of which can be more than 30 ft. apart":
                    fileWriter.Write(
                        " --set spelltargets [[@{caster2_level}]] mindless undead creatures, no two of which can be more than 30 ft. apart");
                    break;
                case "one non-mutated sahuagin/level, no two of which can be more than 30 ft. apart":
                    fileWriter.Write(
                        " --set spelltargets [[@{caster2_level}]] non-mutated sahuagin, no two of which can be more than 30 ft. apart");
                    break;
                case "one non-mythic creature/3 levels":
                    fileWriter.Write(
                        " --set spelltargets [[{1,floor((@{caster2_level}+0)/3)}kh1]] non-mythic creatures");
                    break;
                case "one object of no more than 10 cu. ft./level":
                    fileWriter.Write(" --set spelltargets one object of no more than [[@{caster2_level}*10]] cu.ft.");
                    break;
                case "one object of up to 1 lb./level":
                    fileWriter.Write(" --set spelltargets one object of up to [[@{caster2_level}]] lbs.");
                    break;
                case "one object of up to 10 cu. ft./level or one construct creature of any size":
                    fileWriter.Write(
                        " --set spelltargets one object of up to [[@{caster2_level}*10]] or one construct creature of any size");
                    break;
                case "one object of up to 5 lb./level":
                    fileWriter.Write(" --set spelltargets one object of up to [[@{caster2_level}*5]] lbs.");
                    break;
                case "one object or creature per caster level":
                    fileWriter.Write(" --set spelltargets [[@{caster2_level}]] objects or creatures");
                    break;
                case "one object touched of up to 100 lbs./level":
                    fileWriter.Write(" --set spelltargets one object touched of up to [[@{caster2_level}*100]] lbs.");
                    break;
                case "one object weighing no more than 1 lb./level":
                    fileWriter.Write(" --set spelltargets one object weighing no more than [[@{caster2_level}]] lbs.");
                    break;
                case "one object weighing no more than 5 lbs./level":
                    fileWriter.Write(
                        " --set spelltargets one object weighing no more than [[@{caster2_level}*5]] lbs.");
                    break;
                case "one object/2 levels or 1 10-ft. square/2 levels; see text":
                    fileWriter.Write(
                        " --set spelltargets [[{1,floor((@{caster2_level}+0)/2)}kh1]] objects, or [[{1,floor((@{caster2_level}+0)/2)}kh1]] 10-ft. squares; see text");
                    break;
                case "one or more Medium creatures/level, no two of which can be more than 30 ft. apart":
                    fileWriter.Write(
                        " --set spelltargets [[@{caster2_level}]] Medium creatures, no two of which can be more than 30 ft. apart");
                    break;
                case "one or more objects touched, up to 1 lb./level":
                    fileWriter.Write(
                        " --set spelltargets one or more objects touched, up to [[@{caster2_level}]] lbs.");
                    break;
                case "one portal, up to 20 sq. ft./level":
                    fileWriter.Write(" --set spelltargets one portal, up to [[@{caster2_level}*20]] sq.ft.");
                    break;
                case
                "one primary target plus one additional target/3 levels  (each of which must be within 15 ft. of the primary target)"
                :
                    fileWriter.Write(
                        " --set spelltargets one primary target, plus [[{1,floor((@{caster2_level}+0)/3)}kh1]] additional targets (each of which must be within 15 ft. of the primary target)");
                    break;
                case
                "one primary target, plus one secondary target/level (each of which must be within 30 ft. of the primary target)"
                :
                    fileWriter.Write(
                        " --set spelltargets one primary target, plus [[{1,floor((@{caster2_level}+0)/3)}kh1]] additional targets (each of which must be within 30 ft. of the primary target)");
                    break;
                case "one ropelike object, length up to 50 ft. + 5 ft./level":
                    fileWriter.Write(
                        " --set spelltargets one ropelike object, length up to [[50+(@{caster2_level}*5)]] ft.");
                    break;
                case "one rope-like object, length up to 50 ft. + 5 ft./level; see text":
                    fileWriter.Write(
                        " --set spelltargets one ropelike object, length up to [[50+(@{caster2_level}*5)]] ft.; see text");
                    break;
                case "one Small object per caster level; see text":
                    fileWriter.Write(" --set spelltargets [[@{caster2_level}]] Small objects; see text");
                    break;
                case "one Small or smaller creature/level, no two of which can be more than 30 ft. apart":
                    fileWriter.Write(
                        " --set spelltargets [[@{caster2_level}]] Small or smaller creatures, no two of which can be more than 30 ft. apart");
                    break;
                case "one summoned creature you control/level, no two of which can be more than 30 ft. apart":
                    fileWriter.Write(
                        " --set spelltargets [[@{caster2_level}]] summoned creatures you control, no two of which can be more than 30 ft. apart");
                    break;
                case "one touched creature/level":
                    fileWriter.Write(" --set spelltargets [[@{caster2_level}]] touched creatures");
                    break;
                case "one touched holy symbol per 3 caster levels":
                    fileWriter.Write(
                        " --set spelltargets [[{1,floor((@{caster2_level}+0)/3)}kh1]] touched holy symbols");
                    break;
                case "one touched object of up to 10 pounds/level":
                    fileWriter.Write(" --set spelltargets one touched object of up to [[@{caster2_level}]] lbs.");
                    break;
                case "one touched object of up to 2 cu. ft./level":
                    fileWriter.Write(" --set spelltargets one touched object of up to [[@{caster2_level}*2]] cu.ft.");
                    break;
                case "one touched object of up to 50 lbs./level and 3 cu. ft./level":
                    fileWriter.Write(
                        " --set spelltargets one touched object of up to [[@{caster2_level}*50]] lbs. and [[@{caster2_level}*3]] cu.ft.");
                    break;
                case "one touched object weighing up to 5 lbs./level":
                    fileWriter.Write(
                        " --set spelltargets one touched object weighing up to [[@{caster2_level}*5]] lbs.");
                    break;
                case "one touched piece of wood no larger than 10 cu. ft. + 1 cu. ft./level":
                    fileWriter.Write(
                        " --set spelltargets one touched piece of wood no larger than [[10+@{caster2_level}]] ft.");
                    break;
                case "one undead creature/level, no two of which can be more than 30 ft. apart":
                    fileWriter.Write(
                        " --set spelltargets [[@{caster2_level}]] undead creatures, no two of which can be more than 30 ft. apart");
                    break;
                case "one weapon per level":
                    fileWriter.Write(" --set spelltargets [[@{caster2_level}]] weapons");
                    break;
                case "one weapon/3 levels":
                    fileWriter.Write(" --set spelltargets [[{1,floor((@{caster2_level}+0)/3)}kh1]] weapons");
                    break;
                case "one weapon/level, no two of which can be more than 20 ft. apart":
                    fileWriter.Write(
                        " --set spelltargets [[@{caster2_level}]] weapons, no two of which can be more than 20 ft. apart");
                    break;
                case "one willing ally/5 levels":
                    fileWriter.Write(" --set spelltargets [[{1,floor((@{caster2_level}+0)/5)}kh1]] willing allies");
                    break;
                case "one willing creature or object (up to a 2-ft. cube/level) touched":
                    fileWriter.Write(
                        " --set spelltargets one willing creature or object (up to [[@{caster2_level}]] 2-ft. cubes) touched");
                    break;
                case "one willing creature plus one/2 caster levels, no two of which can be more than 30 ft. apart":
                    fileWriter.Write(
                        " --set spelltargets [[{1,floor((@{caster2_level}+0)/2)}kh1]] willing creatures, no two of which can be more than 30 ft. apart");
                    break;
                case "one willing creature/level":
                    fileWriter.Write(" --set spelltargets [[@{caster2_level}]] willing creatures");
                    break;
                case "one willing living creature per 2 levels, no two of which may be more than 30 ft. apart":
                    fileWriter.Write(
                        " --set spelltargets [[{1,floor((@{caster2_level}+0)/2)}kh1]] willing living creatures, no two of which may be more than 30 ft. apart");
                    break;
                case "one willing living creature per three levels, no two of which may be more than 30 ft. apart":
                    fileWriter.Write(
                        " --set spelltargets [[{1,floor((@{caster2_level}+0)/3)}kh1]] willing living creatures, no two of which may be more than 30 ft. apart");
                    break;
                case "one willing living creature per three levels, no two of which may be more than 30 ft. apart.":
                    fileWriter.Write(
                        " --set spelltargets [[{1,floor((@{caster2_level}+0)/3)}kh1]] willing living creatures, no two of which may be more than 30 ft. apart");
                    break;
                case "one willing living creature/level, no two of which can be more than 30 ft. apart":
                    fileWriter.Write(
                        " --set spelltargets [[{1,floor((@{caster2_level}+0)/1)}kh1]] willing living creatures, no two of which may be more than 30 ft. apart");
                    break;
                case "one willing undead creature per 3 levels, no two of which can be more than 30 ft. apart":
                    fileWriter.Write(
                        " --set spelltargets [[{1,floor((@{caster2_level}+0)/3)}kh1]] willing living creatures, no two of which may be more than 30 ft. apart");
                    break;
                case "snow or snow-sculpted object touched, up to 5 cubic ft.  + 1 cubic ft./level":
                    fileWriter.Write(
                        " --set spelltargets snow or snow-sculpted object touched, up to [[5+@{caster2_level}]] cu.ft.");
                    break;
                case "stone or stone object touched, up to 10 cu. ft. + 1 cu. ft./level":
                    fileWriter.Write(
                        " --set spelltargets stone or stone object touched, up to [[10+@{caster2_level}]] cu.ft.");
                    break;
                case "touched nonmagical circle of vine, rope, or thong with a 2 ft. diameter + 2 ft./level":
                    fileWriter.Write(
                        " --set spelltargets touched nonmagical circle of vine, rope, or thong with a [[2+(@{caster2_level}*2)]]");
                    break;
                case
                "two willing creatures plus another creature per 6 levels, no two of which can be more than 30 feet apart"
                :
                    fileWriter.Write(
                        " --set spelltargets [[{1,floor((@{caster2_level}+0)/6)}kh1*2]] willing creatures, no two of which can be more than 30 ft. apart");
                    break;
                case "up to 1 HD/level of vermin, no two of which can be more than 30 ft. apart":
                    fileWriter.Write(
                        " --set spelltargets up to [[@{caster2_level}]] HD/level of vermin, no two of which can be more than 30 ft. apart");
                    break;
                case "up to 10 cu. ft./level; see text":
                    fileWriter.Write(" --set spelltargets up to [[@{caster2_level}*10]] cu.ft.; see text");
                    break;
                case "up to 2 HD/level of plant creatures, no two of which can be more than 30 ft. apart":
                    fileWriter.Write(
                        " --set spelltargets up to [[@{caster2_level}*2]] of plant creatures, no two of which can be more than 30 ft. apart");
                    break;
                case "up to 2 HD/level of undead creatures, no two of which can be more than 30 ft. apart":
                    fileWriter.Write(
                        " --set spelltargets up to [[@{caster2_level}*2]] HD of undead creatures, no two of which can be more than 30 ft. apart");
                    break;
                case "up to one creature per 3 caster levels":
                    fileWriter.Write(" --set spelltargets up to [[{1,floor((@{caster2_level}+0)/3)}kh1]] creatures");
                    break;
                case "up to one creature per level, all within 30 ft. of each other":
                    fileWriter.Write(
                        " --set spelltargets [[@{caster2_level}]] creatures, no two of which can be more than 30 ft. apart");
                    break;
                case "up to one creature touched per four levels":
                    fileWriter.Write(" --set spelltargets up to [[{1,floor((@{caster2_level}+0)/4)}kh1]] creatures");
                    break;
                case "up to one creature/3 levels, no two of which can be more than 30 ft. apart":
                    fileWriter.Write(
                        " --set spelltargets up to [[{1,floor((@{caster2_level}+0)/3)}kh1]] creatures, no two of which can be more than 30 ft. apart");
                    break;
                case "up to one creature/level within 180 feet":
                    fileWriter.Write(" --set spelltargets up to [[@{caster2_level}]] creatures within 180 ft.");
                    break;
                case "up to one creature/level, no two of which can be more than 30 feet apart":
                    fileWriter.Write(
                        " --set spelltargets up to [[@{caster2_level}]] creatures, no two of which can be more than 30 ft. apart");
                    break;
                case "up to one creature/level, no two of which can be more than 30 ft. apart":
                    fileWriter.Write(
                        " --set spelltargets up to [[@{caster2_level}]] creatures, no two of which can be more than 30 ft. apart");
                    break;
                case "up to one living creature/level, no two of which can be more than 30 feet apart":
                    fileWriter.Write(
                        " --set spelltargets up to [[@{caster2_level}]] living creatures, no two of which can be more than 30 ft. apart");
                    break;
                case "up to one natural or manufactured weapon per 3 caster levels":
                    fileWriter.Write(
                        " --set spelltargets up to [[{1,floor((@{caster2_level}+0)/3)}kh1]] natural or manufactured weapons");
                    break;
                case
                "up to one object per 3 caster levels, each weighing 10 lbs. or less whose longest dimension is 6 ft. or less"
                :
                    fileWriter.Write(
                        " --set spelltargets up to [[{1,floor((@{caster2_level}+0)/3)}kh1]] objects, each weighing 10 lbs. or less whose longest dimension is 6 ft. or less");
                    break;
                case "up to one touched creature/level":
                    fileWriter.Write(" --set spelltargets up to [[@{caster2_level}]] touched creatures");
                    break;
                case "up to one touched object per level weighing up to 5 lbs. each":
                    fileWriter.Write(
                        " --set spelltargets up to [[@{caster2_level}]] touched objects per level, weighing up to 5 lbs. each");
                    break;
                case "up to one weapon/level, no two of which can be more than 30 ft. apart":
                    fileWriter.Write(
                        " --set spelltargets up to [[@{caster2_level}]] weapons, no two of which can be more than 30 ft. apart");
                    break;
                case "up to one willing creature per level, all within 30 ft. of each other.":
                    fileWriter.Write(
                        " --set spelltargets up to [[@{caster2_level}]] willing creatures, no two of which can be more than 30 ft. apart");
                    break;
                case "willing, living creature or creatures touched or one creature/level (see text)":
                    fileWriter.Write(
                        " --set spelltargets willing, living creature or creatures touched or [[@{caster2_level}]] creatures; see text");
                    break;
                case "you and one ally/level, no two of which can be more than 30 ft. apart (see text)":
                    fileWriter.Write(
                        " --set spelltargets you and [[@{caster2_level}]] allies, no two of which can be more than 30 ft. apart; see text");
                    break;
                case "you and one creature/level":
                    fileWriter.Write(" --set spelltargets you and [[@{caster2_level}]] creatures");
                    break;
                case "you and one other touched creature per three levels":
                    fileWriter.Write(
                        " --set spelltargets you and [[{1,floor((@{caster2_level}+0)/3)}kh1]] touched creatures");
                    break;
                case "you and one touched creature per three levels":
                    fileWriter.Write(
                        " --set spelltargets you and [[{1,floor((@{caster2_level}+0)/3)}kh1]] touched creatures");
                    break;
                case "you and one willing creature/2 levels, all of which must be within 30 feet of you":
                    fileWriter.Write(
                        " --set spelltargets you and [[{1,floor((@{caster2_level}+0)/2)}kh1]] willing creatures, all of which must be within 30 ft. of you");
                    break;
                case "you and up to 1 willing creature/2 caster levels, no two of which can be more than 30 ft. apart":
                    fileWriter.Write(
                        " --set spelltargets you and up to [[{1,floor((@{caster2_level}+0)/2)}kh1]] willing creatures, no two of which can be more than 30 ft. apart");
                    break;
                case "you or a creature or object weighing no more than 100 lbs./level":
                    fileWriter.Write(
                        " --set spelltargets you or a creature or object weighting no more than [[@{caster2_level}*100]]");
                    break;
                case "you or one willing creature or one object (total weight up to 100 lbs./level)":
                    fileWriter.Write(
                        " --set spelltargets you or one willing creature or one object (total weight up to [[@{caster2_level}*100]] lbs.)");
                    break;
                case "you plus one additional willing creature touched per two caster levels":
                    fileWriter.Write(
                        " --set spelltargets you plus [[{1,floor((@{caster2_level}+0)/2)}kh1]] additional willing creatures touched");
                    break;
                case "you plus one creature/level, no two of which can be more than 30 ft. apart":
                    fileWriter.Write(
                        " --set spelltargets you plus [[@{caster2_level}]] creatures, no two of which can be more than 30 ft. apart");
                    break;
                case "you plus one touched creature/3 levels":
                    fileWriter.Write(
                        " --set spelltargets you plus [[{1,floor((@{caster2_level}+0)/3)}kh1]] touched creatures");
                    break;
                case "you plus one willing creature per 2 levels, no two of which can be more than 30 ft. apart":
                    fileWriter.Write(
                        " --set spelltargets you plus [[{1,floor((@{caster2_level}+0)/2)}kh1]] willing creatures, no two of which can be more than 30 ft. apart");
                    break;
                case "you plus one willing creature per 3 levels, no two of which can be more than 30 ft. part":
                    fileWriter.Write(
                        " --set spelltargets you plus [[{1,floor((@{caster2_level}+0)/3)}kh1]] willing creatures, no two of which can be more than 30 ft. apart");
                    break;
                case "you plus one willing creature per three levels, no two of which can be more than 30 ft. apart":
                    fileWriter.Write(
                        " --set spelltargets you plus [[{1,floor((@{caster2_level}+0)/3)}kh1]] willing creatures, no two of which can be more than 30 ft. apart");
                    break;
                case "you plus one willing creature/3 levels, no two of which can be more than 30 ft. apart":
                    fileWriter.Write(
                        " --set spelltargets you plus [[{1,floor((@{caster2_level}+0)/3)}kh1]] willing creatures, no two of which can be more than 30 ft. apart");
                    break;
                case "you plus one willing living creature per 3 levels, no two of which can be more than 30 ft. apart":
                    fileWriter.Write(
                        " --set spelltargets you plus [[{1,floor((@{caster2_level}+0)/3)}kh1]] willing creatures, no two of which can be more than 30 ft. apart");
                    break;
                default:
                    fileWriter.Write($" --set spelltargets {spellTargets}");
                    break;
            }
        }

        private static void SetNpcSpellArea(StreamWriter fileWriter, string spellArea)
        {
            if (string.IsNullOrEmpty(spellArea)) return;
            switch (spellArea)
            {
                case "1 creature/level":
                    fileWriter.Write(" --set spellarea [[@{caster2_level}]] creatures");
                    break;
                case "10-ft. cube/level (S)":
                    fileWriter.Write(" --set spellarea [[@{caster2_level}]] 10-ft. cubes");
                    break;
                case "10-ft. square/level; see text":
                    fileWriter.Write(" --set spellarea [[@{caster2_level}]] 10-ft. squares; see text");
                    break;
                case "30-ft. cube/level":
                    fileWriter.Write(" --set spellarea [[@{caster2_level}]] 30-ft. cubes");
                    break;
                case "40 ft./level radius cylinder 40 ft. high":
                    fileWriter.Write(" --set spellarea [[@{caster2_level}*40]] radius cylinder 40 ft. high");
                    break;
                case "60-ft. cube/level":
                    fileWriter.Write(" --set spellarea [[@{caster2_level}]] cube");
                    break;
                case "circle centered on you with a radius of 400 ft. + 40 ft./level":
                    fileWriter.Write(
                        " --set spellarea circle centered on you, with a radius of [[400+(@{caster2_level}*40)]] ft.");
                    break;
                case "circle centered on you, with a radius of 100 feet + 10 feet per level":
                    fileWriter.Write(
                        " --set spellarea circle centered on you, with a radius of [[100+(@{caster2_level}*10)]] ft.");
                    break;
                case "circle, centered on you, with a radius of 1 mile/level":
                    fileWriter.Write(
                        " --set spellarea circle centered on you, with a radius of [[@{caster2_level}]] miles");
                    break;
                case "circle, centered on you, with a radius of 400 ft. + 40 ft./level":
                    fileWriter.Write(
                        " --set spellarea circle centered on you with a radius of [[400+(@{caster2_level}*40)]] ft.");
                    break;
                case "contiguous area consisting of one 10-foot cube/level (S)":
                    fileWriter.Write(
                        " --set spellarea contiguous area consisting of [[@{caster2_level}]] 10-ft. cubes");
                    break;
                case "contiguous area up to one 5-foot cube/caster level (S)":
                    fileWriter.Write(" --set spellarea contiguous area up to [[@{caster2_level}]] 5-ft. cubes");
                    break;
                case "cylinder (5-ft./level radius, 40 ft. high)":
                    fileWriter.Write(
                        " --set spellarea cylinder ([[@{caster2_level}*5]] ft. radius, 40 ft. high)");
                    break;
                case "cylinder of water (5-ft. radius/level, 30 ft. deep)":
                    fileWriter.Write(
                        " --set spellarea cylinder of water ([[@{caster2_level}*5]] ft. radius, 30 ft. deep)");
                    break;
                case "dust, earth, sand, or water in one 5-ft. square/level":
                    fileWriter.Write(
                        " --set spellarea dust, earth, sand, or water in [[@{caster2_level}]] 5-ft. squares");
                    break;
                case "four 5-ft. squares/level (see text)":
                    fileWriter.Write(" --set spellarea [[@{caster2_level}*4]] 5-ft. squares");
                    break;
                case "object touched or up to 5 sq. ft./level":
                    fileWriter.Write(" --set spellarea object touched or up to [[@{caster2_level}*5]] sq. ft.");
                    break;
                case "one 10-ft. cube per level":
                    fileWriter.Write(" --set spellarea [[@{caster2_level}]] 10-ft. cubes");
                    break;
                case "one 10-ft. cube/level":
                    fileWriter.Write(" --set spellarea [[@{caster2_level}]] 10-ft. cubes");
                    break;
                case "one 10-ft. cube/level (S)":
                    fileWriter.Write(" --set spellarea [[@{caster2_level}]] 10-ft. cubes (S)");
                    break;
                case "one 1-mile cube/level":
                    fileWriter.Write(" --set spellarea [[@{caster2_level}]] 1-mile cubes");
                    break;
                case "one 20-ft. cube/level":
                    fileWriter.Write(" --set spellarea [[@{caster2_level}]] 20-ft. cubes");
                    break;
                case "one 20-ft. cube/level (S)":
                    fileWriter.Write(" --set spellarea [[@{caster2_level}]] 20-ft. cubes (S)");
                    break;
                case "one 20-ft. square/level":
                    fileWriter.Write(" --set spellarea [[@{caster2_level}]] 20-ft. squares (S)");
                    break;
                case "one 30-ft. cube/level":
                    fileWriter.Write(" --set spellarea [[@{caster2_level}]] 30-ft. cubes");
                    break;
                case "one 30-ft. cube/level (S)":
                    fileWriter.Write(" --set spellarea [[@{caster2_level}]] 30-ft. cubes (S)");
                    break;
                case "one 40-ft. cube/level (S)":
                    fileWriter.Write(" --set spellarea [[@{caster2_level}]] 40-ft. cubes (S)");
                    break;
                case "one 5-ft. cube/level (S)":
                    fileWriter.Write(" --set spellarea [[@{caster2_level}]] 5-ft. cubes (S)");
                    break;
                case "one 5-ft. square/2 levels":
                    fileWriter.Write(" --set spellarea [[{1,floor((@{caster2_level}+0)/2)}kh1]] 5-ft. cubes");
                    break;
                case "one 5-ft. square/level (S); the area must be a stone surface":
                    fileWriter.Write(
                        " --set spellarea [[@{caster2_level}]] 5-ft. cubes (S); the area must be a stone surface");
                    break;
                case "one creature/level, no two of which can be more than 30 ft. apart":
                    fileWriter.Write(
                        " --set spellarea [[@{caster2_level}]] creatures, no two of which can be more than 30 ft. apart");
                    break;
                case "radius spread of up to 10 ft./level, 90 ft. high":
                    fileWriter.Write(
                        " --set spellarea radius spread of up to [[@{caster2_level}*10]] ft., 90 ft. high");
                    break;
                case "ten 10-foot cubes/level":
                    fileWriter.Write(" --set spellarea [[@{caster2_level}*10]] 10-ft. cubes");
                    break;
                case "two 10-ft. cubes per level (S)":
                    fileWriter.Write(" --set spellarea [[@{caster2_level}*2]] 10-ft. cubes (S)");
                    break;
                case "up to 10-ft.-radius/level emanation centered on you":
                    fileWriter.Write(
                        " --set spellarea up to [[@{caster2_level}*10]]-ft.-radius emanation centered on you");
                    break;
                case "up to 200 sq. ft./level":
                    fileWriter.Write(" --set spellarea up to [[@{caster2_level}*200]] sq. ft.");
                    break;
                case "up to one 10-ft. cube/level (S)":
                    fileWriter.Write(" --set spellarea up to [[@{caster2_level}]] 10-ft. cubes (S)");
                    break;
                case "up to one creature/level, no two of which may be more than 30 ft. apart":
                    fileWriter.Write(
                        " --set spellarea up to [[@{caster2_level}]] creatures, no two of which may be more than 30 ft. apart");
                    break;
                case "up to two 10-ft. cubes/level":
                    fileWriter.Write(" --set spellarea up to [[@{caster2_level}*2]]-ft. cubes");
                    break;
                case "water in a volume of 10 ft./level by 10 ft./level by 2 ft./level":
                    fileWriter.Write(
                        " --set spellarea water in a volume of [[@{caster2_level}*10]] ft. by [[@{caster2_level}*10]] ft. by [[@{caster2_level}*2]] ft.");
                    break;
                default:
                    fileWriter.Write($" --set spellarea {spellArea}");
                    break;
            }
        }

        private static void SetNpcSpellRange(StreamWriter fileWriter, string spellRange)
        {
            if (string.IsNullOrEmpty(spellRange)) return;
            switch (spellRange)
            {
                case "long (400 ft. + 40 ft./level)":
                    fileWriter.Write(" --set spellrange Long ([[400+(40*@{caster2_level})]])");
                    break;
                case "medium (100 ft. + 10 ft./level)":
                    fileWriter.Write(" --set spellrange Medium ([[100+(10*@{caster2_level})]])");
                    break;
                case "close (25 ft. + 5 ft./2 levels)":
                    fileWriter.Write(" --set spellrange Close ([[({1,floor((@{caster2_level}+0)/2)}kh1*5)+25]] ft.)");
                    break;
                default:
                    fileWriter.Write($" --set spellrange {spellRange}");
                    break;
            }
        }
    }
}
