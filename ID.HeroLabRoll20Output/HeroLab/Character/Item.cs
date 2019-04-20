using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using ID.HeroLabRoll20Output.HeroLab.Base;

namespace ID.HeroLabRoll20Output.HeroLab.Character
{
    public class Item : HeroLabNamedProperty
    {
        [XmlAttribute("quantity")]
        public string Quantity { get; set; }

        [XmlElement("weight")]
        public HeroLabTextValue Weight { get; set; }

        [XmlElement("cost")]
        public HeroLabTextValue Cost { get; set; }

        [XmlElement("description")]
        public string Description { get; set; }

        [XmlElement("itemslot")]
        public string ItemSlot { get; set; }
    }
}
