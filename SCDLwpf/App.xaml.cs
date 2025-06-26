using System.Configuration;
using System.Data;
using System.Windows;
using System.Globalization;
using System.Threading;
using SCDLwpf.Properties;
using SCDLwpf.Localization;


namespace SCDLwpf;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        var savedLanguage = Settings.Default.Language;
        if (!string.IsNullOrEmpty(savedLanguage))
        {
            LocalizationManager.Instance.SetCulture(savedLanguage);
        }

    }

    protected override void OnExit(ExitEventArgs e)
    {
        Settings.Default.Language = Thread.CurrentThread.CurrentUICulture.Name;
        Settings.Default.Save();

        base.OnExit(e);
    }
}

