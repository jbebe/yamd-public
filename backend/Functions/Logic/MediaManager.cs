using Functions.Utils;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using YamdFunctions.Types;

namespace YamdFunctions
{
    public class MediaManagerFactory
    {
        private static Dictionary<MediaType, IMediaManager> MediaManagers { get; set; } = new Dictionary<MediaType, IMediaManager>();

        public static void AddManager<T>(T instance) where T : IMediaManager 
        {
            MediaManagers.Add(instance.Type, instance);
        }

        public static IMediaManager GetManager(MediaType type) => MediaManagers[type];
    }

    public interface IMediaManager
    {
        public MediaType Type { get; }

        /// <summary>
        /// 
        /// </summary>
        /// <remarks>
        /// During "process", the following properties must be filled:
        ///   * Title (of the media)
        ///   * ImageB64 (small picture for the video)
        ///   * DownloadUrl (the actual URL to download the media)
        /// </remarks>
        /// <param name="entity"></param>
        /// <returns></returns>
        Task<MediaEntity> ProcessAsync(MediaEntity entity);

        /// <summary>
        /// Returns the resolution enum for a given media format
        /// </summary>
        /// <remarks>
        /// This needs to be implemented by all managers as the notation may vary
        /// </remarks>
        /// <param name="text">The raw text that holds resolution information</param>
        ResolutionType GetResolution(string text);
    }

    public abstract class MediaManagerBase : IMediaManager
    {
        public MediaType Type { get; }

        private Func<IWebClient> WebClientCreator { get; set; }

        public IWebClient CreateWebClient() => WebClientCreator();

        public MediaManagerBase(MediaType type, Func<IWebClient> webClientCreator)
        {
            Type = type;
            WebClientCreator = webClientCreator;
        }

        public abstract Task<MediaEntity> ProcessAsync(MediaEntity entity);

        public abstract ResolutionType GetResolution(string text);
    }
}