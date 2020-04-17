using Helper.Utils.Jobs;
using NUnit.Framework;

namespace Helper.Tests
{
    [TestFixture]
    public class ClearGitRepositoryJob_Tests
    {
        [TestCase("refs/remotes/origin/master", true)]
        [TestCase("refs/remotes/origin/prelive", true)]
        [TestCase("refs/remotes/origin/releases/release13.1", true)]
        [TestCase("refs/remotes/origin/production_12_2", true)]
        [TestCase("refs/remotes/origin/Releases/12.2.4", true)]
        [TestCase("refs/remotes/origin/release/12.2.4", true)]
        [TestCase("refs/remotes/origin/HEAD", true)]
        [TestCase("refs/remotes/origin/R6.4", true)]
        [TestCase("refs/remotes/origin/R10.5", true)]

        [TestCase("refs/remotes/origin/master-241285", false)]
        [TestCase("refs/remotes/origin/feature/PR21910-93-extended-logging", false)]
        [TestCase("refs/remotes/origin/clients/bug/master/313047", false)]
        [TestCase("refs/remotes/origin/clients/task/master/325128_nuget", false)]
        [TestCase("refs/remotes/origin/lead/ou/bug/315608/master", false)]
        [TestCase("refs/remotes/origin/Branch_release/13.2/#oleg", false)]
        [TestCase("refs/remotes/origin/fp_master", false)]
        [TestCase("refs/remotes/origin/FP_11.5", false)]
        [TestCase("refs/remotes/origin/fp/tickets/rigths_336975", false)]
        public void SkipBranchByName_Test(string branchName, bool skip)
        {
            Assert.AreEqual(skip, ClearGitRepositoryJobUtils.SkipBranchByName(branchName));
        }
    }
}
