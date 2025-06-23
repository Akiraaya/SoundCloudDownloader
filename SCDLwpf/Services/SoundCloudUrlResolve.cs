using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using SCDLwpf.Interfaces;
using SCDLwpf.Models;
using System.Net.Http;
using System.Windows;

namespace SCDLwpf.Services
{
    public class SoundCloudUrlResolve : ITrackResolver
    {
        private readonly Download _download;

        public SoundCloudUrlResolve(Download download)
        {
            _download = download;
        }

        public async Task<TrackInfo> ResolveTrackAsync(string trackUrl)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(trackUrl))
                {
                    MessageBox.Show("Please enter the correct SoundCloud link", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return null;
                }

                using HttpClient client = new HttpClient();

                string resolveUrl = $"https://api-v2.soundcloud.com/resolve?url={trackUrl}&client_id={_download.ClientId}";

                var response = await client.GetStringAsync(resolveUrl);
                if (string.IsNullOrWhiteSpace(response))
                {
                    MessageBox.Show("Unable to retrieve data from the specified link. Check the URL and try again.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return null;
                }

                var json = JsonDocument.Parse(response).RootElement;

                string title = json.TryGetProperty("title", out var titleProp) ? titleProp.GetString() ?? "Unknown title" : "Unknown title";
                string genre = json.TryGetProperty("genre", out var genreProp) ? genreProp.GetString() ?? "Unknown genre" : "Unknown genre";
                int year = json.TryGetProperty("created_at", out var dateProp) && DateTime.TryParse(dateProp.GetString(), out var dt) ? dt.Year : 0;

                string? artworkUrl = null;
                string? artist = null;
                string? album = null;

                if (json.TryGetProperty("publisher_metadata", out var pub) && pub.ValueKind == JsonValueKind.Object)
                {
                    if (pub.TryGetProperty("album_title", out var a)) album = a.GetString();
                    if (pub.TryGetProperty("artist", out var ar)) artist = ar.GetString();
                }

                if (string.IsNullOrWhiteSpace(artist) && json.TryGetProperty("user", out var user) && user.ValueKind == JsonValueKind.Object)
                {
                    if (user.TryGetProperty("username", out var fullName) && fullName.ValueKind == JsonValueKind.String)
                    {
                        artist = fullName.GetString();
                    }
                }

                if (json.TryGetProperty("artwork_url", out var art) && art.ValueKind == JsonValueKind.String)
                {
                    artworkUrl = art.GetString()?.Replace("-large", "-t500x500");
                }

                if (!json.TryGetProperty("media", out var media) || !media.TryGetProperty("transcodings", out var transcodings))
                {
                    MessageBox.Show("Не удалось найти информацию о потоках трека.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    return null;
                }

                string? progressiveUrl = null;
                foreach (var transcoding in transcodings.EnumerateArray())
                {
                    if (transcoding.TryGetProperty("format", out var format) &&
                        format.TryGetProperty("protocol", out var protocol) &&
                        protocol.GetString() == "progressive")
                    {
                        progressiveUrl = transcoding.GetProperty("url").GetString();
                        break;
                    }
                }

                if (string.IsNullOrWhiteSpace(progressiveUrl))
                {
                    MessageBox.Show("No progressive stream found for download.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return null;
                }

                string mp3InfoUrl = $"{progressiveUrl}?client_id={_download.ClientId}";
                var streamInfoJson = await client.GetStringAsync(mp3InfoUrl);
                var streamInfo = JsonDocument.Parse(streamInfoJson);
                string mp3Url = streamInfo.RootElement.TryGetProperty("url", out var urlProp) ? urlProp.GetString() ?? "" : "";

                if (string.IsNullOrWhiteSpace(mp3Url))
                {
                    MessageBox.Show("Failed to get MP3 stream link.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return null;
                }

                return new TrackInfo
                {
                    Title = title,
                    Artist = artist ?? "Unknown artist",
                    Album = album ?? "Unknown album",
                    Genre = genre,
                    Year = year,
                    ArtworkUrl = artworkUrl,
                    StreamUrl = mp3Url
                };
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Unknown error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return null;
            }
        }
    }
}
