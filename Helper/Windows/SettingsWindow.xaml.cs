using System;
using System.Windows;

namespace Helper.Windows
{
    public partial class SettingsWindow
    {
        public SettingsWindow()
        {
            InitializeComponent();

            _pwdBox.Password = Settings.Default.Password;
        }

        private void ButtonBase_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                Settings.Default.Password = _pwdBox.Password;
                Settings.Default.Save();
                MessageBox.Show("Done");
            }
            catch (Exception exception)
            {
                App.ShowError(exception);
            }
        }
    }
}
