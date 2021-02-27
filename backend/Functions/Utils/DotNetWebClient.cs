using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Threading.Tasks;

namespace Functions.Utils
{
    public interface IWebClient: IDisposable
    {
        void SetHeaders(IDictionary<string, string> headers);

        Task<Stream> GetGzipResponseBodyStreamAsync(Uri url);

        byte[] GetResponseBodyBytes(Uri url);
    }

    public class DotNetWebClient : IWebClient
    {
        private WebClient HttpClient { get; set; }

        public DotNetWebClient()
        {
            HttpClient = new WebClient();
        }

        public void Dispose() => HttpClient.Dispose();

        public void SetHeaders(IDictionary<string, string> headers)
        {
            foreach (var (k, v) in headers)
                HttpClient.Headers[k] = v;
        }

        public async Task<Stream> GetGzipResponseBodyStreamAsync(Uri url)
        {
            var gzippedResponseStream = await HttpClient.OpenReadTaskAsync(url);
            var responseStream = new GZipStream(gzippedResponseStream, CompressionMode.Decompress);

            return responseStream;
        }

        public byte[] GetResponseBodyBytes(Uri url) =>
            HttpClient.DownloadData(url);
    }
}
