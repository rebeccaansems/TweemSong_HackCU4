using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using TweetSharp;

namespace TweemSong_HackCU4
{
    public class ThemeSong
    {
        private string twitterHandle;

        public ThemeSong(string username)
        {
            twitterHandle = username;
        }

        public async Task<string> GenerateThemeSong()
        {
            var allTweets = GetAllTweets();

            return SentimentAnalysis(allTweets.First()).magnitude.ToString();
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

        private DocumentSentiment SentimentAnalysis(string text)
        {
            var uri = "https://language.googleapis.com/v1/documents:analyzeSentiment?key=" + Codes.GoogleCloudKey;
            string json = Post(uri, "{\"document\": {\"type\": \"PLAIN_TEXT\",\"content\": \"" + text + "\"},\"encodingType\": \"UTF8\"}");
            var obj = JsonConvert.DeserializeObject<RootObject>(json);
            return obj.documentSentiment;

        }


        //Helper Methods

        //FROM https://stackoverflow.com/questions/27108264/c-sharp-how-to-properly-make-a-http-web-get-request
        public async Task<string> Get(string uri)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(uri);
            request.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;

            using (HttpWebResponse response = (HttpWebResponse)await request.GetResponseAsync())
            using (Stream stream = response.GetResponseStream())
            using (StreamReader reader = new StreamReader(stream))
            {
                return await reader.ReadToEndAsync();
            }
        }

        //FROM https://stackoverflow.com/questions/3735988/how-to-post-raw-data-using-c-sharp-httpwebrequest
        public static string Post(string url, string json)
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
    }

    //Sentiment Objects
    public class DocumentSentiment
    {
        public double magnitude { get; set; }
        public double score { get; set; }
    }

    public class RootObject
    {
        public DocumentSentiment documentSentiment { get; set; }
    }
}
