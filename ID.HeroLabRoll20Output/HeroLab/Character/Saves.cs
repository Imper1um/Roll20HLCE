﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace ID.HeroLabRoll20Output.HeroLab.Character
{
    [Serializable]
    public class Saves
    {
        [XmlElement("save", typeof(Save))]
        public Save[] Items { get; set; }

        [XmlElement("allsaves")]
        public Save AllSaves { get; set; }
    }
}
