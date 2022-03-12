using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Media;
using Helper.Jobs;
using Helper.Jobs.Impl;

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
                {
                    if (_job is SyncFilesJob syncFilesJob)
                    {
                    }
                }

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
            object lastValue = true;

            var brush = GetBackground(lastValue);
            Background = brush;

            _tbName.Text = Job != null
                ? Job.Name
                : "-";

            if (lastValue is Exception error)
            {
                _tbMessage.Visibility = Visibility.Visible;
                _tbMessage.Text = error.GetBaseException().Message;
            }
            else
                _tbMessage.Visibility = Visibility.Collapsed;
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
            var lastValues = Job.History.Values
                .OrderByDescending(v => v.Key)
                .Take(50);
            var text = string.Join(Environment.NewLine, lastValues.Select(ToString));
            MessageBox.Show(text, "History", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private static string ToString(KeyValuePair<DateTime, object> v)
        {
            if (v.Value is Exception error)
                return $"{v.Key:hh:mm:ss} - {error.GetBaseException().Message}";

            return $"{v.Key:hh:mm:ss} - {v.Value}";
        }
    }
}
