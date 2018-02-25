using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using TweetSharp;
using SpotifyAPI.Web;
using SpotifyAPI.Web.Auth;
using System.Net.Http.Headers;
using System;
using System.Text;
using System.Net.Http;

namespace TweemSong_HackCU4
{
    public class ThemeSong
    {
        private string twitterHandle;

        public ThemeSong(string username)
        {
            twitterHandle = username;
        }

        public Track GenerateThemeSong()
        {
            var allTweets = string.Join(' ', GetAllTweets());
            var sentiments = GetSentimentAnalysis(allTweets);
            var themeSong = JsonConvert.DeserializeObject<RootObjectSpotify>(GetSongRecommendations(sentiments));
            return themeSong.tracks.First();
        }

        private IEnumerable<string> GetAllTweets()
        {
            var service = new TwitterService(Codes.TwitterKey, Codes.TwitterSecret);
            service.AuthenticateWith(Codes.TwitterAccessToken, Codes.TwitterAccessTokenSecret);

            var options = new ListTweetsOnUserTimelineOptions
            {
                Count = 50,
                ScreenName = twitterHandle
            };

            return service.ListTweetsOnUserTimeline(options).Select(x => x.Text);
        }

        private DocumentSentiment GetSentimentAnalysis(string text)
        {
            var uri = "https://language.googleapis.com/v1/documents:analyzeSentiment?key=" + Codes.GoogleCloudKey;
            string json = Post(uri, "{\"document\": {\"type\": \"PLAIN_TEXT\",\"content\": \"" + text + "\"},\"encodingType\": \"UTF8\"}");
            var sentimentObject = JsonConvert.DeserializeObject<RootObjectSentiment>(json);
            return sentimentObject.documentSentiment;
        }

        private string GetSongRecommendations(DocumentSentiment sentiment)
        {
            string uri1 = "https://api.spotify.com/v1/recommendations?limit=1&market=US&seed_genres=country%2Cdance%2Crock%2Cpop%2Cmetal&target_danceability=";
            string uri2 = "&target_energy=";
            string uri3 = "&target_instrumentalness=0.3&target_liveness=";
            string uri4 = "&min_popularity=50&target_popularity=75&target_speechiness=0.33&target_valence=";

            string uri = string.Format("{0}{1}{2}{3}{4}{5}{6}{7}", uri1, NumberConversionSentiments(sentiment.magnitude),
                uri2, NumberConversionSentiments(sentiment.magnitude),
                uri3, NumberConversionSentiments(sentiment.magnitude),
                uri4, NumberConversionSentiments(sentiment.score));

            WebHeaderCollection headers = new WebHeaderCollection
            {
                { "Authorization", "Authorization: Bearer " + GetSpotifyToken().Result }
            };
            return Get(uri, headers).Result;
        }

        private static async Task<string> GetSpotifyToken()
        {
            string credentials = String.Format("{0}:{1}", Codes.SpotifyClientID, Codes.SpotifyClientSecret);

            using (var client = new HttpClient())
            {
                //Define Headers
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(Encoding.UTF8.GetBytes(credentials)));

                //Prepare Request Body
                List<KeyValuePair<string, string>> requestData = new List<KeyValuePair<string, string>>();
                requestData.Add(new KeyValuePair<string, string>("grant_type", "client_credentials"));

                FormUrlEncodedContent requestBody = new FormUrlEncodedContent(requestData);

                //Request Token
                var request = await client.PostAsync("https://accounts.spotify.com/api/token", requestBody);
                var response = await request.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<AccessToken>(response).access_token;
            }
        }


        //Helper Methods

        //FROM https://stackoverflow.com/questions/27108264/c-sharp-how-to-properly-make-a-http-web-get-request
        private async Task<string> Get(string uri, WebHeaderCollection headers = null)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(uri);
            request.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;
            if (headers != null)
            {
                request.Headers = headers;
            }

            using (HttpWebResponse response = (HttpWebResponse)await request.GetResponseAsync())
            using (Stream stream = response.GetResponseStream())
            using (StreamReader reader = new StreamReader(stream))
            {
                return await reader.ReadToEndAsync();
            }
        }

        //FROM https://stackoverflow.com/questions/3735988/how-to-post-raw-data-using-c-sharp-httpwebrequest
        private string Post(string url, string json)
        {
            var httpWebRequest = (HttpWebRequest)WebRequest.Create(url);
            httpWebRequest.ContentType = "application/json";
            httpWebRequest.Method = "POST";

            using (var streamWriter = new StreamWriter(httpWebRequest.GetRequestStream()))
            {
                streamWriter.Write(json);
            }

            string rawJson = "";
            using (var response = (HttpWebResponse)httpWebRequest.GetResponse())
            {
                using (var reader = new StreamReader(response.GetResponseStream()))
                {
                    rawJson += reader.ReadToEnd();
                }
            }
            return rawJson;
        }

        private double NumberConversionSentiments(double num)
        {
            return Math.Min((num + 1) / 2, 1);
        }
    }

    //Sentiment Objects
    public class DocumentSentiment
    {
        public double magnitude { get; set; }
        public double score { get; set; }
    }

    public class RootObjectSentiment
    {
        public DocumentSentiment documentSentiment { get; set; }
    }

    //Spotify Objects
    class AccessToken
    {
        public string access_token { get; set; }
        public string token_type { get; set; }
        public long expires_in { get; set; }
    }

    public class ExternalUrls
    {
        public string spotify { get; set; }
    }

    public class Artist
    {
        public ExternalUrls external_urls { get; set; }
        public string href { get; set; }
        public string id { get; set; }
        public string name { get; set; }
        public string type { get; set; }
        public string uri { get; set; }
    }

    public class ExternalUrls2
    {
        public string spotify { get; set; }
    }

    public class Image
    {
        public int height { get; set; }
        public string url { get; set; }
        public int width { get; set; }
    }

    public class Album
    {
        public string album_type { get; set; }
        public List<Artist> artists { get; set; }
        public ExternalUrls2 external_urls { get; set; }
        public string href { get; set; }
        public string id { get; set; }
        public List<Image> images { get; set; }
        public string name { get; set; }
        public string type { get; set; }
        public string uri { get; set; }
    }

    public class ExternalUrls3
    {
        public string spotify { get; set; }
    }

    public class Artist2
    {
        public ExternalUrls3 external_urls { get; set; }
        public string href { get; set; }
        public string id { get; set; }
        public string name { get; set; }
        public string type { get; set; }
        public string uri { get; set; }
    }

    public class ExternalIds
    {
        public string isrc { get; set; }
    }

    public class ExternalUrls4
    {
        public string spotify { get; set; }
    }

    public class Track
    {
        public Album album { get; set; }
        public List<Artist2> artists { get; set; }
        public int disc_number { get; set; }
        public int duration_ms { get; set; }
        public bool @explicit { get; set; }
        public ExternalIds external_ids { get; set; }
        public ExternalUrls4 external_urls { get; set; }
        public string href { get; set; }
        public string id { get; set; }
        public bool is_playable { get; set; }
        public string name { get; set; }
        public int popularity { get; set; }
        public string preview_url { get; set; }
        public int track_number { get; set; }
        public string type { get; set; }
        public string uri { get; set; }
    }

    public class RootObjectSpotify
    {
        public List<Track> tracks { get; set; }
    }
}
