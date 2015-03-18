using Troupe.Common.Interfaces;

namespace Troupe.WindowsApp.TroupeClient.Messages {
    
    public class TroupeMessageImpl : ITroupeMessage {
        public string Message { get; set; }
        public string Topic { get; set; }
    }
}