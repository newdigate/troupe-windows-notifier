namespace Troupe.WindowsApp.TroupeClient.Messages {
    public class TroupeNotificationMessage : TroupeMessageImpl {
        private readonly string title;
        private readonly string message;
        private readonly string link;

        public TroupeNotificationMessage(string title, string message, string link) {
            this.title = title;
            this.link = link;
            this.message = message;
        }

        public string Message {
            get { return message; }
        }

        public string Title {
            get { return title; }
        }

        public string Link
        {
            get { return link; }
        }

    }
}