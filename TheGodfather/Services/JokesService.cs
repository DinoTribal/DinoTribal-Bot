﻿#region USING_DIRECTIVES
using System;
using System.IO;
using System.Net;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
#endregion

namespace TheGodfather.Services
{
    public static class JokesService
    {
        public static async Task<string> GetRandomJokeAsync()
        {
            var res = await GetStringResponseAsync("https://icanhazdadjoke.com/")
                .ConfigureAwait(false);
            return res;
        }

        public static async Task<IReadOnlyList<string>> SearchForJokesAsync(string query)
        {
            var res = await GetStringResponseAsync("https://icanhazdadjoke.com/search?term=" + query.Replace(' ', '+'))
                .ConfigureAwait(false);
            if (string.IsNullOrWhiteSpace(res))
                return null;
            return res.Split('\n');
        }

        public static async Task<string> GetYoMommaJokeAsync()
        {
            string data = null;
            using (WebClient wc = new WebClient()) {
                data = await wc.DownloadStringTaskAsync("http://api.yomomma.info/")
                    .ConfigureAwait(false);
            }

            try {
                return JObject.Parse(data)["joke"].ToString();
            } catch (JsonException) {
                throw new WebException();
            }
        }

        private static async Task<string> GetStringResponseAsync(string url)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            request.AutomaticDecompression = DecompressionMethods.GZip;
            request.Accept = "text/plain";

            string data = null;
            using (var response = await request.GetResponseAsync().ConfigureAwait(false))
            using (var stream = response.GetResponseStream())
            using (var reader = new StreamReader(stream)) {
                data = await reader.ReadToEndAsync()
                    .ConfigureAwait(false);
            }

            return data;
        }
    }
}
