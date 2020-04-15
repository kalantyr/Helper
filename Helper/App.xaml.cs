using System;
using System.IO;
using Helper.Checkers;

namespace Helper
{
    public partial class App
    {
        public Project Project { get; private set; }

        public App()
        {
            NewProject();
        }

        public void NewProject()
        {
            Project = new Project
            {
                Checkers = new AllCheckers
                {
                    ChatAvailableChecker = new ChatAvailableChecker[0]
                }
            };
            ProjectChanged?.Invoke(this, EventArgs.Empty);
        }

        public void LoadProject(Stream stream)
        {
            Project = Project.Load(stream);
            ProjectChanged?.Invoke(this, EventArgs.Empty);
        }

        public void SaveProject(Stream stream)
        {
            Project.Save(stream);
        }

        public EventHandler ProjectChanged;
    }
}
