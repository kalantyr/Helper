using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Helper.Checkers;
using LibGit2Sharp;
using Newtonsoft.Json;

namespace Helper.Jobs
{
    public class ClearGitRepositoryJob : IJob
    {
        private static readonly string[] BranchStopWords =
        {
            "release", "master"
        };
        
        public string Url { get; set; }

        public string UserName { get; set; }

        public string Password { get; set; }

        [JsonIgnore]
        public ICheckerHistory History { get; }

        private string RepositoryFolder => Path.Combine(App.TempFolder, "git", Name);

        [JsonIgnore]
        public string Name
        {
            get
            {
                if (Url == null)
                    return "Url == null";

                var repoName = Url.Split('/').Last();
                if (string.IsNullOrEmpty(repoName))
                    return "repoName not found";

                return repoName.Replace(".git", string.Empty);
            }
        }

        public ClearGitRepositoryJob()
        {
            History = new CheckerHistory();
        }

        public async Task Run(CancellationToken cancellationToken)
        {
            try
            {
                Fetch();

                var options = new PushOptions
                {
                    CredentialsProvider = CredentialsHandler,
                    OnPushStatusError = OnPushStatusError
                };

                using var repository = new Repository(RepositoryFolder);
                {
                    var remote = repository.Network.Remotes["origin"];

                    foreach (var branch in repository.Branches.Where(br => br.IsRemote))
                    {
                        if (Skip(branch))
                            continue;

                        repository.Network.Push(remote, ":" + branch.UpstreamBranchCanonicalName, options);
                    }
                }

                History.AddResult(DateTime.Now, true);
            }
            catch (Exception e)
            {
                History.AddResult(DateTime.Now, e);
            }
        }

        private void OnPushStatusError(PushStatusError pushstatuserrors)
        {
            throw new NotImplementedException();
        }

        private void Fetch()
        {
            if (!Directory.Exists(RepositoryFolder))
            {
                Directory.CreateDirectory(RepositoryFolder);
                var options = new CloneOptions { CredentialsProvider = CredentialsHandler };
                Repository.Clone(Url, RepositoryFolder, options);
            }
            else
                using (var repository = new Repository(RepositoryFolder))
                {
                    var options = new FetchOptions { CredentialsProvider = CredentialsHandler };
                    Commands.Fetch(repository, "origin", new string[0], options, string.Empty);
                }
        }

        private Credentials CredentialsHandler(string url, string usernamefromurl, SupportedCredentialTypes types)
        {
            return new UsernamePasswordCredentials
            {
                Username = UserName,
                Password = Password
            };
        }

        private static bool Skip(Branch branch)
        {
            foreach (var stopWord in BranchStopWords)
                if (branch.CanonicalName.Contains("/" + stopWord, StringComparison.InvariantCultureIgnoreCase))
                    return true;

            var lastCommitDate = branch.Commits.Max(c => c.Author.When);
            var elapsed = DateTimeOffset.Now - lastCommitDate;
            if (elapsed.TotalDays < 90)
                return true;

            return false;
        }
    }
}
