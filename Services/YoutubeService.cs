using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using yungleanlyrics.Interfaces;
using Google.Apis.Services;
using Google.Apis.YouTube.v3;
using Google.Apis.YouTube.v3.Data;
using System.Linq;

namespace yungleanlyrics.Services
{
    public class YoutubeService : IYoutubeService
    {
        private readonly ILogger<YoutubeService> _log;
        private readonly IConfiguration _config;

        public YoutubeService(ILogger<YoutubeService> logger, IConfiguration config)
        {
            _log = logger;
            _config = config;
        }
        public string GetSongURL(string song) 
        {    
            var youtubeService = new YouTubeService(new BaseClientService.Initializer()
            {
                ApiKey = _config.GetValue<string>("YoutubeAPIKey"),
                ApplicationName = this.GetType().ToString()
            });

            var searchListRequest = youtubeService.Search.List("snippet");
            searchListRequest.Q = song;
            searchListRequest.MaxResults = 5;

            // Call the search.list method to retrieve results matching the specified query term.
            var searchListResponse = searchListRequest.Execute();

            // Get top rate video - with our search query, i am confident that it'll find the correct video
            SearchResult youtubeVideo = searchListResponse.Items.FirstOrDefault();

            return $"https://www.youtube.com/watch?v={youtubeVideo.Id.VideoId}";
        }
    }
}