using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace ID.HeroLabRoll20Output.HeroLab.Character
{
    [Serializable]
    public class Flaws
    {
        [XmlElement("flaw", typeof(Feat))]
        public List<Feat> Items { get; set; }
    }
}
