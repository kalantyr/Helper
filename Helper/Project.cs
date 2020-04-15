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
            Checkers = new AllCheckers
            {
                ChatAvailableCheckers = new ChatAvailableChecker[0]
            };
            Jobs = new AllJobs
            {
                ClearGitRepositoryJobs = new ClearGitRepositoryJob[0]
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

            project.Jobs = new AllJobs
            {
                ClearGitRepositoryJobs = new[]
                {
                    new ClearGitRepositoryJob { Url = "https://github.com/kalantyr/Art.git", UserName = "userName", Password = "password" }
                }
            };

            return project;
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
