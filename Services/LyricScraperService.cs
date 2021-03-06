using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using HtmlAgilityPack;
using Microsoft.Extensions.Configuration;
using yungleanlyrics.Interfaces;
using Microsoft.Extensions.Logging;
using yungleanlyrics.Models;

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

        public Song ParseHTML(string html) {
            HtmlDocument htmlDoc = new HtmlDocument();
            htmlDoc.LoadHtml(html);

            var songDiv = htmlDoc.DocumentNode.SelectNodes("//div[@class='col-xs-12 col-lg-8 text-center']")
                .SelectMany(x => x.Descendants("div"));
            var title = htmlDoc.DocumentNode.SelectNodes("//div[@class='col-xs-12 col-lg-8 text-center']")
                .SelectMany(x => x.Descendants("b"));

            var titleName = title.Select(x => x.InnerText.Trim()).ToList ();
            var contents = songDiv.Select(x => x.InnerText.Trim()).ToList ();

            Song songDetails = new Song();
            songDetails.ArtistName = _config.GetValue<string>("ArtistName");

            string songAndAlbum = string.Empty;
            songDetails.SongName = titleName[1];

            if (titleName.ElementAtOrDefault(2) != null) {
                songDetails.AlbumName = titleName[2];
                songAndAlbum = Regex.Replace($"{songDetails.SongName} - {songDetails.AlbumName}", "[^-0-9A-Za-z ]", "", RegexOptions.Compiled);
            } else {
                songAndAlbum = Regex.Replace($"{songDetails.SongName}", "[^-0-9A-Za-z ]", "", RegexOptions.Compiled);
            }

            var song = contents.OrderByDescending(s => s.Length).First();
            string[] linesInSong = song.Split("\n");
            songDetails.SelectedLyrics = GetRandomValueFromString(linesInSong);

            // Sometimes it scrapes lines in the page that are just [Hook: ] or [Artist Name: ]
            if ((songDetails.SelectedLyrics.StartsWith("[") && songDetails.SelectedLyrics.EndsWith("]")) || songDetails.SelectedLyrics == string.Empty) {
                songDetails.SelectedLyrics = string.Empty;
            } else {
                songDetails.SelectedLyrics = $"{songDetails.SelectedLyrics} [{songAndAlbum}]"; 
            }

            return songDetails;
        }

        public string GetSongLink(string html)
        {
            HtmlDocument htmlDoc = new HtmlDocument();
            htmlDoc.LoadHtml(html);

            var divs = htmlDoc.DocumentNode.SelectNodes("//div[@id='listAlbum']");
            var hrefs = divs.Descendants("a")
                .Select(node => node.GetAttributeValue("href", ""))
                .ToList();

            var fullLinkList = hrefs.Select(trimLink => trimLink.Substring(2)).Select(songLink => $"https://www.azlyrics.com{songLink}").ToArray();

            
            return GetRandomValueFromString(fullLinkList);
        }

        private string GetRandomValueFromString(string[] listOfStrings)
        {
            Random random = new Random();
            int randomNumber = random.Next(0, listOfStrings.Length);
            return listOfStrings[randomNumber];
        }
    }
}
