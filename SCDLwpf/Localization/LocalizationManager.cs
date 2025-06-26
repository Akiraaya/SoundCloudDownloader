using System.ComponentModel;
using System.Globalization;
using System.Threading;
using SCDLwpf.Properties;

namespace SCDLwpf.Localization
{
    public class LocalizationManager : INotifyPropertyChanged
    {
        private static LocalizationManager _instance;

        public static LocalizationManager Instance
        {
            get { return _instance ??= new LocalizationManager(); }
        }

        private LocalizationManager() { }

        public void SetCulture(string cultureCode)
        {
            var culture = new CultureInfo(cultureCode);
            Thread.CurrentThread.CurrentCulture = culture;
            Thread.CurrentThread.CurrentUICulture = culture;

            OnPropertyChanged(string.Empty);
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public string AppTitle => Resources.AppTitle;
        public string SongURL => Resources.SongURL;
        public string ClientID => Resources.ClientID;
        public string UseCustomClientID => Resources.UseCustomClientID;
        public string Download => Resources.Download;
        public string SelectPath => Resources.SelectPath;
        public string Status => Resources.Status;
        public string SongName => Resources.SongName;
        public string Artist => Resources.Artist;
        public string Album => Resources.Album;
        public string Genre => Resources.Genre;
        public string Year => Resources.Year;
        public string Language => Resources.Language;


        public string Status_DownloadPath => Resources.Status_DownloadPath;
    }
}