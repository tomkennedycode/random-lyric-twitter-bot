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
        public StartService (ILogger<StartService> log, IConfiguration config, ILyricScraperService lyricScraper) {
            _log = log;
            _config = config;
            _lyricScaper = lyricScraper;
        }
        public void Run ()
        {
            string url = "https://www.azlyrics.com/lyrics/yunglean/sauron.html";

            //Scrap the urls
            var response = _lyricScaper.CallUrl(url).Result;
            string test = _lyricScaper.ParseHTML(response);
            _log.LogInformation("got the line here boss - {test}", test);
            //Tweet the line!

        }
    }
}
