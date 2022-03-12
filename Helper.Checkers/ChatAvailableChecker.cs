using System;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;

[assembly:InternalsVisibleTo("Helper.Tests")]

namespace Helper.Checkers
{
    public class ChatAvailableChecker: HttpCheckerBase
    {
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
            var text = await response.Content.ReadAsStringAsync(cancellationToken);

            var data = JsonSerializer.Deserialize<Data>(text);
            return data.Status != "offline";
        }

        public class Data
        { 
            [JsonPropertyName("room_status")]
            public string Status { get; set; }
        }
    }
}