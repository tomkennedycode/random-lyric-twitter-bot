using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Text;
using System.Security.Cryptography;
using Microsoft.Extensions.Configuration;
using yungleanlyrics.Interfaces;
using Microsoft.Extensions.Logging;

namespace yungleanlyrics.Services
{
    public class TweetService : ITweetService
    {
        const string TwitterApiBaseUrl = "https://api.twitter.com/1.1/";
        private readonly ILogger<TweetService> _log;
        private readonly IConfiguration _config;
	    readonly HMACSHA1 hash;
        readonly DateTime dateTime = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        public TweetService (ILogger<TweetService> log, IConfiguration config) {
            _log = log;
            _config = config;

            hash = new HMACSHA1(new ASCIIEncoding()
                .GetBytes(string.Format("{0}&{1}", _config.GetValue<string>("CustomerKeySecret"), _config.GetValue<string>("AccessTokenSecret"))));
        }

        public Task<string> Tweet(string text)
        {
            var request = new Dictionary<string, string> {
                { "status", text },
                { "trim_user", "1" }
            };

            return SendRequest("statuses/update.json", request);
        }

        Task<string> SendRequest(string url, Dictionary<string, string> data)
        {
            var fullUrl = TwitterApiBaseUrl + url;

            // Timestamps are in seconds since 1/1/1970.
            var timestamp = (int)((DateTime.UtcNow - dateTime).TotalSeconds);

            // Add all the OAuth headers we'll need to use when constructing the hash.
            data.Add("oauth_consumer_key", _config.GetValue<string>("CustomerKey"));
            data.Add("oauth_signature_method", "HMAC-SHA1");
            data.Add("oauth_timestamp", timestamp.ToString());
            data.Add("oauth_nonce", "a");
            data.Add("oauth_token", _config.GetValue<string>("AccessToken"));
            data.Add("oauth_version", "1.0");

            // Generate the OAuth signature and add it to our payload.
            data.Add("oauth_signature", GenerateSignature(fullUrl, data));

            // Build the OAuth HTTP Header from the data.
            string oAuthHeader = GenerateOAuthHeader(data);

            // Build the form data (exclude OAuth stuff that's already in the header).
            var formData = new FormUrlEncodedContent(data.Where(kvp => !kvp.Key.StartsWith("oauth_")));

            return SendRequest(fullUrl, oAuthHeader, formData);
        }

        string GenerateSignature(string url, Dictionary<string, string> data)
        {
            var signatureString = string.Join(
                "&",
                data
                    .Union(data)
                    .Select(kvp => string.Format("{0}={1}", Uri.EscapeDataString(kvp.Key), Uri.EscapeDataString(kvp.Value)))
                    .OrderBy(s => s)
            );

            var fullSignatureData = string.Format(
                "{0}&{1}&{2}",
                "POST",
                Uri.EscapeDataString(url),
                Uri.EscapeDataString(signatureString.ToString())
            );

            return Convert.ToBase64String(hash.ComputeHash(new ASCIIEncoding().GetBytes(fullSignatureData.ToString())));
        }

        string GenerateOAuthHeader(Dictionary<string, string> data)
        {
            return "OAuth " + string.Join(
                ", ",
                data
                    .Where(kvp => kvp.Key.StartsWith("oauth_"))
                    .Select(kvp => string.Format("{0}=\"{1}\"", Uri.EscapeDataString(kvp.Key), Uri.EscapeDataString(kvp.Value)))
                    .OrderBy(s => s)
            );
        }

        async Task<string> SendRequest(string fullUrl, string oAuthHeader, FormUrlEncodedContent formData)
        {
            using (var http = new HttpClient())
            {
                http.DefaultRequestHeaders.Add("Authorization", oAuthHeader);

                var httpResponse = http.PostAsync(fullUrl, formData).Result;
                var responseBody = await httpResponse.Content.ReadAsStringAsync();

                return responseBody;
            }
        }

    }
}
