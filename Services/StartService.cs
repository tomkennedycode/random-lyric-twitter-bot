using Microsoft.Extensions.Configuration;
using yungleanlyrics.Interfaces;
using Microsoft.Extensions.Logging;

namespace yungleanlyrics.Services
{
    public class StartService : IStartService
    {
        private readonly ILogger<StartService> _log;
        private readonly IConfiguration _config;
        private readonly ILyricScraperService _lyricScaper;
        private readonly ITweetService _tweetService;
        public StartService (ILogger<StartService> log, IConfiguration config, ILyricScraperService lyricScraper, ITweetService tweetService) {
            _log = log;
            _config = config;
            _lyricScaper = lyricScraper;
            _tweetService = tweetService;
        }
        public void Run ()
        {
            // Get list of songs
            string url = "https://www.azlyrics.com/y/yunglean.html";
            var listOfSongsHtml = _lyricScaper.CallUrl(url).Result;
            var songUrl = _lyricScaper.GetSongLink(listOfSongsHtml);

            //Scrap the urls
            var response = _lyricScaper.CallUrl(songUrl).Result;
            string lyric = _lyricScaper.ParseHTML(response);
            _log.LogInformation("got the line here boss - {test}", lyric);

            //Tweet the line!
            var resp = _tweetService.Tweet(lyric);
        }
    }
}
