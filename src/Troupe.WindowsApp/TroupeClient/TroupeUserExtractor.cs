using System.Collections.Generic;
using log4net;
using Troupe.Common.Interfaces;

namespace Troupe.WindowsApp.TroupeClient {
    public class TroupeUserExtractor {
        private static readonly ILog logger = LogManager.GetLogger(typeof(TroupeUserExtractor));

        public ITroupeUser CreateTroupeUser(IDictionary<string, object> subdict)
        {
            TroupeUser troupeUser = new TroupeUser();
            foreach (string key in subdict.Keys) {
                if (null != subdict[key]) {
                    string value = subdict[key].ToString();
                    SetProperty(key, value, troupeUser);
                }
                else
                    logger.InfoFormat("createTroupeUser: key {0} is null", key);
            }

            logger.InfoFormat(@"createTroupeUser: returns(Id={0}, DisplayName={1}, Url={2}, AvatarUrlSmall={3}, AvatarUrlMedium={4}, V={5}",
                              troupeUser.Id,
                              troupeUser.DisplayName,
                              troupeUser.Url,
                              troupeUser.AvatarUrlSmall,
                              troupeUser.AvatarUrlMedium,
                              troupeUser.V);

            return troupeUser;
        }
        public void SetProperty(string key, string value, ITroupeUser troupeUser)
        {
            if (key == null) return;
            switch (key.ToLower())
            {
                case "id": troupeUser.Id = value; break;
                case "displayname": troupeUser.DisplayName = value; break;
                case "url": troupeUser.Url = value; break;
                case "avatarurlsmall": troupeUser.AvatarUrlSmall = value; break;
                case "avatarurlmedium": troupeUser.AvatarUrlMedium = value; break;
                case "v": troupeUser.V = value; break;

                default:
                    logger.InfoFormat("SetProperty: key value discarded {0}={1}", key, value);
                    break;
            }
        }

    }
}