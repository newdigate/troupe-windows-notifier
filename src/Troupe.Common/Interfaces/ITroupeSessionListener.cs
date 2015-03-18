namespace Troupe.Common.Interfaces {

    public interface ITroupeSessionListener {
        
        void LoginCompleted(bool successful);
        void LogoutCompleted();
        
        void Handshaking();
        void MessageReceived(ITroupeTopic topic, ITroupeMessage message);
        
        void TopicAdded(ITroupeTopic topic);
        void TopicUpdate(ITroupeTopic topic);
        void TopicRemoved(ITroupeTopic topic);
        
        void UnreadMessages(string troupeId, int numUnreadMessages);
        
        void InviteReceived(ITroupeMessage troupeMessage);
        void InviteAdded(ITroupeInvite troupeTopic);
        void InviteRemoved(ITroupeInvite troupeTopic);
    }
}