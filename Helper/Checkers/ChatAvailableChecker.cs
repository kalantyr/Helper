using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;

[assembly:InternalsVisibleTo("Helper.Tests")]

namespace Helper.Checkers
{
    public class ChatAvailableChecker: HttpCheckerBase
    {
        private const int PrevCount = 3;

        private readonly Queue<string> _prevTexts = new Queue<string>();

        protected override HttpRequestMessage CreateRequest()
        {
            const string headers = @"
                :method: GET
                :scheme: https
                sec-fetch-dest: empty
                sec-fetch-mode: cors
                sec-fetch-site: same-origin
                user-agent: Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/80.0.3987.163 Safari/537.36
                x-requested-with: XMLHttpRequest";

            var uri = new Uri(Address);
            var h = uri.Scheme + Uri.SchemeDelimiter + uri.Host;
            var requestUri = Address.Replace(h, h + "/api/panel_context/");
            var request = new HttpRequestMessage(HttpMethod.Get, requestUri);

            var lines = headers.Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries).Select(ln => ln.Trim());
            foreach (var line in lines)
            {
                var j = line.IndexOf(":");
                if (j > 0)
                {
                    var name = line.Substring(0, j);
                    var value = line.Substring(j + 1);
                    request.Headers.Add(name, value);
                }
            }

            return request;
        }

        protected override async Task<bool> IsAvailable(HttpResponseMessage response, CancellationToken cancellationToken)
        {
            var text = await response.Content.ReadAsStringAsync();

            var available = !string.IsNullOrWhiteSpace(text);

            if (available)
            {
                if (_prevTexts.Count > 1 && _prevTexts.All(t => t == text))
                    available = false;
                if (available)
                {
                    using var request = new HttpRequestMessage(HttpMethod.Get, Address);
                    var html = await SendAsync(request, cancellationToken);
                    text = await html.Content.ReadAsStringAsync();
                    var s = GetLastBroadcastText(text);
                    if (s == null || s.Contains("minut") || s.Contains(" ago"))
                        available = false;
                }
            }

            _prevTexts.Enqueue(text);
            if (_prevTexts.Count > PrevCount)
                _prevTexts.Dequeue();

            return available;
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
    }
}