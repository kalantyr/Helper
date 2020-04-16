using System;
using System.IO;
using System.Windows;
using Helper.Utils;
using Microsoft.Win32;

namespace Helper
{
    public partial class MainWindow
    {
        private Project Project => ((App)Application.Current).Project;

        public MainWindow()
        {
            InitializeComponent();

            ((App)Application.Current).ProjectChanged += OnProjectChanged;

            Activated += (sender, e) => WindowsUtils.StopFlash(this);

            Loaded += (sender, e) =>
            {
                if (File.Exists(Settings.Default.LastProjectFile))
                    Load(Settings.Default.LastProjectFile);
            };
        }

        private void OnProjectChanged(object sender, EventArgs e)
        {
            _checkers.Project = Project;
            _jobs.Project = Project;
        }

        private void OnExitClick(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void OnLoadClick(object sender, RoutedEventArgs e)
        {
            var fileDialog = new OpenFileDialog();
            TuneFileDialog(fileDialog);
            if (fileDialog.ShowDialog() == true)
                Load(fileDialog.FileName);
        }

        private void Load(string fileName)
        {
            using var file = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.Read);
                ((App) Application.Current).LoadProject(file);
        }

        private void OnSaveClick(object sender, RoutedEventArgs e)
        {
            var fileDialog = new SaveFileDialog();
            TuneFileDialog(fileDialog);
            if (fileDialog.ShowDialog() == true)
            {
                using (var file = new FileStream(fileDialog.FileName, FileMode.Create, FileAccess.Write, FileShare.None))
                    ((App)Application.Current).SaveProject(file);

                Settings.Default.LastProjectFile = fileDialog.FileName;
                Settings.Default.Save();
            }
        }

        private static void TuneFileDialog(FileDialog fileDialog)
        {
            fileDialog.DefaultExt = ".helper.json";
            fileDialog.Filter = "Helper-files|*.helper.json|All files|*.*";

            if (File.Exists(Settings.Default.LastProjectFile))
                fileDialog.FileName = Settings.Default.LastProjectFile;
        }

        private void OnNewClick(object sender, RoutedEventArgs e)
        {
            ((App)Application.Current).NewProject();
        }
    }
}
