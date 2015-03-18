using System;
using Troupe.Common.Interfaces;

namespace Troupe.WindowsApp.TroupeClient {
    public class TroupeTopic : ITroupeTopic
    {
        public string Favourite { get; set; }
        public string Id { get; set; }
        public string LastAccessTime { get; set; }
        public string Name { get; set; }
        public string OneToOne { get; set; }
        public string UnreadItems { get; set; }
        public string User { get; set; }
        public string Uri { get; set; }
        public string Url { get; set; }
        public string Version { get; set; }
        public string V { get; set; }
    }
}