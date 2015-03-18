namespace Troupe.Common.Interfaces {
    public interface ISessionClientDataStore {
        void SaveSessionClientData(ITroupeAuthenticationToken token);
        ITroupeAuthenticationToken GetClientSessionData();
        void DeleteSessionClientData();
    }
}