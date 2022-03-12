using System;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Helper.Checkers
{
    public class BngChecker: HttpCheckerBase
    {
        protected override HttpRequestMessage CreateRequest()
        {
            const string headers = @"
                accept: text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,image/apng,*/*;q=0.8,application/signed-exchange;v=b3;q=0.9
                accept-language: ru-RU,ru;q=0.9,en-US;q=0.8,en;q=0.7
                cache-control: no-cache
                pragma: no-cache
                sec-fetch-dest: document
                sec-fetch-mode: navigate
                sec-fetch-site: same-origin
                sec-fetch-user: ?1
                upgrade-insecure-requests: 1
                user-agent: Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/81.0.4044.129 Safari/537.36";

            var requestUri = Address.Replace(Name, "profile/" + Name);
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
            var text = await response.Content.ReadAsStringAsync(cancellationToken);
            return !text.Contains("class=\"badge_online __hidden\"");
        }
    }
}
