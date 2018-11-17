namespace Our.Umbraco.AzureCDNToolkit.Tests
{
    using System.Collections;
    using System.Collections.Generic;
    using System.Web.Mvc;
    using NUnit.Framework;

    [TestFixture]
    public class UrlHelperTests
    {
        [Test]
        public void TestAbsoluteUrl()
        {
            AzureCdnToolkit.Instance.Refresh();

            var url = "https://i.ytimg.com/vi/mW3S0u8bj58/maxresdefault.jpg";
            var expected = "https://i.ytimg.com/vi/mW3S0u8bj58/maxresdefault.jpg";

            var resolvedUrl = new UrlHelper().ResolveCdn(url).ToString();
            Assert.AreEqual(expected, resolvedUrl);
        }

        [Test]
        public void TestAbsoluteUrlWithQuerystring()
        {
            AzureCdnToolkit.Instance.Refresh();

            var url = "https://i.ytimg.com/vi/mW3S0u8bj58/maxresdefault.jpg?v=12345";
            var expected = "https://i.ytimg.com/vi/mW3S0u8bj58/maxresdefault.jpg?v=12345";

            var resolvedUrl = new UrlHelper().ResolveCdn(url).ToString();
            Assert.AreEqual(expected, resolvedUrl);
        }

        [Test]
        public void TestRelativeMediaUrlNaked()
        {
            AzureCdnToolkit.Instance.Refresh();

            var url = "/media/1051/church.jpg";
            var expected = "https://azurecdntoolkitdemo.blob.core.windows.net/media/1051/church.jpg?v=0.0.1";

            var resolvedUrl = new UrlHelper().ResolveCdn(url, asset: false).ToString();
            Assert.AreEqual(expected, resolvedUrl);
        }

        [Test]
        public void TestRelativeMediaUrlWithCacheBuster()
        {
            AzureCdnToolkit.Instance.Refresh();

            var url = "/media/1051/church.jpg?rnd=12122121112";
            var expected = "https://azurecdntoolkitdemo.blob.core.windows.net/media/1051/church.jpg?rnd=12122121112";

            var resolvedUrl = new UrlHelper().ResolveCdn(url, asset: false, cacheBuster: "12122121112").ToString();
            Assert.AreEqual(expected, resolvedUrl);
        }

        [Test]
        public void TestRelativeMediaUrlNakedCustomContainer()
        {
            AzureCdnToolkit.Instance.Refresh();
            AzureCdnToolkit.Instance.MediaContainer = "myspecialcontainer";
            var url = "/media/1051/church.jpg";
            var expected = "https://azurecdntoolkitdemo.blob.core.windows.net/myspecialcontainer/1051/church.jpg?v=0.0.1";

            var resolvedUrl = new UrlHelper().ResolveCdn(url, asset: false).ToString();
            Assert.AreEqual(expected, resolvedUrl);
        }

        [Test]
        public void TestRelativeMediaUrlWithQuerystring()
        {
            AzureCdnToolkit.Instance.Refresh();

            var url = "/media/1051/church.jpg?width=100";
            var expected = "/media/1051/church.jpg?width=100&v=0.0.1";

            var resolvedUrl = new UrlHelper().ResolveCdn(url, asset: false, htmlEncode: false).ToString();
            Assert.AreEqual(expected, resolvedUrl);
        }

        [Test]
        public void TestRelativeAssetUrlNaked()
        {
            AzureCdnToolkit.Instance.Refresh();

            var url = "/css/mycss.css";
            var expected = "https://azurecdntoolkitdemo.blob.core.windows.net/assets/css/mycss.css?v=0.0.1";

            var resolvedUrl = new UrlHelper().ResolveCdn(url).ToString();
            Assert.AreEqual(expected, resolvedUrl);
        }

        [Test]
        public void TestSasToken()
        {
            // You need to start AzureStorageEmulator for this test!

            AzureCdnToolkit.Instance.Refresh();
            var url = "http://127.0.0.1:10000/media/test.jpg";
            var expectedParams = 6;

            var cdnUrl = Helpers.AzureStorageHelper.Instance.GetPathWithSasTokenQuery(url);
            var qs = cdnUrl.Split('?')[1].Split('&');

            Assert.IsTrue(cdnUrl.StartsWith($"{url}?"));
            Assert.AreEqual(expectedParams, qs.Length);
            Assert.IsTrue(ContainsStarting(qs, "sv="));
            Assert.IsTrue(ContainsStarting(qs, "sig="));
            Assert.IsTrue(ContainsStarting(qs, "st="));
            Assert.IsTrue(ContainsStarting(qs, "se="));
            Assert.IsTrue(ContainsStarting(qs, "sr="));
            Assert.IsTrue(ContainsStarting(qs, "sp="));

            Assert.IsFalse(ContainsStarting(qs, "xxx="));

        }
        private bool ContainsStarting(string[] collection, string startingWith) {
            bool rVal = false;
            foreach (var item in collection)
            {
                if (item.StartsWith(startingWith))
                {
                    rVal = true;
                    break;
                }
            }
            return rVal;
        }
    }
}
