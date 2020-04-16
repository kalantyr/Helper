using System;
using System.Windows;
using LibGit2Sharp;

namespace Helper.Windows
{
    public partial class GitCredentialsWindow
    {
        public Credentials Credentials { get; private set; }

        public GitCredentialsWindow()
        {
            InitializeComponent();
        }

        public GitCredentialsWindow(string title): this()
        {
            _tbText.Text = title;
        }

        private void OnOkClick(object sender, RoutedEventArgs e)
        {
            try
            {
                Credentials = new UsernamePasswordCredentials
                {
                    Username = _tbUsername.Text,
                    Password = _pbPassword.Password
                };
                DialogResult = true;
            }
            catch (Exception exception)
            {
                App.ShowError(exception);
            }
        }
    }
}
