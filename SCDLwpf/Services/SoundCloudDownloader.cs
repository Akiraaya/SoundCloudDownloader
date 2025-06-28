using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SCDL.Interfaces;
using SCDL.Models;
using System.Net.Http;
using System.IO;

namespace SCDL.Services
{
    public class SoundCloudDownloader : IAudioDownloader
    {
        public async Task<string> DownloadTrackAsync(TrackInfo track, string path, Action<string> reportProgress)
        {
            string sanitizedTitle = SanitizeFileName(track.Title);
            string sanitizedArtist = SanitizeFileName(track.Artist);

            string fileName = $"{sanitizedArtist} - {sanitizedTitle}.mp3";
            string fullPath = System.IO.Path.Combine(path, fileName);

            using HttpClient client = new HttpClient();
            reportProgress("Starting downloading...");

            try
            {
                using var stream = await client.GetStreamAsync(track.StreamUrl);
                using var file = File.Create(fullPath);
                await stream.CopyToAsync(file);

                reportProgress($"Song: {track.Title} successfully stored into {fullPath}");
                return fullPath;
            }
            catch (Exception ex)
            {
                reportProgress($"Error while downloading: {ex.Message}");
                throw;
            }
        }

        private string SanitizeFileName(string name)
        {
            if (string.IsNullOrEmpty(name))
                return "Unknown";

            foreach (var c in Path.GetInvalidFileNameChars())
                name = name.Replace(c, '_');

            foreach (var c in Path.GetInvalidPathChars())
                name = name.Replace(c, '_');

            if (name.Length > 100)
                name = name.Substring(0, 100);

            return name.Trim();
        }
    }
}
