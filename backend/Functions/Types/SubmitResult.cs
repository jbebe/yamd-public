using System.Collections.Generic;
using System.Text.Json.Serialization;
using YamdFunctions.Types;

namespace Functions.Types
{
    public class MediaEntityResult
    {
        // metadata
        [JsonPropertyName("createdDate")]
        public string CreatedDate => Original.CreatedDate.ToString("O");

        [JsonPropertyName("state")]
        public string State => Original.State.StringValue();

        // generated md5 hash of normalized url

        [JsonPropertyName("id")]
        public string Id => Original.Id;

        // given

        [JsonPropertyName("mediaUrl")]
        public string MediaUrl => Original.MediaUrl.OriginalString;

        // calculated on frontend

        [JsonPropertyName("type")]
        public string Type => Original.Type.StringValue();

        // coming from api response

        [JsonPropertyName("title")]
        public string? Title => Original.Title;

        [JsonPropertyName("imageB64")]
        public string? ImageB64 => Original.ImageB64;

        // coming from table query

        [JsonPropertyName("downloadFormats")]
        public List<DownloadFormat>? DownloadFormats => Original.DownloadFormats;

        [JsonIgnore]
        public MediaEntity Original { get; set; }

        public MediaEntityResult(MediaEntity entity)
        {
            Original = entity;
        }
    }

    public class SubmitResult
    {

        [JsonPropertyName("token")]
        public string? Token { get; set; }

        [JsonPropertyName("entity")]
        public MediaEntityResult? Entity { get; set; }

        public SubmitResult(string? token = null, MediaEntity? entity = null)
        {
            Token = token;
            Entity = entity != null ? new MediaEntityResult(entity) : null;
        }
    }
}
