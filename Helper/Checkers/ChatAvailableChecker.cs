using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using Newtonsoft.Json;

namespace Helper.Checkers
{
    public class ChatAvailableChecker: IChecker, IDisposable
    {
        private const int PrevCount = 3;

        private readonly HttpClient _httpClient;

        private volatile bool _checkInProcess;
        private readonly Queue<string> _prevTexts = new Queue<string>();

        [JsonIgnore]
        public string Name
        {
            get
            {
                var uri = new Uri(Address);
                return uri.AbsolutePath.Trim('/');
            }
        }

        public bool IsDisabled { get; set; }

        public TimeSpan? Interval => null;

        public string Address { get; set; }

        [JsonIgnore]
        public ICheckerHistory History { get; }

        public void CopyToClipboard()
        {
            Clipboard.SetText(Address);
        }

        [JsonIgnore]
        public EventHandler Notify { get; set; }
        
        public bool NeedNotify { get; set; }

        public ChatAvailableChecker()
        {
            var handler = new HttpClientHandler { UseDefaultCredentials = true };
            _httpClient = new HttpClient(handler, true);

            History = new CheckerHistory();
        }

        public async Task Check(CancellationToken cancellationToken)
        {
            if (_checkInProcess)
                return;

            try
            {
                _checkInProcess = true;

                using var request = CreateRequest();
                var response = await _httpClient.SendAsync(request, HttpCompletionOption.ResponseContentRead,
                    cancellationToken);
                response.EnsureSuccessStatusCode();

                var text = await response.Content.ReadAsStringAsync();

                var lastResult = (bool?) History.LastValue;

                var available = !string.IsNullOrWhiteSpace(text);
                if (available)
                    if (_prevTexts.Count > 1 && _prevTexts.All(t => t == text))
                        available = false;

                History.AddResult(DateTime.Now, available);

                if (lastResult == false && available)
                    if (NeedNotify)
                        Notify?.Invoke(this, EventArgs.Empty);

                _prevTexts.Enqueue(text);
                if (_prevTexts.Count > PrevCount)
                    _prevTexts.Dequeue();
            }
            catch (Exception e)
            {
                Debug.WriteLine(e);
                History.AddResult(DateTime.Now, false);
            }
            finally
            {
                _checkInProcess = false;
            }
        }

        private HttpRequestMessage CreateRequest()
        {
            var headers = @"
                :method: GET
                :scheme: https
                accept: */*
                accept-encoding: gzip, deflate, br
                accept-language: ru-RU,ru;q=0.9,en-US;q=0.8,en;q=0.7
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

        public void Dispose()
        {
            _httpClient?.Dispose();
        }
    }
}
