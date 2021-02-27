using AngleSharp;
using AngleSharp.Js;
using Jint.Native.Array;
using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using YamdFunctions.Types;
using System.Text.Json.Serialization;
using System.Collections.Generic;
using AngleSharp.Dom;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Formats.Jpeg;
using System.Linq;
using Functions.Utils;
using System.Web;

namespace YamdFunctions.Logic
{
    public class YouTubeMediaManager : MediaManagerBase
    {
        class YouTubeMediaObject
        {
            [JsonPropertyName("qualityLabel")]
            public string? QualityLabel { get; set; }

            [JsonPropertyName("url")]
            public Uri? Url { get; set; }

            [JsonPropertyName("signatureCipher")]
            public string? SignatureCipher { get; set; }
        }

        public YouTubeMediaManager(Func<IWebClient> webClientGetter) : base(MediaType.YouTube, webClientGetter) {}

        public override ResolutionType GetResolution(string text)
        {
            return text switch
            {
                "240p" => ResolutionType._LQ,
                "360p" => ResolutionType._MQ,
                "480p" => ResolutionType._MQ,
                "720p" => ResolutionType._HD,
                "1080p" => ResolutionType._FullHD,
                _ => ResolutionType._4K,
            };
        }

        public async override Task<MediaEntity> ProcessAsync(MediaEntity entity)
        {
            var rawHtmlContent = await GetHtmlAsync(entity);

            var htmlContent = await GetDomAsync(rawHtmlContent);
            var mediaList = GetMediaList(htmlContent);
            
            entity.DownloadFormats = mediaList.Select(x => new DownloadFormat(GetResolution(x.QualityLabel), GetUrl(x))).ToList();
            //entity.Title ??= GetTitle(htmlContent);
            //entity.ImageB64 ??= await GetThumbnailAsync(htmlContent, 32);
            entity.State = MediaState.Success;

            return entity;
        }

        private Uri GetUrl(YouTubeMediaObject obj)
        {
            if (obj.Url != null)
                return obj.Url;

            // https://github.com/l1ving/youtube-dl/blob/c438ea87e4db650f5c44526b155ea335ea49d006/youtube_dl/extractor/youtube.py#L1956

            List<(string Name, string Value)> _parse_qsl(string qs, bool keep_blank_values = false, bool strict_parsing = false, string encoding = "utf-8", string errors = "replace")
            {
                var pairs = (from s1 in qs.Split("&")
                             from s2 in s1.Split(";")
                             select s2).ToList();
                var r = new List<(string Name, string value)>();
                foreach (var name_value in pairs)
                {
                    if (name_value == null && !strict_parsing)
                    {
                        continue;
                    }
                    var nv = name_value.Split("=", 2).ToList();
                    if (nv.Count != 2)
                    {
                        if (strict_parsing)
                        {
                            throw new Exception(string.Format("bad query field: %r", name_value));
                        }
                        // Handle case of a control-name with no equal sign
                        if (keep_blank_values)
                        {
                            nv.Add("");
                        }
                        else
                        {
                            continue;
                        }
                    }
                    if (nv[1].Length > 0 || keep_blank_values)
                    {
                        var name = nv[0].Replace("+", " ");
                        name = HttpUtility.UrlDecode(name);
                        var value = nv[1].Replace("+", " ");
                        value = HttpUtility.UrlDecode(value);
                        r.Add((name, value));
                    }
                }
                return r;
            }

            Dictionary<string, List<string>> compat_parse_qs(string qs, bool keep_blank_values = false, bool strict_parsing = false, string encoding = "utf-8", string errors = "replace")
            {
                var parsed_result = new Dictionary<string, List<string>>();
                var pairs = _parse_qsl(qs, keep_blank_values, strict_parsing, encoding: encoding, errors: errors);
                foreach (var (name, value) in pairs)
                {
                    if (parsed_result.ContainsKey(name)) parsed_result[name].Add(value);
                    else parsed_result[name] = new List<string> { value };
                }
                
                return parsed_result;
            }

            var deciphered = compat_parse_qs(obj.SignatureCipher);
            
            return new Uri(deciphered["url"][0]);
        }

        private string? GetTitle(IDocument document)
        {
            return document.QuerySelector(@"head > meta[property=""og:title""]").GetAttribute("content");
        }

        private async Task<string?> GetThumbnailAsync(IDocument document, int size)
        {
            try
            {
                // Locate and download thumbnail
                var thumbnailRawUrl = document.QuerySelector(@"head > meta[property=""og:image""]").GetAttribute("content");
                var thumbnailUrl = new Uri(thumbnailRawUrl, UriKind.Absolute);
                byte[]? imageBytes;
                using (var httpClient = CreateWebClient())
                    imageBytes = httpClient.GetResponseBodyBytes(thumbnailUrl);
                var stream = new MemoryStream(imageBytes);

                // Resize to very small jpeg
                using var image = await Image.LoadAsync(stream);
                var resizeOpts = new ResizeOptions
                {
                    Mode = ResizeMode.Crop,
                    Size = new Size(size),
                };
                image.Mutate(x => x.Resize(resizeOpts));
                var outStream = new MemoryStream();
                await image.SaveAsJpegAsync(outStream, new JpegEncoder { Quality = 15 });

                // Convert to base64
                string base64 = Convert.ToBase64String(outStream.ToArray());
                return $"data:image/jpg;base64,{base64}";
            }
            catch (Exception)
            {
                // Pull down logging from Framework to this class and use it
                return null;
            }
        }

        private async Task<IDocument> GetDomAsync(string rawHtmlContent)
        {
            var config = AngleSharp.Configuration.Default.WithJs();
            var context = BrowsingContext.New(config);
            return await context.OpenAsync(req => req.Content(rawHtmlContent));
        }

        private List<YouTubeMediaObject> GetMediaList(IDocument document)
        {
            var jsCommand = "JSON.parse(ytplayer.config.args.player_response).streamingData.formats";
            if (!(document.ExecuteScript(jsCommand) is ArrayInstance mediaFormatsJs))
                throw new Exception("Result array must be an array of objects");

            return JintHelpers.SerializeArray<YouTubeMediaObject>(mediaFormatsJs);
        }

        private async Task<string> GetHtmlAsync(MediaEntity entity)
        {
            var mediaId = entity.MediaUrl.ParseQueryString()["v"];
            using var httpClient = CreateWebClient();
            var headers = new Dictionary<string, string>
            {
                // Default Chrome download
                ["Upgrade-Insecure-Requests"] = "1",
                ["User-Agent"] = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/85.0.4183.121 Safari/537.36",
                ["Accept"] = "text/html,application/xhtml+xml,application/xml;q=0.9,image/avif,image/webp,image/apng,*/*;q=0.8,application/signed-exchange;v=b3;q=0.9",
                ["Sec-Fetch-Site"] = "same-origin",
                ["Sec-Fetch-Mode"] = "navigate",
                ["Sec-Fetch-User"] = "?1",
                ["Sec-Fetch-Dest"] = "document",
                ["Accept-Encoding"] = "gzip, deflate, br",
                ["Accept-Language"] = "en-US,en;q=0.9",

                // Mimic referrer from google.com
                ["Referer"] = $"https://www.google.com/search?q={mediaId}",
            };
            httpClient.SetHeaders(headers);

            // Download the HTML
            var responseStream = await httpClient.GetGzipResponseBodyStreamAsync(entity.MediaUrl);
            var responseStr = new StreamReader(responseStream).ReadToEnd();

            return responseStr;
        }
    }
}
