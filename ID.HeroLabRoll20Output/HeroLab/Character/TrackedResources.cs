using System;
using System.Xml.Serialization;

namespace ID.HeroLabRoll20Output.HeroLab.Character
{
    [Serializable]
    public class TrackedResources
    {
        [XmlElement("trackedresource", typeof(TrackedResource))]
        public TrackedResource[] Items { get; set; }
    }
}