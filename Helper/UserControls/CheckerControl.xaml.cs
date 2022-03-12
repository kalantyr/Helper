using System;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Media;
using Helper.Core.Checkers;

namespace Helper.UserControls
{
    public partial class CheckerControl
    {
        private IChecker _checker;

        public IChecker Checker
        {
            get => _checker;
            set
            {
                if (_checker == value)
                    return;

                _checker = value;

                if (_checker != null)
                    _checker.History.Changed += HistoryChanged;

                TuneControls();
            }
        }

        private void HistoryChanged(object sender, EventArgs e)
        {
            if (Dispatcher.CheckAccess())
                TuneControls();
            else
                Dispatcher.Invoke(() => HistoryChanged(sender, e));
        }

        public CheckerControl()
        {
            InitializeComponent();
            TuneControls();
        }

        private async void OnRefreshClick(object sender, RoutedEventArgs e)
        {
            await Checker.Check(CancellationToken.None);
            TuneControls();
        }

        private void TuneControls()
        {
            var brush = GetBackground(Checker?.History.LastValue);
            Background = brush;

            _tbName.Text = Checker != null
                ? Checker.Name
                : "-";

            var lastCheckResult = Checker?.History.LastValue as bool?;
            if (lastCheckResult != true)
            {
                var lastDate = Checker?.LastAvailableTime;
                if (lastDate != null)
                {
                    var elapsed = DateTime.Now - lastDate.Value;
                    if (elapsed.TotalHours < 1)
                        _tbLastTime.Text = $"{Math.Round(elapsed.TotalMinutes)} мин. назад";
                    else
                        if (elapsed.TotalDays < 1)
                            _tbLastTime.Text = $"{Math.Round(elapsed.TotalHours)} ч. назад";
                        else
                            _tbLastTime.Text = $"{Math.Round(elapsed.TotalDays)} дн. назад";
                }
                else
                    _tbLastTime.Text = "?";
            }
            else
                _tbLastTime.Text = null;
        }

        private static SolidColorBrush GetBackground(object lastCheckResult)
        {
            if (lastCheckResult == null)
                return Brushes.LightGray;

            var b = lastCheckResult as bool?;
            if (b != null)
                switch (b.Value)
                {
                    case true:
                        return Brushes.LightGreen;
                    case false:
                        return Brushes.LightCoral;
                }

            throw new NotImplementedException();
        }

        private void OnCopyClick(object sender, RoutedEventArgs e)
        {
            var text = Checker.GetTextForClipboard();
            Clipboard.SetText(text);
        }

        private void OnHistoryClick(object sender, RoutedEventArgs e)
        {
            var lastValues = Checker.History.Values
                .OrderByDescending(v => v.Key)
                .Take(50);
            var text = string.Join(Environment.NewLine,
                lastValues.Select(v => $"{v.Key:hh:mm:ss} - {v.Value}"));
            MessageBox.Show(text, "History", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void OnRemoveClick(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show($"Remove item {Checker.Name}?", "Warning", MessageBoxButton.YesNo, MessageBoxImage.Warning, MessageBoxResult.Yes) != MessageBoxResult.Yes)
                return;

            var project = ((App)Application.Current).Project;
            project.Remove(Checker);
        }
    }
}
