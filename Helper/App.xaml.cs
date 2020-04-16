using System;
using System.IO;
using System.Reflection;

namespace Helper
{
    public partial class App
    {
        private static string _tempFolder;

        public Project Project { get; private set; }

        public App()
        {
            NewProject();
        }

        public void NewProject()
        {
            Project = new Project();
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

        public static string TempFolder
        {
            get
            {
                if (string.IsNullOrEmpty(_tempFolder))
                {
                    var fi = new FileInfo(Assembly.GetEntryAssembly().Location);
                    var tempFolder = Path.Combine(fi.DirectoryName, "temp");

                    tempFolder = @"C:\H_T";

                    if (!Directory.Exists(tempFolder))
                        Directory.CreateDirectory(tempFolder);
                    _tempFolder = tempFolder;
                }
                return _tempFolder;
            }
        }
    }
}
