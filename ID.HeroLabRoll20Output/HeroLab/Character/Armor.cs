using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using ID.HeroLabRoll20Output.HeroLab.Base;

namespace ID.HeroLabRoll20Output.HeroLab.Character
{
    public class Armor : Item
    {
        [XmlAttribute("ac")]
        public string ArmorClass { get; set; }

        [XmlAttribute("equipped")]
        public string Equipped { get; set; }

        [XmlAttribute("natural")]
        public string Natural { get; set; }

        [XmlAttribute("useradded")]
        public string UserAdded { get; set; }

        
    }
}
