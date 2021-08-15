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
    public abstract class HttpCheckerBase: IChecker, IDisposable
    {
        private const char Separator = '|';

        private readonly HttpClient _httpClient;

        private volatile bool _checkInProcess;

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

        [JsonIgnore]
        public DateTime? LastAvailableTime
        {
            get
            {
                if (Settings.Default.LastCheckerAvailable == null)
                    return null;

                using var request = CreateRequest();
                var key = GetLastAvailableTimeKey(request);
                var tuple = Settings.Default.LastCheckerAvailable
                    .Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries)
                    .Select(ParseLastDateTime)
                    .FirstOrDefault(t => t.Item1 == key);
                return tuple?.Item2;
            }
        }

        private static Tuple<string, DateTime?> ParseLastDateTime(string s)
        {
            if (string.IsNullOrWhiteSpace(s))
                return null;
            var parts = s.Split(Separator);
            return new Tuple<string, DateTime?>(parts[0], DateTime.Parse(parts[1]));
        }

        private static void SetLastAvailableTime(HttpRequestMessage requestMessage)
        {
            var lines = !string.IsNullOrWhiteSpace(Settings.Default.LastCheckerAvailable)
                ? Settings.Default.LastCheckerAvailable.Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries).ToList()
                : new List<string>();

            var line = lines.FirstOrDefault(ln => ln.StartsWith(GetLastAvailableTimeKey(requestMessage)));
            if (line != null)
                lines.Remove(line);
            lines.Add(string.Join(Separator, GetLastAvailableTimeKey(requestMessage), DateTime.Now));

            Settings.Default.LastCheckerAvailable = string.Join(Environment.NewLine, lines);
            Settings.Default.Save();
        }

        private static string GetLastAvailableTimeKey(HttpRequestMessage message)
        {
            return message.RequestUri.AbsoluteUri.TrimEnd('/');
        }

        public event Action<IChecker> NotFound;

        protected HttpCheckerBase()
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
                var response = await SendAsync(request, cancellationToken);

                var lastResult = (bool?)History.LastValue;

                var available = await IsAvailable(response, cancellationToken);
                History.AddResult(DateTime.Now, available);
                if (available)
                    SetLastAvailableTime(request);

                if (lastResult == false && available)
                    if (NeedNotify)
                        Notify?.Invoke(this, EventArgs.Empty);
            }
            catch (Exception e)
            {
                if (e.GetBaseException().Message.Contains("404"))
                    NotFound?.Invoke(this);
                else
                {
                    Debug.WriteLine(e);
                    History.AddResult(DateTime.Now, false);
                }
            }
            finally
            {
                _checkInProcess = false;
            }
        }

        internal async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var response = await _httpClient.SendAsync(request, HttpCompletionOption.ResponseContentRead, cancellationToken);
            response.EnsureSuccessStatusCode();
            return response;
        }

        protected abstract HttpRequestMessage CreateRequest();

        protected abstract Task<bool> IsAvailable(HttpResponseMessage response, CancellationToken cancellationToken);

        public void Dispose()
        {
            _httpClient?.Dispose();
        }
    }
}
