using Helper.Jobs;

namespace Helper
{
    public class SettingsImpl: ISettings
    {
        public string Password => Settings.Default.Password;
    }
}
