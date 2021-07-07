using Microsoft.Extensions.Configuration;
using yungleanlyrics.Interfaces;
using Microsoft.Extensions.Logging;
using System;
using yungleanlyrics.Models;
using Newtonsoft.Json.Linq;

namespace yungleanlyrics.Services
{
    public class StartService : IStartService
    {
        private readonly ILogger<StartService> _log;
        private readonly IConfiguration _config;
        private readonly ILyricScraperService _lyricScaper;
        private readonly ITweetService _tweetService;
        private readonly IYoutubeService _youtubeService;
        public StartService (ILogger<StartService> log,
                            IConfiguration config,
                            ILyricScraperService lyricScraper,
                            ITweetService tweetService,
                            IYoutubeService youtubeService) {
            _log = log;
            _config = config;
            _lyricScaper = lyricScraper;
            _tweetService = tweetService;
            _youtubeService = youtubeService;
        }
        public void Run ()
        {
            try {
                _log.LogInformation("checking if bot is allowed to tweet...");

                if (_tweetService.ShouldTweet()) {
                    bool hasTweeted = false;
                    // We cannot keep scraping website otherwise it will ban us, so make sure we only attempt 3 times before closing down.
                    int maximumAttempts = 3;
                    while(!hasTweeted && maximumAttempts > 0) {
                        
                        _log.LogInformation("bot has been given permission to tweet");

                        // Get list of songs
                        var listOfSongsHtml = _lyricScaper.CallUrl(_config.GetValue<string>("LyricsUrl")).Result;
                        var songUrl = _lyricScaper.GetSongLink(listOfSongsHtml);
                        _log.LogInformation("scraper has chosen this song - {songUrl}", songUrl);

                        //Scrap the urls
                        var scraperResponse = _lyricScaper.CallUrl(songUrl).Result;
                        Song song = _lyricScaper.ParseHTML(scraperResponse);
                        _log.LogInformation("the line is ready to be tweeted - {lyric}", song.SelectedLyrics);

                        //Tweet the line if we haven't detected any false lyrics
                        if (song.SelectedLyrics != string.Empty) {
                            var tweetResponse = _tweetService.Tweet(song.SelectedLyrics);
                            _log.LogInformation("tweet has been sent");

                            // Get youtube url of tweeted song
                            string youtubeUrl = _youtubeService.GetSongURL($"{song.ArtistName} {song.SongName}");
                            _log.LogInformation("youtube url has been retrieved - ", youtubeUrl);

                            // Get ID of lyric tweet so we can reply with youtube url
                            var parsedStuff = JObject.Parse(tweetResponse.Result);
                            var tweetID = parsedStuff["id_str"].ToString();

                            // Reply to original lyric tweet with url
                            var tweetReply = _tweetService.TweetReply(tweetID, youtubeUrl);
                            _log.LogInformation("reply tweet has been sent");
                            hasTweeted = true;
                        } else {
                            maximumAttempts -= 1;
                        }
                    }
                } else {
                    _log.LogInformation("the bot has already tweeted today.");
                }
                

            } catch (Exception exception) {
                _log.LogInformation("bot ran into problem, message - {errorMessage}", exception.Message);
            } finally {
                _log.LogInformation("ending startservice");
            }
        }
    }
}
