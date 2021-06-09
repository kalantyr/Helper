using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;
using Newtonsoft.Json;

[assembly:InternalsVisibleTo("Helper.Tests")]

namespace Helper.Checkers
{
    public class ChatAvailableChecker: HttpCheckerBase
    {
        private static readonly JsonSerializer JsonSerializer = JsonSerializer.Create();

        protected override HttpRequestMessage CreateRequest()
        {
            var uri = new Uri(Address);
            var h = uri.Scheme + Uri.SchemeDelimiter + uri.Host;
            var requestUri = Address.Replace(h, h + "/api/biocontext");
            var request = new HttpRequestMessage(HttpMethod.Get, requestUri);
            return request;
        }

        protected override async Task<bool> IsAvailable(HttpResponseMessage response, CancellationToken cancellationToken)
        {
            var text = await response.Content.ReadAsStringAsync();

            using (var reader = new StringReader(text))
            using (var jReader = new JsonTextReader(reader))
            {
                var data = JsonSerializer.Deserialize<Data>(jReader);
                if (data.Status == "offline")
                    return false;
                return true;
            }
        }

        internal static string GetLastBroadcastText(string text)
        {
            var i1 = text.IndexOf("Last Broadcast", StringComparison.InvariantCulture);
            if (i1 < 0)
                return null;

            var i2 = text.Substring(0, i1).LastIndexOf("<div class=\"attribute\">", StringComparison.InvariantCulture);

            const string div = "</div>";
            var i3 = i2;
            for (var i = 0; i < 3; i++)
                i3 += text.Substring(i3).IndexOf(div, StringComparison.InvariantCulture) + div.Length;
    
            var s = text.Substring(i2, i3 - i2);
            var xml = XElement.Parse(s);
            return xml.Elements().ToArray()[1].Value;
        }

        public class Data
        { 
            [JsonProperty("room_status")]
            public string Status { get; set; }
        }
    }
}