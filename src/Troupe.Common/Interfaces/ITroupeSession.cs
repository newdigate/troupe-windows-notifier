namespace Troupe.Common.Interfaces {
    public interface ITroupeSession {
        void AddSessionListener(ITroupeSessionListener listener);

        void BeginSession();
        void EndSession(bool logout);
    }
}