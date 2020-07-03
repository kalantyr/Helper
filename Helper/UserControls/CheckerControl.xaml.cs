using System;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Media;
using Helper.Checkers;

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
            Checker.CopyToClipboard();
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
