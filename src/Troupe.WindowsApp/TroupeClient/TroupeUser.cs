using Troupe.Common.Interfaces;

namespace Troupe.WindowsApp.TroupeClient {
    public class TroupeUser : ITroupeUser {
        public string Id { get; set; }
        public string DisplayName { get; set; }
        public string Url { get; set; }
        public string AvatarUrlSmall { get; set; }
        public string AvatarUrlMedium { get; set; }
        public string V { get; set; }
    }
}