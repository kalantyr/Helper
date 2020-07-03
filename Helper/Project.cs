using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Helper.Checkers;
using Helper.Jobs;
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
        public IReadOnlyCollection<IJob> AllJobs => Jobs.ClearGitRepositoryJobs;

        public AllCheckers Checkers { get; set; }

        public AllJobs Jobs { get; set; }

        public Project()
        {
            //const string gitHost = "http://tfs4alm10v:8080/tfs/TFS2005%20-%20upgraded%20Projects/CustomerPortal/_git/";

            Checkers = new AllCheckers
            {
                ChatAvailableCheckers = new ChatAvailableChecker[0],
                BngCheckers = new BngChecker[0]
            };
            Jobs = new AllJobs
            {
                ClearGitRepositoryJobs = new ClearGitRepositoryJob[0]
                /*
                ClearGitRepositoryJobs = new[]
                {
                    new ClearGitRepositoryJob { Url = gitHost + "nuget-logs" },
                    new ClearGitRepositoryJob { Url = gitHost + "nuget-adapters" },
                    new ClearGitRepositoryJob { Url = gitHost + "database" },
                    new ClearGitRepositoryJob { Url = gitHost + "user-ui" },
                    new ClearGitRepositoryJob { Url = gitHost + "support" },
                    new ClearGitRepositoryJob { Url = gitHost + "portal" },
                    new ClearGitRepositoryJob { Url = gitHost + "Vtb.WebServices.AuthorizationServices" },
                    new ClearGitRepositoryJob { Url = gitHost + "Vtb.WebServices.AuthService" },
                    new ClearGitRepositoryJob { Url = gitHost + "v6.backend.integration.api" }
                }
                */
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
            return JsonSerializer.Deserialize<Project>(jsonReader);
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
        public ClearGitRepositoryJob[] ClearGitRepositoryJobs { get; set; }
    }
}
