using Troupe.Common.Interfaces;

namespace Troupe.WindowsApp.TroupeClient {
    public class TroupeInvite : ITroupeInvite {
        public string Id { get; set; }
        public string OneToOneInvite { get; set; }
        public ITroupeUser FromUser { get; set; }
        public ITroupeUser User { get; set; }
        public string Email { get; set; }
        public string AcceptUrl { get; set; }
        public string Name { get; set; }
        public string V { get; set; }
    }
}