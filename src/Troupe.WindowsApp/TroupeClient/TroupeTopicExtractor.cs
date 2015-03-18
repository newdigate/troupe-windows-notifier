using System.Collections;
using System.Collections.Generic;
using System.Web.Script.Serialization;
using log4net;
using Troupe.Common.Interfaces;

namespace Troupe.WindowsApp.TroupeClient {
    public class TroupeTopicExtractor {
        private static readonly ILog logger = LogManager.GetLogger(typeof(TroupeTopicExtractor));
        private readonly JavaScriptSerializer _jsonSerializer = new JavaScriptSerializer();

        private IEnumerable<ITroupeTopic> ExtractTopicsFromList(object[] dict)
        {
            List<ITroupeTopic> topics = new List<ITroupeTopic>();
            foreach (IDictionary<string, object> subdict in dict) {
                topics.Add(CreateTroupeTopic(subdict));
            }

            return topics;
        }

        private IEnumerable<ITroupeTopic> ExtractTopicsFromArrayList(ArrayList dict)
        {
            List<ITroupeTopic> topics = new List<ITroupeTopic>();
            foreach (IDictionary<string, object> subdict in dict) {
                topics.Add( CreateTroupeTopic(subdict));
            }

            return topics;
        }

        public TroupeTopic CreateTroupeTopic(IDictionary<string, object> subdict) {
            TroupeTopic troupeTopic = new TroupeTopic();
            foreach (string key in subdict.Keys) {
                if (null != subdict[key])
                {
                    string value = subdict[key].ToString();
                    SetProperty(key, value, troupeTopic);
                }
                else
                    logger.InfoFormat("createTroupeTopic: key {0} is null", key);

            }

            logger.InfoFormat(@"createTroupeTopic: returns(Favourite={0}, Id={1}, LastAccessTime={2}, Name={3}, OneToOne={4}, UnreadItems={5}, User={6}, Uri={7}, Url={8}, Version={9}, V={10})",
                troupeTopic.Favourite,
                troupeTopic.Id,
                troupeTopic.LastAccessTime,
                troupeTopic.Name,
                troupeTopic.OneToOne,
                troupeTopic.UnreadItems,
                troupeTopic.User,
                troupeTopic.Uri,
                troupeTopic.Url,
                troupeTopic.Version,
                troupeTopic.V);
        
            return troupeTopic;
        }

        public IEnumerable<ITroupeTopic> ExtractTopics(IDictionary<string, object> dict) {
            if (dict.ContainsKey("snapshot")) {
               
                object obj = dict["snapshot"];
                if (typeof(ArrayList) == obj.GetType()) {
                    logger.InfoFormat("found troupe topic snapshot containing arraylist of {0} objects", ((ArrayList)obj).Count);
                    return ExtractTopicsFromArrayList((ArrayList) obj);
                }
                if (typeof(object[]) == obj.GetType()) {
                    logger.InfoFormat("found troupe topic snapshot containing object[] of {0} objects", ((object[])obj).Length);
                    return ExtractTopicsFromList((object[]) obj);
                }
            }
           
            return null;
        }

        public void SetProperty(string key, string value, ITroupeTopic troupeTopic) {
            if (key == null) return;
            switch (key.ToLower()) {
                case "id": troupeTopic.Id = value; break;
                case "favourite": troupeTopic.Favourite = value; break;
                case "lastaccesstime": troupeTopic.LastAccessTime = value; break;
                case "name": troupeTopic.Name = value; break;
                case "onetoone": troupeTopic.OneToOne = value; break;
                case "unreaditems": troupeTopic.UnreadItems = value; break;
                case "user" : troupeTopic.User = value; break;
                case "uri": troupeTopic.Uri = value; break;
                case "url": troupeTopic.Url = value; break;
                case "version": troupeTopic.Version = value; break;
                case "v": troupeTopic.V = value; break;

                default:
                    logger.InfoFormat("SetProperty: key value discarded {0}={1}", key, value);
                    break;  
            }
        }
    }
}