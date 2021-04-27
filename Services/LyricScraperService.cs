using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using HtmlAgilityPack;
using Microsoft.Extensions.Configuration;
using yungleanlyrics.Interfaces;
using Microsoft.Extensions.Logging;

namespace yungleanlyrics.Services
{
    public class LyricScraperService : ILyricScraperService
    {
        private readonly ILogger<LyricScraperService> _log;
        private readonly IConfiguration _config;
        public LyricScraperService (ILogger<LyricScraperService> log, IConfiguration config) {
            _log = log;
            _config = config;
        }

        public async Task<string> CallUrl(string fullUrl) {
            HttpClient client = new HttpClient();
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls13;
            client.DefaultRequestHeaders.Accept.Clear();
            var response = client.GetStringAsync(fullUrl);
            return await response;
        }

        public string ParseHTML(string html) {
            HtmlDocument htmlDoc = new HtmlDocument();
            htmlDoc.LoadHtml(html);
            List<string> wikiLink = new List<string>();

            var divs = htmlDoc.DocumentNode.SelectNodes("//div[@class='col-xs-12 col-lg-8 text-center']")
                .SelectMany(x => x.Descendants("div"));

            var title = htmlDoc.DocumentNode.SelectNodes("//div[@class='col-xs-12 col-lg-8 text-center']")
                .SelectMany(x => x.Descendants("b"));

            var titleName = title.Select(x => x.InnerText.Trim()).ToList ();

            var contents = divs.Select(x => x.InnerText.Trim()).ToList ();

            string songAndAlbum = string.Empty;
            if (titleName.ElementAtOrDefault(2) != null) {
                songAndAlbum = Regex.Replace($"{titleName[1]} - {titleName[2]}", "[^-0-9A-Za-z ]", "", RegexOptions.Compiled);
            } else {
                songAndAlbum = Regex.Replace($"{titleName[1]}", "[^-0-9A-Za-z ]", "", RegexOptions.Compiled);
            }

            var song = contents.OrderByDescending(s => s.Length).First ();

            string[] linesInSong = song.Split("\n");

            // get random line from list
            Random randy = new Random();
            int randomNumber = randy.Next(0, linesInSong.Length);
            string randomLine = linesInSong[randomNumber];

            return $"{randomLine} [{songAndAlbum}]";

        }
    }
}
