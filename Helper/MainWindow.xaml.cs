﻿using System;
using System.IO;
using System.Threading;
using System.Windows;
using Helper.Core;
using Helper.Tools;
using Helper.Utils;
using Helper.Windows;
using Microsoft.Win32;
using Size = System.Windows.Size;
using Point = System.Drawing.Point;

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

            if (Settings.Default.MainWindowPos != Point.Empty)
            {
                if (Math.Abs(Settings.Default.MainWindowPos.X) > 10000 || Math.Abs(Settings.Default.MainWindowPos.Y) > 10000)
                    Settings.Default.MainWindowPos = new Point();

                Left = Settings.Default.MainWindowPos.X;
                Top = Settings.Default.MainWindowPos.Y;
            }

            ((App)Application.Current).ProjectChanged += OnProjectChanged;

            Activated += (sender, e) => WindowsUtils.StopFlash(this);

            Loaded += (sender, e) =>
            {
                if (File.Exists(Settings.Default.LastProjectFile))
                    Load(Settings.Default.LastProjectFile);
            };

            SizeChanged += MainWindow_SizeChanged;
            LocationChanged += MainWindow_LocationChanged;
        }

        private void MainWindow_LocationChanged(object sender, EventArgs e)
        {
            if (IsInitialized)
            {
                Settings.Default.MainWindowPos = new Point((int)Left, (int)Top);
                Settings.Default.Save();
            }
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
            _events.Project = Project;
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
            ((App)Application.Current).LoadProject(file);

            Settings.Default.LastProjectFile = fileName;
            Settings.Default.Save();
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

        private void OnSettingsClick(object sender, RoutedEventArgs e)
        {
            var window = new SettingsWindow { Owner = this };
            window.ShowDialog();
        }
    }
}
