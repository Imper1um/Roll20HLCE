using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace ID.HeroLabRoll20Output.HeroLab.Base
{
    [Serializable]
    public class HeroLabProgram
    {
        [XmlElement("programinfo")]
        public string ProgramInfo { get; set; }
        [XmlElement("version")]
        public HeroLabVersion Version { get; set; }
    }
}
