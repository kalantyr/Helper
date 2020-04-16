using System.Collections.Generic;
using System.IO;
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

        [JsonIgnore]
        public IReadOnlyCollection<IChecker> AllCheckers => Checkers.ChatAvailableCheckers;

        [JsonIgnore]
        public IReadOnlyCollection<IJob> AllJobs => Jobs.ClearGitRepositoryJobs;

        public AllCheckers Checkers { get; set; }

        public AllJobs Jobs { get; set; }

        public Project()
        {
            const string userName = @"REGION\VTB198703";
            const string password = "******";
            const string gitHost = "http://tfs4alm10v:8080/tfs/TFS2005%20-%20upgraded%20Projects/CustomerPortal/_git/";

            Checkers = new AllCheckers
            {
                ChatAvailableCheckers = new ChatAvailableChecker[0]
            };
            Jobs = new AllJobs
            {
                ClearGitRepositoryJobs = new[]
                {
                    new ClearGitRepositoryJob
                    {
                        Url = gitHost + "nuget-logs",
                        UserName = userName,
                        Password = password
                    },
                    new ClearGitRepositoryJob
                    {
                        Url = gitHost + "nuget-adapters",
                        UserName = userName,
                        Password = password
                    },
                    new ClearGitRepositoryJob
                    {
                        Url = gitHost + "database",
                        UserName = userName,
                        Password = password
                    },
                    new ClearGitRepositoryJob
                    {
                        Url = gitHost + "user-ui",
                        UserName = userName,
                        Password = password
                    },
                    new ClearGitRepositoryJob
                    {
                        Url = gitHost + "support",
                        UserName = userName,
                        Password = password
                    },
                    new ClearGitRepositoryJob
                    {
                        Url = gitHost + "portal",
                        UserName = userName,
                        Password = password
                    },
                    new ClearGitRepositoryJob
                    {
                        Url = gitHost + "Vtb.WebServices.AuthorizationServices",
                        UserName = userName,
                        Password = password
                    },
                    new ClearGitRepositoryJob
                    {
                        Url = gitHost + "Vtb.WebServices.AuthService",
                        UserName = userName,
                        Password = password
                    },
                    new ClearGitRepositoryJob
                    {
                        Url = gitHost + "v6.backend.integration.api",
                        UserName = userName,
                        Password = password
                    },
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
            return JsonSerializer.Deserialize<Project>(jsonReader);
        }
    }

    public class AllCheckers
    {
        public ChatAvailableChecker[] ChatAvailableCheckers { get; set; }
    }

    public class AllJobs
    {
        public ClearGitRepositoryJob[] ClearGitRepositoryJobs { get; set; }
    }
}
