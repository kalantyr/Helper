using System;
using System.Diagnostics;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using Newtonsoft.Json;

namespace Helper.Checkers
{
    public abstract class HttpCheckerBase: IChecker, IDisposable
    {
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
                var response = await _httpClient.SendAsync(request, HttpCompletionOption.ResponseContentRead, cancellationToken);
                response.EnsureSuccessStatusCode();

                var lastResult = (bool?)History.LastValue;

                var available = await IsAvailable(response);
                History.AddResult(DateTime.Now, available);

                if (lastResult == false && available)
                    if (NeedNotify)
                        Notify?.Invoke(this, EventArgs.Empty);
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

        protected abstract HttpRequestMessage CreateRequest();

        protected abstract Task<bool> IsAvailable(HttpResponseMessage response);

        public void Dispose()
        {
            _httpClient?.Dispose();
        }
    }
}
