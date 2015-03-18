namespace Troupe.Common.Interfaces {
    public interface ITroupeUser {
        string Id { get; set; }
        string DisplayName { get; set; }
        string Url { get; set; }
        string AvatarUrlSmall { get; set; }
        string AvatarUrlMedium { get; set; }
        string V { get; set; }
    }
}