using System.IO;
using Helper.Core;
using Helper.Core.Checkers;
using Helper.Core.Events;
using Helper.Core.Jobs.Impl;
using Helper.Core.Utils;
using NUnit.Framework;

namespace Helper.Tests
{
    public class ProjectSerializer_Tests
    {
        [Test]
        public void Save_Load_Test()
        {
            var project1 = new Project
            {
                Checkers = new AllCheckers
                {
                    BngCheckers = new []
                    {
                        new BngChecker { Address = "http://test.com" }
                    },
                    ChatAvailableCheckers = new []
                    {
                        new ChatAvailableChecker { Address = "http://test.com"}
                    }
                },
                Events = new AllEvents
                {
                    TimeEvents = new []
                    {
                        new TimeEvent { Schedule = "test" }
                    }
                },
                Jobs = new AllJobs
                {
                    EncryptFilesJobs = new []
                    {
                        new EncryptFilesJob
                        {
                            Password = "123",
                            Options = new EncryptFilesJob.EncryptOptions
                            {
                                DestFolder = "C:\\temp"
                            }
                        }
                    },
                    SyncFilesJobs = new []
                    {
                        new SyncFilesJob
                        {
                            RootFolders = new [] {"C:\\Test"}
                        }
                    }
                }
            };

            var serializer = new ProjectSerializer();
            using var stream1 = new MemoryStream();
            serializer.Save(project1, stream1);
            
            var data = stream1.ToArray();

            using var stream2 = new MemoryStream(data);
            var project2 = serializer.Load(stream2);

            Assert.AreEqual(project1.Checkers.BngCheckers.Length, project2.Checkers.BngCheckers.Length);
            Assert.AreEqual(project1.Checkers.BngCheckers[0].Address, project2.Checkers.BngCheckers[0].Address);
            Assert.AreEqual(project1.Checkers.ChatAvailableCheckers.Length, project2.Checkers.ChatAvailableCheckers.Length);
            Assert.AreEqual(project1.Checkers.ChatAvailableCheckers[0].Address, project2.Checkers.ChatAvailableCheckers[0].Address);

            Assert.AreEqual(project1.Events.TimeEvents.Length, project2.Events.TimeEvents.Length);
            Assert.AreEqual(project1.Events.TimeEvents[0].Schedule, project2.Events.TimeEvents[0].Schedule);

            Assert.AreEqual(project1.Jobs.EncryptFilesJobs.Length, project2.Jobs.EncryptFilesJobs.Length);
            Assert.AreEqual(project1.Jobs.EncryptFilesJobs[0].Password, project2.Jobs.EncryptFilesJobs[0].Password);
            Assert.AreEqual(project1.Jobs.EncryptFilesJobs[0].Options.DestFolder, project2.Jobs.EncryptFilesJobs[0].Options.DestFolder);
            Assert.AreEqual(project1.Jobs.SyncFilesJobs.Length, project2.Jobs.SyncFilesJobs.Length);
            Assert.AreEqual(project1.Jobs.SyncFilesJobs[0].RootFolders[0], project2.Jobs.SyncFilesJobs[0].RootFolders[0]);
        }
    }
}
