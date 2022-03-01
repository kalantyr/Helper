using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Helper.Checkers;
using Helper.Events;
using Helper.Jobs;
using Helper.Jobs.Impl;
using Newtonsoft.Json;

namespace Helper
{
    public class Project
    {
        private static readonly JsonSerializer JsonSerializer = JsonSerializer.CreateDefault(new JsonSerializerSettings
        {
            Formatting = Formatting.Indented,
            DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate
        });

        public event Action<Project, IChecker> CheckerRemoved;

        [JsonIgnore]
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

        [JsonIgnore]
        public IReadOnlyCollection<IJob> AllJobs
        {
            get
            {
                return Jobs != null
                    ? Jobs.SyncFilesJobs
                    : Array.Empty<IJob>();
            }
        }

        [JsonIgnore]
        public IReadOnlyCollection<IEvent> AllEvents => Events.TimeEvents;

        public AllCheckers Checkers { get; set; }

        public AllJobs Jobs { get; set; }

        public AllEvents Events { get; set; }

        public Project()
        {
            //const string gitHost = "http://tfs4alm10v:8080/tfs/TFS2005%20-%20upgraded%20Projects/CustomerPortal/_git/";

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
                }
            };
        }

        public void Save(Stream stream)
        {
            using var writer = new StreamWriter(stream);
                JsonSerializer.Serialize(writer, this);
        }

        public static Project Load(Stream stream)
        {
            using var reader = new StreamReader(stream);
            using var jsonReader = new JsonTextReader(reader);
            var project = JsonSerializer.Deserialize<Project>(jsonReader);

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
        public ChatAvailableChecker[] ChatAvailableCheckers { get; set; }

        public BngChecker[] BngCheckers { get; set; }
    }

    public class AllJobs
    {
        public SyncFilesJob[] SyncFilesJobs { get; set; } = Array.Empty<SyncFilesJob>();
    }

    public class AllEvents
    {
        public TimeEvent[] TimeEvents { get; set; }
    }
}
