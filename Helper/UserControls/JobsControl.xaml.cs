﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
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

        public Project Project
        {
            get => _project;
            set
            {
                if (value == Project)
                    return;

                _project = value;
                TuneControls();
            }
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
            if (Project?.AllCheckers == null)
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
    }
}
