﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using ID.HeroLabRoll20Output.HeroLab.Base;

namespace ID.HeroLabRoll20Output.HeroLab.Character
{
    public class RaceType : HeroLabNamedProperty
    {
        [XmlAttribute("active")]
        public string Active { get; set; }
    }
}
