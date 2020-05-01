using System;
using System.IO;
using System.Windows;

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


        public static void ShowError(Exception error)
        {
            MessageBox.Show(error.Message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
        }

        public static Window GetWindow(FrameworkElement userControl)
        {
            if (userControl == null) throw new ArgumentNullException(nameof(userControl));

            if (userControl is Window w2)
                return w2;

            var uc = userControl;
            var w = uc.Parent as Window;

            while (w == null)
            {
                uc = (FrameworkElement)uc.Parent;

                if (uc == null)
                    return App.Current.MainWindow;

                w = uc.Parent as Window;
            }

            return w;
        }


        public EventHandler ProjectChanged;

        public static string TempFolder
        {
            get
            {
                if (string.IsNullOrEmpty(_tempFolder))
                {
                    //var fi = new FileInfo(Assembly.GetEntryAssembly().Location);
                    //var tempFolder = Path.Combine(fi.DirectoryName, "temp");

                    var tempFolder = @"C:\H_T";

                    if (!Directory.Exists(tempFolder))
                        Directory.CreateDirectory(tempFolder);
                    _tempFolder = tempFolder;
                }
                return _tempFolder;
            }
        }
    }
}
