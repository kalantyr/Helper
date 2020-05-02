using System;
using System.IO;
using System.Linq;
using System.Windows;
using Helper.Model;
using Helper.Tools;
using Microsoft.Win32;

namespace Helper.Windows
{
    public partial class RemoveUnusedProjectsToolWindow
    {
        private readonly RemoveUnusedProjectsTool _tool;
        private Solution _solution;

        public RemoveUnusedProjectsToolWindow()
        {
            InitializeComponent();
        }
        
        public RemoveUnusedProjectsToolWindow(RemoveUnusedProjectsTool tool): this()
        {
            _tool = tool ?? throw new ArgumentNullException(nameof(tool));
        }

        private void OnBrowseClick(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFileDialog { Filter = "sln-files|*.sln|All files|*.*" };
            if (dialog.ShowDialog() == true)
                _tbSolutionFile.Text = dialog.FileName;
        }

        private void OnOkClick(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!(_cbProject.SelectedItem is Model.Project selectedProject))
                    throw new Exception("Project not selected");

                _tool.Solution = _solution;
                _tool.RootProjectName = selectedProject.Name;

                DialogResult = true;
            }
            catch (Exception error)
            {
                App.ShowError(error);
            }
        }

        private void OnSolutionFileTextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            if (File.Exists(_tbSolutionFile.Text))
            {
                _solution = Solution.FromFile(_tbSolutionFile.Text);
                var projects = _solution.Projects;
                _cbProject.ItemsSource = projects.OrderBy(pr => pr.Name);
                _cbProject.SelectedItem = projects.FirstOrDefault();
            }
        }
    }
}
