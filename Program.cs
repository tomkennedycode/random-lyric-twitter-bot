using HtmlAgilityPack;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Net;
using System.Text;
using System.IO;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace yungleanlyrics
{
    class Program
    {
        static void Main(string[] args)
        {
            string url = "https://www.azlyrics.com/lyrics/yunglean/sauron.html";
            var response = CallUrl(url).Result;
            string test = ParseHTML(response);
        }

        private static async Task<string> CallUrl(string fullUrl)
        {
            HttpClient client = new HttpClient();
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls13;
            client.DefaultRequestHeaders.Accept.Clear();
            var response = client.GetStringAsync(fullUrl);
            return await response;
        }

        private static string ParseHTML(string html){
            HtmlDocument htmlDoc = new HtmlDocument();
            htmlDoc.LoadHtml(html);
            List<string> wikiLink = new List<string>();

            var divs = htmlDoc.DocumentNode.SelectNodes("//div[@class='col-xs-12 col-lg-8 text-center']")
                .SelectMany(x => x.Descendants("div"));

            var title = htmlDoc.DocumentNode.SelectNodes("//div[@class='col-xs-12 col-lg-8 text-center']")
                .SelectMany(x => x.Descendants("b"));

            var titleName = title.Select(x => x.InnerText.Trim()).ToList();

            var contents = divs.Select(x => x.InnerText.Trim()).ToList();

            string songAndAlbum = string.Empty;
            if (titleName.ElementAtOrDefault(2) != null) {
                songAndAlbum = Regex.Replace($"{titleName[1]} - {titleName[2]}", "[^-0-9A-Za-z ]", "", RegexOptions.Compiled);
            } else {
                songAndAlbum = Regex.Replace($"{titleName[1]}", "[^-0-9A-Za-z ]", "", RegexOptions.Compiled);
            }

            var song = contents.OrderByDescending(s => s.Length).First();

            // song = song.Replace("\n", " ");
            string[] linesInSong = song.Split("\n");

            foreach(var line in linesInSong){
                Console.WriteLine($"{line}");
            }

            // get random line from list
            Random randy = new Random();
            int randomNumber = randy.Next(0, linesInSong.Length);
            string randomLine = linesInSong[randomNumber];

            Console.WriteLine($"RANDOM LINE HERE ----- {randomLine}");

            return $"{randomLine} - ({songAndAlbum})";

        }
    }
}
