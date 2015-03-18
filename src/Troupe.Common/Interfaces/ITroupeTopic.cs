namespace Troupe.Common.Interfaces {
    public interface ITroupeTopic {
        string Favourite { get; set; }
        string Id { get; set; }
        string LastAccessTime { get; set; }
        string Name { get; set; }
        string OneToOne { get; set; }
        string UnreadItems { get; set; }
        string User { get; set; }
        string Uri { get; set; }
        string Url { get; set; }
        string Version { get; set; }
        string V { get; set; }
    }
}