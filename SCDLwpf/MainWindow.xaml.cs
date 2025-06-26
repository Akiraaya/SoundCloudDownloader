using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using SCDLwpf.Models;
using SCDLwpf.Services;
using TagLib;
using Microsoft.Win32;
using System.Data;
using System.Windows.Media.Animation;
using System.Diagnostics;
using SCDLwpf.Localization;
using SCDLwpf.Properties;
using System.Windows.Threading;


namespace SCDLwpf;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    private readonly Download _download = new Download();
    private SoundCloud _downloader;

    OpenFolderDialog dialog = new();
    public MainWindow()
    {
        InitializeComponent();
        InitializeSCDownloader();
    }

    private void InitializeSCDownloader()
    {
        var savedPath = Properties.Settings.Default.DownloadPath;

        if (string.IsNullOrEmpty(savedPath))
        {
            savedPath = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyMusic), "SoundCloud Downloads");
        }

        if (!Directory.Exists(savedPath))
        {
            Directory.CreateDirectory(savedPath);
        }

        CreateDownloader();
        _download.Path = savedPath;

        UpdateStatus($"Download Path: {_download.Path}");
    }

    private void CreateDownloader()
    {
        var resolver = new SoundCloudUrlResolve(_download);
        var audioDownloader = new SoundCloudDownloader();
        var metadataWriter = new SoundCloudMetadataWriter();

        _downloader = new SoundCloud(resolver, audioDownloader, metadataWriter, _download.Path);
    }
    private void UpdateStatus(string message)
    {
        if (statusRichTextBox.Dispatcher.CheckAccess())
        {
            statusRichTextBox.AppendText($"[{DateTime.Now:HH:mm:ss}] {message}\n");
            statusRichTextBox.ScrollToEnd();
        }
        else
        {
            statusRichTextBox.Dispatcher.Invoke(() =>
            {
                statusRichTextBox.AppendText($"[{DateTime.Now:HH:mm:ss}] {message}\n");
                statusRichTextBox.ScrollToEnd();
            });
        }

    }

    private void LanguageComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (LanguageComboBox.SelectedItem is ComboBoxItem selectedItem)
        {
            string cultureCode = selectedItem.Tag.ToString();

            LocalizationManager.Instance.SetCulture(cultureCode);
            Properties.Settings.Default.Language = cultureCode;
            Properties.Settings.Default.Save();
            switch (cultureCode)
            {
                case "en-US":
                    downloadBtn.FontSize = 15;
                    pathBtn.FontSize = 15;
                    break;
                case "uk-UA":
                    downloadBtn.FontSize = 12;
                    pathBtn.FontSize = 12;
                    break;
                case "ru-RU":
                    downloadBtn.FontSize = 15;
                    pathBtn.FontSize = 12;
                    break;
            }
            Dispatcher.InvokeAsync(() =>
            {
                AnimateText(SongNameTextBlock, SongNameTextBlockCanvas);
                AnimateText(ArtistNameTextBlock, ArtistNameTextBlockCanvas);
            }, DispatcherPriority.Loaded);

            UpdateStatus($"Language changed to {selectedItem.Content}");
        }
    }
    private async void downloadBtn_Click(object sender, RoutedEventArgs e)
    {
        string url = linkTextBox.Text.Trim();

        if (string.IsNullOrWhiteSpace(url))
        {
            MessageBox.Show("Enter correct SoundCloud link.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            return;
        }

        if (string.IsNullOrEmpty(_download.Path))
        {
            MessageBox.Show("Choose folder.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            return;
        }

        if (clientIdCheckBox.IsChecked == true)
        {
            _download.ClientId = clientIdTextBox.Text.Trim();
            if (string.IsNullOrEmpty(_download.ClientId))
            {
                MessageBox.Show("Enter Client ID.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
        }
        else
        {
            _download.ClientId = "3sN94fvc9AjpzCe1QvVlD3mFwKfucCeC";
        }

        SoundCloudUrlResolve soundCloudUrlResolve = new SoundCloudUrlResolve(_download);
        TrackInfo track;

        try
        {
            UpdateStatus("Retrieving song information...");
            track = await soundCloudUrlResolve.ResolveTrackAsync(url);

            if (track == null)
            {
                UpdateStatus("Failed to retrieve song information.");
                return;
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error while receiving song: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            UpdateStatus($"Error: {ex.Message}");
            return;
        }

        CreateDownloader();

        downloadBtn.IsEnabled = false;
        UpdateStatus("Starting downloading...");

        try
        {
            await _downloader.DownloadTrackAsync(url, UpdateStatus);
            UpdateStatus("Download completed successfully!");

            if (!string.IsNullOrWhiteSpace(track.ArtworkUrl))
            {
                SongCover_Player.Source = new BitmapImage(new Uri(track.ArtworkUrl, UriKind.Absolute));
            }

            SongNameTextBlock.Text = track.Title ?? "Unknown title";
            ArtistNameTextBlock.Text = track.Artist ?? "Unknown artist";
            AlbumNameTextBlock.Text = track.Album ?? "Unknown album";
            GenreTextBlock.Text = track.Genre ?? "Unknown genre";
            YearTextBlock.Text = track.Year > 0 ? track.Year.ToString() : "Unknown year";

            AnimateText(SongNameTextBlock, SongNameTextBlockCanvas);
            AnimateText(ArtistNameTextBlock, ArtistNameTextBlockCanvas);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"An error occurred while loading: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            UpdateStatus($"Error: {ex.Message}");
        }
        finally
        {
            downloadBtn.IsEnabled = true;
        }
    }

    private void clientIdCheckBox_CheckedChanged(object sender, EventArgs e)
    {
        clientIdTextBox.IsEnabled = clientIdCheckBox.IsChecked == true;
        if (!clientIdCheckBox.IsChecked != true)
        {
            clientIdTextBox.Clear();
            WatermarkTextClientIdTextBox.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#a9a9a9"));
        }
        else
        {
            WatermarkTextClientIdTextBox.Foreground = Brushes.Gray;
        }
    }
    private void WatermarkTextLinkTextBox_TextChanged(object sender, TextChangedEventArgs e)
    {
        WatermarkTextLinkTextBox.Visibility = string.IsNullOrEmpty(linkTextBox.Text)
            ? Visibility.Visible
            : Visibility.Collapsed;
    }
    private void WatermarkTextClientIdTextBox_TextChanged(object sender, TextChangedEventArgs e)
    {
        WatermarkTextClientIdTextBox.Visibility = string.IsNullOrEmpty(clientIdTextBox.Text)
            ? Visibility.Visible
            : Visibility.Collapsed;
    }

    private void pathBtn_Click(object sender, RoutedEventArgs e)
    {
        dialog.Multiselect = false;
        dialog.Title = "Select a folder";
        dialog.FolderName = _download.Path;
        bool? result = dialog.ShowDialog();

        if (result == true)
        {
            _download.Path = dialog.FolderName;
            UpdateStatus($"New download folder selected: {_download.Path}");

            Properties.Settings.Default.DownloadPath = _download.Path;
            Properties.Settings.Default.Save();

            CreateDownloader();
        }
    }

    public void AnimateText(TextBlock textBlock, Canvas parentCanvas, double durationSeconds = 5)
    {
        if (textBlock == null || parentCanvas == null)
            return;

        textBlock.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
        double textWidth = textBlock.DesiredSize.Width;
        double canvasWidth = parentCanvas.ActualWidth;

        textBlock.BeginAnimation(Canvas.LeftProperty, null);
        Canvas.SetLeft(textBlock, 0);

        if (textWidth > canvasWidth)
        {
            double maxOffset = canvasWidth - textWidth;

            var animation = new DoubleAnimation
            {
                From = 0,
                To = maxOffset,
                Duration = new Duration(TimeSpan.FromSeconds(durationSeconds)),
                AutoReverse = true,
                RepeatBehavior = RepeatBehavior.Forever,
                EasingFunction = new SineEase { EasingMode = EasingMode.EaseInOut }
            };

            textBlock.BeginAnimation(Canvas.LeftProperty, animation);
        }
        else
        {
            Canvas.SetLeft(textBlock, 0);
        }
    }

    private void OpenFolderButton_Click(object sender, RoutedEventArgs e)
    {
        if (!string.IsNullOrEmpty(_download.Path) && Directory.Exists(_download.Path))
        {
            Process.Start("explorer.exe", _download.Path);
        }
        else
        {
            MessageBox.Show("Directory not found.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
}