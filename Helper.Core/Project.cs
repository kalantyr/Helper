using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Helper.Core.Checkers;
using Helper.Core.Events;
using Helper.Core.Jobs;
using Helper.Core.Jobs.Impl;
using Helper.Core.Utils;

namespace Helper.Core
{
    public class Project
    {
        public event Action<Project, IChecker> CheckerRemoved;

        [System.Text.Json.Serialization.JsonIgnore]
        public IReadOnlyCollection<IChecker> AllCheckers
        {
            get
            {
                var list = new List<IChecker>();
                list.AddRange(Checkers.ChatAvailableCheckers);
                list.AddRange(Checkers.BngCheckers);
                return list;
            }
        }

        [System.Text.Json.Serialization.JsonIgnore]
        public IReadOnlyCollection<IJob> AllJobs
        {
            get
            {
                var list = new List<IJob>();
                if (Jobs != null)
                {
                    list.AddRange(Jobs.SyncFilesJobs);
                    list.AddRange(Jobs.EncryptFilesJobs);
                }
                return list;
            }
        }

        [System.Text.Json.Serialization.JsonIgnore]
        public IReadOnlyCollection<IEvent> AllEvents => Events.TimeEvents;

        public AllCheckers Checkers { get; set; }

        public AllJobs Jobs { get; set; }

        public AllEvents Events { get; set; }

        public Project()
        {
            Checkers = new AllCheckers
            {
                ChatAvailableCheckers = new ChatAvailableChecker[0],
                BngCheckers = new BngChecker[0]
            };
            Jobs = CreateDevJobs();
            Events = new AllEvents
            {
                TimeEvents = new []
                {
                    new TimeEvent { Name = "Test", WarningPeriod = TimeSpan.FromMinutes(5) },
                }
            };
        }

        private static AllJobs CreateDevJobs()
        {
            return new AllJobs
            {
                SyncFilesJobs = new []
                {
                    new SyncFilesJob
                    {
                        RootFolders = new []
                        {
                            "C:\\Users\\Kalavarda\\Мой диск",
                            "C:\\_\\2022_03\\01\\TestShare"
                        }
                    }
                },
                EncryptFilesJobs = new []
                {
                    new EncryptFilesJob
                    {
                        Options = new EncryptFilesJob.EncryptOptions
                        {
                            SourceFolder = "C:\\Users\\Kalavarda\\Мой диск",
                            DestFolder = "C:\\Users\\Kalavarda\\YandexDisk"
                        }
                    }
                }
            };
        }

        public void Save(Stream stream)
        {
            new ProjectSerializer().Save(this, stream);
        }

        public static Project Load(Stream stream)
        {
            var project = new ProjectSerializer().Load(stream);

            project.Events ??= new AllEvents
            {
                TimeEvents = new[]
                {
                    new TimeEvent {Name = "Test", WarningPeriod = TimeSpan.FromMinutes(5)}
                }
            };

            if (!project.AllJobs.Any())
                project.Jobs = CreateDevJobs();

            return project;
        }

        public void Remove(IChecker checker)
        {
            if (checker == null) throw new ArgumentNullException(nameof(checker));

            if (Checkers.BngCheckers.Contains(checker))
            {
                Checkers.BngCheckers = Checkers.BngCheckers.Where(ch => ch != checker).ToArray();
                CheckerRemoved?.Invoke(this, checker);
                return;
            }

            if (Checkers.ChatAvailableCheckers.Contains(checker))
            {
                Checkers.ChatAvailableCheckers = Checkers.ChatAvailableCheckers.Where(ch => ch != checker).ToArray();
                CheckerRemoved?.Invoke(this, checker);
                return;
            }

            throw new NotImplementedException();
        }
    }

    public class AllCheckers
    {
        public ChatAvailableChecker[] ChatAvailableCheckers { get; set; } = Array.Empty<ChatAvailableChecker>();

        public BngChecker[] BngCheckers { get; set; } = Array.Empty<BngChecker>();
    }

    public class AllJobs
    {
        public event Action<IJob> Removed;

        public SyncFilesJob[] SyncFilesJobs { get; set; } = Array.Empty<SyncFilesJob>();
        
        public EncryptFilesJob[] EncryptFilesJobs { get; set; } = Array.Empty<EncryptFilesJob>();

        public void Remove(IJob job)
        {
            if (job == null) throw new ArgumentNullException(nameof(job));

            if (job is SyncFilesJob syncFilesJob)
            {
                SyncFilesJobs = SyncFilesJobs.Remove(syncFilesJob);
                Removed?.Invoke(job);
                return;
            }

            if (job is EncryptFilesJob encryptFilesJob)
            {
                EncryptFilesJobs = EncryptFilesJobs.Remove(encryptFilesJob);
                Removed?.Invoke(job);
                return;
            }

            throw new NotImplementedException();
        }
    }

    public class AllEvents
    {
        public TimeEvent[] TimeEvents { get; set; } = Array.Empty<TimeEvent>();
    }
}
