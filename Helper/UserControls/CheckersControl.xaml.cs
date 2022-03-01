using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using Helper.Checkers;
using Helper.Utils;

namespace Helper.UserControls
{
    public partial class CheckersControl
    {
        private const int MaxParallelChecks = 3;

        private static readonly Random _rand = new Random();
        private readonly System.Timers.Timer _timer;
        private Project _project;

        private static readonly TimeSpan TimerInterval = TimeSpan.FromSeconds(10);
        private static readonly TimeSpan DefaultCheckInterval = TimeSpan.FromMinutes(2);

        public Project Project
        {
            get => _project;
            set
            {
                _project = value;
                TuneControls();

                if (_project != null)
                {
                    foreach (var checker in _project.AllCheckers)
                    {
                        checker.Notify += OnNotify;
                        checker.NotFound += ch =>
                        {
                            _project.Remove(ch);
                        };
                    }

                    _project.CheckerRemoved += OnCheckerRemoved;
                }
            }
        }

        private void OnCheckerRemoved(Project prj, IChecker ch)
        {
            TuneControls();
        }

        private void OnNotify(object sender, EventArgs e)
        {
            if (Dispatcher.CheckAccess())
                WindowsUtils.Flash(Application.Current.MainWindow);
            else
                Dispatcher.Invoke(() => OnNotify(sender, e));
        }

        public IChecker[] SelectedCheckers
        {
            get
            {
                if (_lb.SelectedItems.Count == 0)
                    return new IChecker[0];

                return _lb.SelectedItems
                    .Cast<CheckerControl>()
                    .Select(checkerControl => checkerControl.Checker)
                    .ToArray();
            }
        }

        public IChecker SelectedChecker
        {
            get
            {
                var selectedCheckers = SelectedCheckers;
                if (selectedCheckers.Length == 1)
                    return selectedCheckers[0];
                else
                    return null;
            }
        }

        public CheckersControl()
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

        private void OnTtimer(object sender, ElapsedEventArgs e)
        {
            if (Project?.AllCheckers == null)
                return;

            var tasks = new List<Task>();

            foreach (var checker in Project.AllCheckers.OrderBy(ch => _rand.Next(Project.AllCheckers.Count)))
                if (NeedCheck(checker))
                    tasks.Add(new Task(() => checker.Check(CancellationToken.None)));

            foreach (var task in tasks.Take(MaxParallelChecks).ToArray())
                task.RunSynchronously();
        }

        private static bool NeedCheck(IChecker checker)
        {
            if (checker.IsDisabled)
                return false;

            if (checker.History.LastTime == null)
                return true;

            if (checker.Interval != null)
                return DateTime.Now - checker.History.LastTime.Value > checker.Interval.Value;

            return DateTime.Now - checker.History.LastTime.Value > DefaultCheckInterval;
        }

        private void TuneControls()
        {
            if (Project == null)
            {
                _lb.ItemsSource = null;
                return;
            }

            var checkerControls = Project.AllCheckers
                .OrderByDescending(ch => ch.Name)
                .Select(ch => new CheckerControl { Checker = ch })
                .ToArray();
            _lb.ItemsSource = checkerControls;

            _btnRemove.IsEnabled = SelectedCheckers.Any();
        }

        private void OnRemoveClick(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("Remove ?", "Confirm", MessageBoxButton.YesNo, MessageBoxImage.Warning, MessageBoxResult.Yes) != MessageBoxResult.Yes)
                return;

            throw new NotImplementedException();
        }

        private void OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
        }
    }
}
