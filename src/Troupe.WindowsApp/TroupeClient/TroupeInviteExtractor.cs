using System.Collections.Generic;
using log4net;
using Troupe.Common.Interfaces;

namespace Troupe.WindowsApp.TroupeClient {
    public class TroupeInviteExtractor {
        private static readonly ILog logger = LogManager.GetLogger(typeof(TroupeInviteExtractor));
        private readonly TroupeUserExtractor _troupeUserExtractor = new TroupeUserExtractor();

        public IEnumerable<ITroupeInvite> ExtractInvites(IDictionary<string, object> dict)
        {
            if (dict.ContainsKey("snapshot")) {
                object obj = dict["snapshot"];
                if (typeof(object[]) == obj.GetType())
                {
                    logger.InfoFormat("found invite snapshot containing object[] of {0} objects", ((object[])obj).Length);
                    return ExtractInvitesFromList((object[])obj);
                }
            }

            return null;
        }

        private IEnumerable<ITroupeInvite> ExtractInvitesFromList(object[] dict)
        {
            List<ITroupeInvite> topics = new List<ITroupeInvite>();
            foreach (IDictionary<string, object> subdict in dict)
            {
                topics.Add(CreateTroupeInvite(subdict));
            }

            return topics;
        }

        public ITroupeInvite CreateTroupeInvite(IDictionary<string, object> subdict)
        {
            TroupeInvite troupeInvite = new TroupeInvite();
            foreach (string key in subdict.Keys) {
                if (null != subdict[key]) {
                    object value = subdict[key];
                    if (value.GetType() == typeof(string))
                        SetProperty(key, (string)value, troupeInvite);
                    else  if (value.GetType() == typeof(Dictionary<string,object>))
                        SetProperty(key, (Dictionary<string,object>)value, troupeInvite);
                    else
                        logger.WarnFormat("CreateTroupeInvite: Unexpected type: {0} ({1})", value.GetType(), key);
                }
                else
                    logger.InfoFormat("createTroupeInvite: key {0} is null", key);
            }

            logger.InfoFormat(@"createTroupeInvite: returns(Id={0}, OneToOneInvite={1}, FromUser={2}, User={3}, AcceptUrl={4}, Name={5}, V={6})",
                              troupeInvite.Id,
                              troupeInvite.OneToOneInvite,
                              troupeInvite.FromUser,
                              troupeInvite.User,
                              troupeInvite.AcceptUrl,
                              troupeInvite.Name,
                              troupeInvite.V);

            return troupeInvite;
        }

        public void SetProperty(string key, string value, ITroupeInvite troupeTopic)
        {
            if (key == null) return;
            switch (key.ToLower())
            {
                case "id": troupeTopic.Id = value; break;
                case "onetooneinvite": troupeTopic.OneToOneInvite = value; break;
                case "email": troupeTopic.Email = value; break;
                case "accepturl": troupeTopic.AcceptUrl = value; break;
                case "name": troupeTopic.Name = value; break;
                case "v": troupeTopic.V = value; break;

                default:
                    logger.InfoFormat("SetProperty: key value discarded {0}={1}", key, value);
                    break;
            }
        }

        public void SetProperty(string key, Dictionary<string,object> value, ITroupeInvite troupeTopic)
        {
            if (key == null) return;
            switch (key.ToLower()) {
                case "fromuser": troupeTopic.FromUser = _troupeUserExtractor.CreateTroupeUser(value); break;
                case "user": troupeTopic.User = _troupeUserExtractor.CreateTroupeUser(value); break;

                default:
                    logger.InfoFormat("SetProperty: key value discarded {0}={1}", key, value);
                    break;
            }
        }

    }
}