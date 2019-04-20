using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace ID.HeroLabRoll20Output.HeroLab.Character
{
    [Serializable]
    public class PathfinderClasses
    {
        [XmlAttribute("level")]
        public string Level { get; set; }
        [XmlAttribute("summary")]
        public string Summary { get; set; }
        [XmlAttribute("summaryabbr")]
        public string SummaryAbbreviation { get; set; }
        [XmlElement("class", typeof(PathfinderClass))]
        public PathfinderClass[] Classes { get; set; }
    }
}
