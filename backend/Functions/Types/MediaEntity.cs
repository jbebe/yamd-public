using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace YamdFunctions.Types
{
    public enum MediaState
    {
        Created,
        Prefilled,
        Success,
        Error,
    }

    public enum MediaType
    {
        YouTube,
    }

    public enum ResolutionType
    {
        _LQ,
        _MQ,
        _HD,
        _FullHD,
        _2K,
        _4K,
    }

    public class DownloadFormat
    {
        [JsonPropertyName("resolution")]
        public string Resolution { get; set; }

        [JsonPropertyName("downloadUrl")]
        public string DownloadUrl { get; set; }

        public DownloadFormat()
        {
            Resolution = string.Empty;
            DownloadUrl = string.Empty;
        }

        public DownloadFormat(ResolutionType resolution, Uri downloadUrl)
        {
            Resolution = resolution.StringValue();
            DownloadUrl = downloadUrl.OriginalString;
        }
    }

    public static class MediaExtensions
    {
        public static string StringValue(this MediaType type) => 
            type.ToString("G").ToLowerInvariant();


        public static string StringValue(this MediaState state) =>
            state.ToString("G").ToLowerInvariant();

        public static string StringValue(this ResolutionType type) =>
            type.ToString("G").Substring(1);
    }

    public class MediaTableEntity: TableEntity
    {
        public const string RowKeyValue = "ENTITY";

        public string CreatedDate { get; set; } = string.Empty;
        public string State { get; set; } = string.Empty;
        public string MediaUrl { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public string? Title { get; set; }
        public string? ImageB64 { get; set; }
        public string? DownloadUrl { get; set; }

        [JsonIgnore]
        public string Id => PartitionKey;

        public MediaTableEntity(){}

        public MediaTableEntity(MediaEntity entity)
        {
            PartitionKey = entity.Id;
            RowKey = RowKeyValue;

            CreatedDate = entity.CreatedDate.ToString("O");
            State = entity.State.StringValue();
            MediaUrl = entity.MediaUrl.AbsoluteUri;
            Type = entity.Type.StringValue();

            Title = entity.Title;
            ImageB64 = entity.ImageB64;
            DownloadUrl = JsonSerializer.Serialize(entity.DownloadFormats);
        }

        public MediaEntity ToMediaEntity() =>
            new MediaEntity
            {
                CreatedDate = DateTime.TryParse(CreatedDate, out var parsedCreatedDate)
                    ? parsedCreatedDate
                    : throw new Exception($"Invalid creation date {CreatedDate}"),
                State = Enum.TryParse<MediaState>(State, ignoreCase: true, out var parsedState)
                    ? parsedState
                    : throw new Exception($"Invalid media state {State}"),
                Id = !string.IsNullOrWhiteSpace(Id)
                    ? Id
                    : throw new Exception($"Invalid media ID {Id}"),
                MediaUrl = Uri.TryCreate(MediaUrl, UriKind.Absolute, out var parsedMediaUrl)
                    ? parsedMediaUrl
                    : throw new Exception($"Invalid media URL {MediaUrl}"),
                Type = Enum.TryParse<MediaType>(Type, ignoreCase: true, out var parsedType)
                    ? parsedType
                    : throw new Exception($"Invalid media type {Type}"),

                Title = Title,
                ImageB64 = ImageB64,
                DownloadFormats = JsonSerializer.Deserialize<List<DownloadFormat>>(DownloadUrl),
            };
    }

    public class MediaEntityRequest
    {
        [JsonPropertyName("createdDate")]
        public string? CreatedDate { get; set; }

        [JsonPropertyName("state")]
        public string? State { get; set; }

        [JsonPropertyName("id")]
        public string? Id { get; set; }

        [JsonPropertyName("mediaUrl")]
        public string? MediaUrl { get; set; }

        [JsonPropertyName("type")]
        public string? Type { get; set; }

        [JsonPropertyName("title")]
        public string? Title { get; set; }

        [JsonPropertyName("imageB64")]
        public string? ImageB64 { get; set; }

        [JsonPropertyName("downloadFormats")]
        public string? DownloadFormats { get; set; }


        // Returns whether or not the object coming from the client is valid or not
        [JsonIgnore]
        public bool IsValid =>
            CreatedDate != null &&
            State != null &&
            Id != null &&
            MediaUrl != null &&
            Type != null;

        public MediaEntity ToMediaEntity() =>
            new MediaEntity
            {
                CreatedDate = DateTime.TryParse(CreatedDate, out var parsedCreatedDate) 
                    ? parsedCreatedDate 
                    : throw new Exception($"Invalid creation date {CreatedDate}"),
                State = Enum.TryParse<MediaState>(State, ignoreCase: true, out var parsedState) 
                    ? parsedState 
                    : throw new Exception($"Invalid media state {State}"),
                Id = !string.IsNullOrWhiteSpace(Id) 
                    ? Id 
                    : throw new Exception($"Invalid media ID {Id}"),
                MediaUrl = Uri.TryCreate(MediaUrl, UriKind.Absolute, out var parsedMediaUrl) 
                    ? parsedMediaUrl 
                    : throw new Exception($"Invalid media URL {MediaUrl}"),
                Type = Enum.TryParse<MediaType>(Type, ignoreCase: true, out var parsedType) 
                    ? parsedType 
                    : throw new Exception($"Invalid media type {Type}"),

                Title = Title,
                ImageB64 = ImageB64,
                DownloadFormats = DownloadFormats == null 
                    ? new List<DownloadFormat>()
                    : JsonSerializer.Deserialize<List<DownloadFormat>>(DownloadFormats),
            };
    }

    public class MediaEntity
    {
        // metadata
        public DateTime CreatedDate { get; set; }

        [JsonPropertyName("state")]
        public MediaState State { get; set; }

        // generated md5 hash of normalized url

        [JsonPropertyName("id")]
        public string Id { get; set; } = string.Empty;

        // given

        [JsonPropertyName("mediaUrl")]
        public Uri MediaUrl { get; set; } = new Uri("http://localhost/");

        // calculated on frontend

        [JsonPropertyName("type")]
        public MediaType Type { get; set; }

        // coming from api response

        [JsonPropertyName("title")]
        public string? Title { get; set; }

        [JsonPropertyName("imageB64")]
        public string? ImageB64 { get; set; }

        // coming from table query

        [JsonPropertyName("downloadUrl")]
        public List<DownloadFormat>? DownloadFormats { get; set; }
    }

}
