using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Helper.Checkers;
using Helper.Utils.Jobs;
using LibGit2Sharp;
using Newtonsoft.Json;

namespace Helper.Jobs
{
    public class ClearGitRepositoryJob : IJob
    {
        private const int RemoveOlderThanDays = 90;

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

        private PushStatusError LastPushError { get; set; }

        public ClearGitRepositoryJob()
        {
            History = new CheckerHistory();
        }

        public async Task Run(CancellationToken cancellationToken)
        {
            try
            {
                CloneOrFetch();

                var options = new PushOptions
                {
                    CredentialsProvider = CredentialsHandler,
                    OnPushStatusError = OnPushStatusError
                };

                using var repository = new Repository(RepositoryFolder);
                {
                    var remote = repository.Network.Remotes["origin"];

                    var forRemove = repository.Branches
                        .Where(br => br.IsRemote)
                        .Where(branch => !Skip(branch))
                        .OrderBy(GetLastCommitDate)
                        .ToArray();

                    foreach (var branch in forRemove)
                        try
                        {
                            repository.Network.Push(remote, ":" + branch.UpstreamBranchCanonicalName, options);
                            if (LastPushError != null)
                            {
                                if (LastPushError.Message.Contains("You need to have 'ForcePush'"))
                                    continue;
                                throw new Exception(LastPushError.Message);
                            }
                            else
                                Equals(null);
                        }
                        finally
                        {
                            LastPushError = null;
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
            LastPushError = pushstatuserrors;
        }

        private void CloneOrFetch()
        {
            if (!Directory.Exists(RepositoryFolder))
                Clone();
            else
                Fetch();
        }

        private void Fetch()
        {
            using (var repository = new Repository(RepositoryFolder))
            {
                var options = new FetchOptions {CredentialsProvider = CredentialsHandler};
                Commands.Fetch(repository, "origin", new string[0], options, string.Empty);
            }
        }

        private void Clone()
        {
            try
            {
                Directory.CreateDirectory(RepositoryFolder);
                var options = new CloneOptions
                {
                    CredentialsProvider = CredentialsHandler,
                    CertificateCheck = OnCertificateCheck 
                };
                Repository.Clone(Url, RepositoryFolder, options);
            }
            catch (Exception e)
            {
                Debug.WriteLine(e);

                if (Directory.Exists(RepositoryFolder))
                    Directory.Delete(RepositoryFolder, true);

                throw;
            }
        }

        private bool OnCertificateCheck(Certificate certificate, bool valid, string host)
        {
            throw new NotImplementedException();
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
            if (ClearGitRepositoryJobUtils.SkipBranchByName(branch.CanonicalName))
                return true;

            var elapsed = DateTimeOffset.Now - GetLastCommitDate(branch);
            if (elapsed.TotalDays < RemoveOlderThanDays)
                return true;

            return false;
        }

        private static DateTimeOffset GetLastCommitDate(Branch branch)
        {
            return branch.Commits.Max(c => c.Author.When);
        }
    }
}
