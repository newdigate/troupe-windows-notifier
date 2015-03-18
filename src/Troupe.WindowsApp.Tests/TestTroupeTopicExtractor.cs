using System.Collections.Generic;
using System.Web.Script.Serialization;
using log4net;
using NUnit.Framework;
using Troupe.Common.Interfaces;
using Troupe.WindowsApp.TroupeClient;

namespace Troupe.WindowsApp.Tests
{
    public class TestTroupeTopicExtractor {
        private readonly ILog logger = LogManager.GetLogger(typeof (TestTroupeTopicExtractor));
        private JavaScriptSerializer _jsonSerializer = new JavaScriptSerializer();
        private TroupeTopicExtractor _troupeTopicExtractor;

        [TestFixtureSetUp]
        public void FixtureSetUp() {
            _troupeTopicExtractor = new TroupeTopicExtractor();
        }
        
        [Test]
        public void TestTheMooSubcriptionTopics() {
            string snapshotJson = "{\"snapshot\":[{\"favourite\":true,\"id\":\"5121ff495da2431022000032\",\"lastAccessTime\":\"2013-08-19T12:50:26.317Z\",\"name\":\"Tim Lind\",\"oneToOne\":true,\"unreadItems\":0,\"uri\":null,\"url\":\"/tim\",\"user\":{\"avatarUrlMedium\":\"//dnsjy61awi9j1.cloudfront.net/avatar/m/50f3da67c8de62e7610003a1/4.jpg\",\"avatarUrlSmall\":\"//dnsjy61awi9j1.cloudfront.net/avatar/s/50f3da67c8de62e7610003a1/4.jpg\",\"displayName\":\"Tim Lind\",\"id\":\"50f3da67c8de62e7610003a1\",\"location\":{\"description\":\"Cape Town, Western Cape\",\"timestamp\":\"2013-08-16T08:21:47.000Z\"},\"url\":\"/tim\",\"username\":\"tim\",\"v\":353}},{\"favourite\":true,\"id\":\"51404c386b3d55f75e000007\",\"lastAccessTime\":\"2013-08-19T17:52:37.184Z\",\"name\":\"Mike Bartlett\",\"oneToOne\":true,\"unreadItems\":0,\"uri\":null,\"url\":\"/mike\",\"user\":{\"avatarUrlMedium\":\"//dnsjy61awi9j1.cloudfront.net/avatar/m/4fd48c8040a7cc9644000022/9.jpg\",\"avatarUrlSmall\":\"//dnsjy61awi9j1.cloudfront.net/avatar/s/4fd48c8040a7cc9644000022/9.jpg\",\"displayName\":\"Mike Bartlett\",\"id\":\"4fd48c8040a7cc9644000022\",\"location\":{\"description\":\"Euston, England\",\"timestamp\":\"2013-08-20T08:12:28.000Z\"},\"url\":\"/mike\",\"username\":\"mike\",\"v\":699},\"v\":2},{\"id\":\"51dace0995e7666866000c17\",\"lastAccessTime\":\"2013-08-13T14:13:34.295Z\",\"name\":\"Mauro\",\"oneToOne\":true,\"unreadItems\":0,\"uri\":null,\"url\":\"/malditogeek\",\"user\":{\"avatarUrlMedium\":\"https://www.gravatar.com/avatar/4605adbcd13e20c14e82fcf528b516e6?d=identicon\",\"avatarUrlSmall\":\"https://www.gravatar.com/avatar/4605adbcd13e20c14e82fcf528b516e6?d=identicon\",\"displayName\":\"Mauro\",\"id\":\"51d146b595e7666866000644\",\"location\":{\"description\":\"Christchurch, England\",\"timestamp\":\"2013-08-10T20:15:36.000Z\"},\"url\":\"/malditogeek\",\"username\":\"malditogeek\",\"v\":257},\"v\":2},{\"id\":\"51dc38e028d05bd17b000052\",\"lastAccessTime\":\"2013-08-09T12:39:16.574Z\",\"name\":\"Andy Trevorah\",\"oneToOne\":true,\"unreadItems\":0,\"uri\":null,\"url\":\"/trevorah\",\"user\":{\"avatarUrlMedium\":\"https://www.gravatar.com/avatar/9496bed6a195a98d671fbaee17374fae?d=identicon\",\"avatarUrlSmall\":\"https://www.gravatar.com/avatar/9496bed6a195a98d671fbaee17374fae?d=identicon\",\"displayName\":\"Andy Trevorah\",\"id\":\"51dc0e97ad151df14200004d\",\"url\":\"/trevorah\",\"username\":\"trevorah\",\"v\":5},\"v\":1},{\"id\":\"520cea74fc7696f44f00001e\",\"lastAccessTime\":\"2013-08-15T14:49:25.990Z\",\"name\":\"Andrew\",\"oneToOne\":true,\"unreadItems\":0,\"url\":\"/bongo\",\"user\":{\"avatarUrlMedium\":\"https://www.gravatar.com/avatar/50b886ce4c03e92a8bb9af0c146f2d02?d=identicon\",\"avatarUrlSmall\":\"https://www.gravatar.com/avatar/50b886ce4c03e92a8bb9af0c146f2d02?d=identicon\",\"displayName\":\"Andrew\",\"id\":\"520cea24fc7696f44f00001b\",\"url\":\"/bongo\",\"username\":\"bongo\",\"v\":3},\"v\":1},{\"id\":\"51b8dc65c50035be4d0001e4\",\"lastAccessTime\":\"2013-07-29T22:24:12.465Z\",\"name\":\"Moo Moo Man\",\"oneToOne\":true,\"unreadItems\":0,\"uri\":null,\"url\":\"/one-one/4fd6127d7e32c3e64700004d\",\"user\":{\"avatarUrlMedium\":\"https://www.gravatar.com/avatar/a5748d56b04d1545f104c48c3de77120?d=identicon\",\"avatarUrlSmall\":\"https://www.gravatar.com/avatar/a5748d56b04d1545f104c48c3de77120?d=identicon\",\"displayName\":\"Moo Moo Man\",\"id\":\"4fd6127d7e32c3e64700004d\",\"url\":\"/one-one/4fd6127d7e32c3e64700004d\"},\"v\":2},{\"id\":\"51f6ed279e78901e0c000039\",\"lastAccessTime\":\"2013-08-07T15:08:59.815Z\",\"name\":\"horse\",\"oneToOne\":true,\"unreadItems\":0,\"url\":\"/horse\",\"user\":{\"avatarUrlMedium\":\"https://www.gravatar.com/avatar/0cef0f8cc2b0f9ec96445b3d7e8a8627?d=identicon\",\"avatarUrlSmall\":\"https://www.gravatar.com/avatar/0cef0f8cc2b0f9ec96445b3d7e8a8627?d=identicon\",\"displayName\":\"horse\",\"id\":\"51f6ec489e78901e0c000031\",\"url\":\"/horse\",\"username\":\"horse\",\"v\":3},\"v\":1},{\"id\":\"52051d707cdbecb954000007\",\"lastAccessTime\":\"2013-08-19T10:30:12.393Z\",\"name\":\"Bongo\",\"oneToOne\":false,\"unreadItems\":0,\"uri\":\"fz7ul2\",\"url\":\"/fz7ul2\",\"v\":2},{\"id\":\"50a3697af9b49364210071d3\",\"lastAccessTime\":\"2013-08-09T12:39:15.327Z\",\"name\":\"Crystalspace\",\"oneToOne\":false,\"unreadItems\":0,\"uri\":\"ku5saq\",\"url\":\"/ku5saq\"},{\"id\":\"50a615eaf9b49364210073dd\",\"lastAccessTime\":\"2013-08-09T11:39:43.270Z\",\"name\":\"Demo Troupe\",\"oneToOne\":false,\"unreadItems\":0,\"uri\":\"xx9xr9\",\"url\":\"/xx9xr9\"},{\"id\":\"505074b145f272102d000007\",\"lastAccessTime\":\"2013-08-08T18:12:02.399Z\",\"name\":\"Feets by Day Campaign Team\",\"oneToOne\":false,\"unreadItems\":0,\"uri\":\"ikh2ba\",\"url\":\"/ikh2ba\",\"v\":1},{\"id\":\"4fd48bda40a7cc9644000004\",\"lastAccessTime\":\"2013-08-13T11:51:13.759Z\",\"name\":\"Moooo Mooo\",\"oneToOne\":false,\"unreadItems\":0,\"uri\":\"txkw2b\",\"url\":\"/txkw2b\",\"v\":22},{\"favourite\":true,\"id\":\"4fd5d8d27f9f889134000316\",\"lastAccessTime\":\"2013-08-20T10:01:36.573Z\",\"name\":\"Souper Troupers\",\"oneToOne\":false,\"unreadItems\":0,\"uri\":\"p7xgor\",\"url\":\"/p7xgor\",\"v\":11},{\"id\":\"5202734f209c32b329000093\",\"lastAccessTime\":\"2013-08-13T16:16:32.818Z\",\"name\":\"Tim + Founders\",\"oneToOne\":false,\"unreadItems\":0,\"uri\":\"bmzny9\",\"url\":\"/bmzny9\",\"v\":4},{\"id\":\"50f18cfcc8de62e76100039a\",\"lastAccessTime\":\"2013-07-29T22:24:11.993Z\",\"name\":\"Troupe Founders\",\"oneToOne\":false,\"unreadItems\":0,\"uri\":\"0nxqqe\",\"url\":\"/0nxqqe\"},{\"id\":\"507d41bb22f659b2640005bd\",\"lastAccessTime\":\"2013-08-07T13:49:12.691Z\",\"name\":\"Troupe Funding\",\"oneToOne\":false,\"unreadItems\":0,\"uri\":\"yueqt1\",\"url\":\"/yueqt1\"}]}";
            Dictionary<string, object> snapshot = _jsonSerializer.Deserialize<Dictionary<string, object>>(snapshotJson);
            foreach (ITroupeTopic topic in _troupeTopicExtractor.ExtractTopics(snapshot)) {
                logger.InfoFormat("Name: {0}", topic.Name);
                logger.InfoFormat("Favourite: {0}", topic.Favourite);
                logger.InfoFormat("Id: {0}", topic.Id);
                logger.InfoFormat("LastAccessTime: {0}", topic.LastAccessTime);
                logger.InfoFormat("OneToOne: {0}", topic.OneToOne);
                logger.InfoFormat("UnreadItems: {0}", topic.UnreadItems);
                logger.InfoFormat("Uri: {0}", topic.Uri);
                logger.InfoFormat("Url: {0}", topic.Url);
                logger.InfoFormat("Version: {0}", topic.Version);
            }

        }
    }
}
