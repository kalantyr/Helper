using System;
using System.Linq;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using Helper.Events;

namespace Helper.UserControls
{
    public partial class EventsControl
    {
        private static readonly TimeSpan TimerInterval = TimeSpan.FromSeconds(10);
        private readonly Timer _timer;
        private Project _project;

        public Project Project
        {
            get => _project;
            set
            {
                _project = value;
                TuneControls();
/*
                foreach (var ev in _project.AllEvents)
                    ev.Notify += OnNotify;

                _project.CheckerRemoved += OnCheckerRemoved;
*/
            }
        }

        public IEvent[] SelectedEvents
        {
            get
            {
                if (_lb.SelectedItems.Count == 0)
                    return new IEvent[0];

                return _lb.SelectedItems
                    .Cast<EventControl>()
                    .Select(control => control.Event)
                    .ToArray();
            }
        }

        public EventsControl()
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
            if (Project?.AllEvents == null)
                return;

            foreach (var ev in Project.AllEvents)
                if (ev.NeedNotify)
                    OnNotify(ev);
        }

        private void TuneControls()
        {
            if (Project == null)
            {
                _lb.ItemsSource = null;
                return;
            }

            var eventControls = Project.AllEvents
                .OrderBy(ev => ev.Name)
                .Select(ev => new EventControl { Event = ev })
                .ToArray();
            _lb.ItemsSource = eventControls;

            _btnRemove.IsEnabled = SelectedEvents.Any();
        }

        private void OnNotify(IEvent ev)
        {
            if (Dispatcher.CheckAccess())
            {
                var mainWindow = Application.Current.MainWindow;

                mainWindow.Topmost = true;
                if (MessageBox.Show(mainWindow, ev.Name, ev.Name, MessageBoxButton.OK, MessageBoxImage.Warning) == MessageBoxResult.OK)
                    mainWindow.Topmost = false;
            }
            else
                Dispatcher.Invoke(() => OnNotify(ev));
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
