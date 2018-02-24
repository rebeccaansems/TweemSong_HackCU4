using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
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

        public string GenerateThemeSong()
        {
            return GetTweets();
        }

        private string GetTweets()
        {
            var service = new TwitterService(Codes.TwitterKey, Codes.TwitterSecret);
            service.AuthenticateWith(Codes.TwitterAccessToken, Codes.TwitterAccessTokenSecret);

            var options = new ListTweetsOnUserTimelineOptions
            {
                Count = 30,
                ScreenName = twitterHandle
            };

            return service.ListTweetsOnUserTimeline(options).First().Text;
        }



        //Helper Methods

        //FROM https://stackoverflow.com/questions/27108264/c-sharp-how-to-properly-make-a-http-web-get-request
        public async Task<string> GetAsync(string uri)
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

        //FROM https://stackoverflow.com/questions/27108264/c-sharp-how-to-properly-make-a-http-web-get-request
        public async Task<string> PostAsync(string uri, string data, string contentType, string method = "POST")
        {
            byte[] dataBytes = Encoding.UTF8.GetBytes(data);

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(uri);
            request.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;
            request.ContentLength = dataBytes.Length;
            request.ContentType = contentType;

            using (Stream requestBody = request.GetRequestStream())
            {
                await requestBody.WriteAsync(dataBytes, 0, dataBytes.Length);
            }

            using (HttpWebResponse response = (HttpWebResponse)await request.GetResponseAsync())
            using (Stream stream = response.GetResponseStream())
            using (StreamReader reader = new StreamReader(stream))
            {
                return await reader.ReadToEndAsync();
            }
        }
    }
}
