using Functions.Utils;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace FunctionsTests.Mock
{
    class MockWebClient : IWebClient
    {
        public Uri Url { get; }

        public string WebsiteContent { get; }

        public MockWebClient(Uri url, string websiteContent)
        {
            Url = url;
            WebsiteContent = websiteContent;
        }

        public void Dispose()
        {
            // Not needed
        }

        public byte[] GetResponseBodyBytes(Uri url)
        {
            return Encoding.UTF8.GetBytes(WebsiteContent);
        }

        public Task<Stream> GetGzipResponseBodyStreamAsync(Uri url)
        {
            var stream = new MemoryStream();
            var writer = new StreamWriter(stream);
            
            writer.Write(WebsiteContent);
            writer.Flush();
            stream.Position = 0;
            
            return Task.FromResult<Stream>(stream);
        }

        public void SetHeaders(IDictionary<string, string> headers)
        {
            // Not needed
        }
    }
}
