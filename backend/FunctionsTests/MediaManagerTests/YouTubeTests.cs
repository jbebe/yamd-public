using FunctionsTests.Mock;
using NUnit.Framework;
using System;
using System.Threading.Tasks;
using YamdFunctions.Logic;
using YamdFunctions.Types;

namespace FunctionsTests
{
    partial class MediaManagerTestBase
    {
        public (Uri Url, string Content) YouTubeVideo_Old =
            (new Uri("https://www.youtube.com/watch?v=yU5KXTDCf-0", UriKind.Absolute),
            GetWebsiteContent("YouTube/yU5KXTDCf-0.html"));

        public (Uri Url, string Content) YouTubeVideo_New =
            (new Uri("https://www.youtube.com/watch?v=Fbp2XcFy6vI", UriKind.Absolute),
            GetWebsiteContent("YouTube/Fbp2XcFy6vI.html"));

        public async Task YouTubeTest_BaseAsync(Uri url, string content)
        {
            var manager = new YouTubeMediaManager(() => new MockWebClient(url, content));

            var entity = await manager.ProcessAsync(new MediaEntity
            {
                MediaUrl = url
            });

            Assert.AreEqual(MediaState.Success, entity.State);
        }

        [Test]
        [Category(Constants.Category.MediaManager)]
        public async Task YouTubeTest_OldVideo()
        {
            await YouTubeTest_BaseAsync(YouTubeVideo_Old.Url, YouTubeVideo_Old.Content);
        }

        [Test]
        [Category(Constants.Category.MediaManager)]
        public async Task YouTubeTest_NewVideo()
        {
            await YouTubeTest_BaseAsync(YouTubeVideo_New.Url, YouTubeVideo_New.Content);
        }
    }
}
