using System;
using System.IO;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using TagLib;
using SCDL.Models;
using System.Runtime.InteropServices;
using SCDL.Services;
using SCDL.Interfaces;

namespace SCDL
{
    public class SoundCloud
    {
        private readonly ITrackResolver _resolver;
        private readonly IAudioDownloader _audioDownloader;
        private readonly IMetadataWriter _metadataWriter;
        private readonly string _path;

        public SoundCloud(ITrackResolver resolver, IAudioDownloader audioDownloader, IMetadataWriter metadataWriter, string path)
        {
            _resolver = resolver;
            _audioDownloader = audioDownloader;
            _metadataWriter = metadataWriter;
            _path = path;
        }

        public async Task DownloadTrackAsync(string trackUrl, Action<string> progress)
        {
            try
            {
                progress("Retrieving song information...");
                var track = await _resolver.ResolveTrackAsync(trackUrl);

                if (track == null)
                {
                    progress("Unable to retrieve track information. Please check the link and try again.");
                    return;
                }

                progress($"Song found: {track.Artist ?? "Unknown artist"} - {track.Title ?? "Unknown title"}");

                var fullFilePath = await _audioDownloader.DownloadTrackAsync(track, _path, progress);

                if (string.IsNullOrWhiteSpace(fullFilePath) || !System.IO.File.Exists(fullFilePath))
                {
                    progress("Error uploading file: The file was not created.");
                    return;
                }

                await _metadataWriter.WriteMetadataAsync(fullFilePath, track, progress);

                progress("Download complete");
            }
            catch (Exception ex)
            {
                progress($"Unknown error: {ex.Message}");
            }
        }
    }
}
