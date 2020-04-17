using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
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
        private const int RemoveOlderThanDays = 60;

        private static readonly IDictionary<string, Credentials> CredentialsCache = new ConcurrentDictionary<string, Credentials>();

        public string Url { get; set; }

        private string RepositoryHost
        {
            get
            {
                var i = Url.LastIndexOf('/');
                if (i < 0)
                    throw new NotImplementedException();

                return Url.Substring(0, i);
            }
        }

        [JsonIgnore]
        public ICheckerHistory History { get; }

        public Action<IJob, string> Message { get; set; }

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

        public Func<string, Credentials> GetCredentials;

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

                var successCount = 0;

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

                    Message?.Invoke(this, "Найдено веток для удаления: " + forRemove.Length);
                    foreach (var branch in forRemove)
                        try
                        {
                            repository.Network.Push(remote, ":" + branch.UpstreamBranchCanonicalName, options);
                            successCount++;
                            if (LastPushError != null)
                            {
                                successCount--;
                                if (LastPushError.Message.Contains("You need to have 'ForcePush'"))
                                    continue;
                                throw new Exception(LastPushError.Message);
                            }
                        }
                        finally
                        {
                            LastPushError = null;
                        }
                }

                Message?.Invoke(this, "Удалено веток: " + successCount);

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
                var options = new CloneOptions { CredentialsProvider = CredentialsHandler };
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

        private Credentials CredentialsHandler(string url, string usernamefromurl, SupportedCredentialTypes types)
        {
            if (!CredentialsCache.ContainsKey(RepositoryHost))
            {
                if (GetCredentials != null)
                {
                    var credentials = GetCredentials("Enter credentials for " + RepositoryHost);
                    if (credentials != null)
                        CredentialsCache.Add(RepositoryHost, credentials);
                    else
                        return null;
                }
                else
                    return null;
            }

            return CredentialsCache[RepositoryHost];
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
