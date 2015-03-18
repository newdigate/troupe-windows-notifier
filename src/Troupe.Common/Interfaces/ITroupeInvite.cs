namespace Troupe.Common.Interfaces {
    public interface ITroupeInvite {
        string Id { get; set; }
        string OneToOneInvite { get; set; }
        ITroupeUser FromUser { get; set; }
        ITroupeUser User { get; set; }
        string Email { get; set; }
        string AcceptUrl { get; set; }
        string Name { get; set; }
        string V { get; set; }
    }
}