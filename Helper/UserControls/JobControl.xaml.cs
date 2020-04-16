using System;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Media;
using Helper.Jobs;

namespace Helper.UserControls
{
    public partial class JobControl
    {
        private IJob _job;

        public IJob Job
        {
            get => _job;
            set
            {
                if (value == _job)
                    return;

                _job = value;

                if (_job != null)
                    _job.History.Changed += HistoryChanged;

                TuneControls();
            }
        }

        public JobControl()
        {
            InitializeComponent();

            TuneControls();
        }

        private void HistoryChanged(object sender, EventArgs e)
        {
            if (Dispatcher.CheckAccess())
                TuneControls();
            else
                Dispatcher.Invoke(() => HistoryChanged(sender, e));
        }

        private void TuneControls()
        {
            var brush = GetBackground(Job?.History.LastValue);
            Background = brush;

            _tbName.Text = Job != null
                ? Job.Name
                : "-";
        }

        private async void OnRunClick(object sender, RoutedEventArgs e)
        {
            await Job.Run(CancellationToken.None);
        }

        private static SolidColorBrush GetBackground(object lastCheckResult)
        {
            if (lastCheckResult == null)
                return Brushes.LightGray;

            if (lastCheckResult is Exception)
                return Brushes.LightCoral;

            return Brushes.LightGreen;
        }

        private void OnHistoryClick(object sender, RoutedEventArgs e)
        {
            if (Job.History.LastValue is Exception error)
            {
                MessageBox.Show(error.GetBaseException().ToString(), "Exception", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            var lastValues = Job.History.Values
                .OrderByDescending(v => v.Key)
                .Take(50);
            var text = string.Join(Environment.NewLine,
                lastValues.Select(v => $"{v.Key:hh:mm:ss} - {v.Value}"));
            MessageBox.Show(text, "History", MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }
}
