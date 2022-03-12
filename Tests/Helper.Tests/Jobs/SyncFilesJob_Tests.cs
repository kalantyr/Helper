using Helper.Core.Jobs.Impl;
using NUnit.Framework;

namespace Helper.Tests.Jobs
{
    public class SyncFilesJob_Tests
    {
        [Test]
        public void RelativePath_AreEquals_Test()
        {
            var p1 = new SyncFilesJob.RelativePath(string.Empty, "test.txt");
            var p2 = new SyncFilesJob.RelativePath(string.Empty, "test.txt");
            Assert.IsTrue(p1.AreEquals(p2));
            Assert.IsTrue(p2.AreEquals(p1));
        }
    }
}
