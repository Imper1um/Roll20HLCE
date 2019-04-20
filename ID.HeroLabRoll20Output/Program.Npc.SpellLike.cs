using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ID.HeroLabRoll20Output.HeroLab.Base;
using ID.HeroLabRoll20Output.HeroLab.Character;
using ID.HeroLabRoll20Output.Spells;

namespace ID.HeroLabRoll20Output
{
    public partial class Program
    {
        private static void WriteNpcSpellLikeAbility(StreamWriter fileWriter, HeroLabCharacter character, HeroLabSpecial spellLikeAbility)
        {
            fileWriter.Write($"!HeroLabImporter --name {character.Name} --mode add --addtype spell-like");
            
            if (spellLikeAbility.Name.ToLower().Contains("(constant)"))
            {
                fileWriter.Write(" --set timesperday constant");
            }
            else if (spellLikeAbility.Name.ToLower().Contains("(at will)"))
            {
                fileWriter.Write(" --set timesperday at-will");
            }
            else if (spellLikeAbility.Name.ToLower().Contains("/day"))
            {
                fileWriter.Write(" --set timesperday per-day");
            }
            else if (spellLikeAbility.Name.ToLower().Contains("/hour"))
            {
                fileWriter.Write(" --set timesperday per-hour");
            }
            else if (spellLikeAbility.Name.ToLower().Contains("/week"))
            {
                fileWriter.Write(" --set timesperday per-week");
            }
            else if (spellLikeAbility.Name.ToLower().Contains("/month"))
            {
                fileWriter.Write(" --set timesperday per-month");
            }
            else if (spellLikeAbility.Name.ToLower().Contains("/year"))
            {
                fileWriter.Write(" --set timesperday per-year");
            }
            var trackedResource = character.TrackedResources.Items.FirstOrDefault(tr => tr.Name == spellLikeAbility.Name);
            if (trackedResource != null)
            {
                fileWriter.Write($" --set perday_max {trackedResource.Max}");
            }
            fileWriter.Write($" --set spellname {spellLikeAbility.ShortName}");
            fileWriter.Write($" --set spelldesc {spellLikeAbility.Description.Replace('\r', ' ').Replace('\n', ' ')}");
            var spell = SpellDatabase.Instance.FirstOrDefault(ss => ss.Name == spellLikeAbility.ShortName);
            WriteNpcSpellInfo(fileWriter, character, spell);
        }

        
    }
}
