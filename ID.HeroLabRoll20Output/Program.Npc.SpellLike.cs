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
        private static string WriteNpcSpellLikeAbility(HeroLabCharacter character, HeroLabSpecial spellLikeAbility)
        {
            var stringBuilder = new StringBuilder($"!HeroLabImporter --name {character.Name} --mode add --addtype spell-like");
            
            if (spellLikeAbility.Name.ToLower().Contains("(constant)"))
            {
                stringBuilder.Append(" --set timesperday constant");
            }
            else if (spellLikeAbility.Name.ToLower().Contains("(at will)"))
            {
                stringBuilder.Append(" --set timesperday at-will");
            }
            else if (spellLikeAbility.Name.ToLower().Contains("/day"))
            {
                stringBuilder.Append(" --set timesperday per-day");
            }
            else if (spellLikeAbility.Name.ToLower().Contains("/hour"))
            {
                stringBuilder.Append(" --set timesperday per-hour");
            }
            else if (spellLikeAbility.Name.ToLower().Contains("/week"))
            {
                stringBuilder.Append(" --set timesperday per-week");
            }
            else if (spellLikeAbility.Name.ToLower().Contains("/month"))
            {
                stringBuilder.Append(" --set timesperday per-month");
            }
            else if (spellLikeAbility.Name.ToLower().Contains("/year"))
            {
                stringBuilder.Append(" --set timesperday per-year");
            }
            var trackedResource = character.TrackedResources.Items.FirstOrDefault(tr => tr.Name == spellLikeAbility.Name);
            if (trackedResource != null)
            {
                stringBuilder.Append($" --set perday_max {trackedResource.Max}");
            }
            stringBuilder.Append($" --set spellname {spellLikeAbility.ShortName}");
            stringBuilder.Append($" --set spelldesc {spellLikeAbility.Description.Replace('\r', ' ').Replace('\n', ' ')}");
            var spell = SpellDatabase.Instance.FirstOrDefault(ss => ss.Name == spellLikeAbility.ShortName);
            WriteNpcSpellInfo(stringBuilder, character, spell, 2);
            return stringBuilder.ToString();
        }

        
    }
}
