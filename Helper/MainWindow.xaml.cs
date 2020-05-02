using System;
using System.IO;
using System.Threading;
using System.Windows;
using Helper.Tools;
using Helper.Utils;
using Helper.Windows;
using Microsoft.Win32;
using Size = System.Windows.Size;

namespace Helper
{
    public partial class MainWindow
    {
        private Project Project => ((App)Application.Current).Project;

        public MainWindow()
        {
            InitializeComponent();

            if (Settings.Default.MainWindowSize != Size.Empty)
            {
                Width = Settings.Default.MainWindowSize.Width;
                Height = Settings.Default.MainWindowSize.Height;
            }

            ((App)Application.Current).ProjectChanged += OnProjectChanged;

            Activated += (sender, e) => WindowsUtils.StopFlash(this);

            Loaded += (sender, e) =>
            {
                if (File.Exists(Settings.Default.LastProjectFile))
                    Load(Settings.Default.LastProjectFile);
            };

            SizeChanged += MainWindow_SizeChanged;
        }

        private void MainWindow_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (e.NewSize == Size.Empty)
                return;

            if (Settings.Default.MainWindowSize == e.NewSize)
                return;

            Settings.Default.MainWindowSize = e.NewSize;
            Settings.Default.Save();
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

        private async void OnRemoveProjectsClick(object sender, RoutedEventArgs e)
        {
            var tool = new RemoveUnusedProjectsTool();
            var window = new RemoveUnusedProjectsToolWindow(tool) { Owner = this };
            if (window.ShowDialog() == true)
                await tool.Run(CancellationToken.None);
        }
    }
}
