using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using Helper.Core;
using Helper.Core.Jobs;
using Helper.Jobs;

namespace Helper.UserControls
{
    public partial class JobsControl
    {
        private const int MaxParallelJobs = 2;

        private static readonly Random _rand = new Random();
        private readonly System.Timers.Timer _timer;

        private Project _project;

        private static readonly TimeSpan TimerInterval = TimeSpan.FromSeconds(10);
        private static readonly TimeSpan DefaultInterval = TimeSpan.FromMinutes(10);

        public IReadOnlyCollection<IJob> SelectedJobs => (from JobControl jobControl in _lb.SelectedItems select jobControl.Job).ToArray();

        public Project Project
        {
            get => _project;
            set
            {
                if (value == Project)
                    return;

                if (_project != null)
                    _project.Jobs.Removed -= Jobs_Removed;

                _project = value;

                if (_project != null)
                    _project.Jobs.Removed += Jobs_Removed;

                TuneControls();
            }
        }

        private void Jobs_Removed(IJob job)
        {
            TuneControls();
        }

        public JobsControl()
        {
            InitializeComponent();

            TuneControls();

            _timer = new System.Timers.Timer
            {
                Interval = TimerInterval.TotalMilliseconds,
                AutoReset = true
            };
            _timer.Elapsed += OnTtimer;
            _timer.Start();
        }

        private void TuneControls()
        {
            if (Project == null)
            {
                _lb.ItemsSource = null;
                return;
            }

            var checkerControls = Project.AllJobs
                .OrderBy(ch => ch.Name)
                .Select(job => new JobControl { Job = job })
                .ToArray();
            _lb.ItemsSource = checkerControls;
        }

        private void OnTtimer(object sender, ElapsedEventArgs e)
        {
            if (Project?.AllJobs == null)
                return;

            var tasks = new List<Task>();

            foreach (var job in Project.AllJobs.OrderBy(ch => _rand.Next(Project.AllJobs.Count)))
                if (NeedRun(job))
                    tasks.Add(new Task(() => job.Run(CancellationToken.None)));

            foreach (var task in tasks.Take(MaxParallelJobs).ToArray())
                task.RunSynchronously();
        }

        private static bool NeedRun(IJob job)
        {
            if (job.IsDisabled)
                return false;

            if (job.History.LastTime == null)
                return true;

            if (job.Interval != null)
                return DateTime.Now - job.History.LastTime.Value > job.Interval.Value;

            return DateTime.Now - job.History.LastTime.Value > DefaultInterval;
        }

        private void ContextMenu_OnOpened(object sender, RoutedEventArgs e)
        {
            var selectedJobs = SelectedJobs;
            _miRunNow.IsEnabled = selectedJobs.Any();
            _miRemove.IsEnabled = selectedJobs.Any();
        }

        private void _miRunNow_OnClick(object sender, RoutedEventArgs e)
        {
            var cancellationToken = CancellationToken.None;
            Task.WaitAll(SelectedJobs.Select(j => j.Run(cancellationToken)).ToArray(), cancellationToken);
        }

        private void _miRemove_OnClick(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("Remove?", "Remove?", MessageBoxButton.YesNo, MessageBoxImage.Warning) != MessageBoxResult.Yes)
                return;

            foreach (var job in SelectedJobs)
                Project.Jobs.Remove(job);
            TuneControls();
        }
    }
}
