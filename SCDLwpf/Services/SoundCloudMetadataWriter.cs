using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TagLib;
using SCDL.Models;
using SCDL.Interfaces;
using System.Net.Http;

namespace SCDL.Services
{
    public class SoundCloudMetadataWriter : IMetadataWriter
    {
        public async Task WriteMetadataAsync(string filePath, TrackInfo track, Action<string> reportProgress)
        {
            if (track.ArtworkUrl == null)
            {
                reportProgress("Cover not found");
                return;
            }
            

            try
            {
                using var file = TagLib.File.Create(filePath);
                file.Tag.Title = track.Title;
                file.Tag.Performers = new[] { track.Artist ?? "Unknown Artist" };
                file.Tag.AlbumArtists = new[] { track.Artist ?? "Unknown Artist" };
                file.Tag.Album = track.Album ?? track.Title;
                file.Tag.Genres = new[] { track.Genre ?? "Unknown Genre" };
                file.Tag.Year = (uint)track.Year;

                file.Tag.Pictures = new IPicture[]
                {
                    new Picture(new ByteVector(await new HttpClient().GetByteArrayAsync(track.ArtworkUrl)))
                    {
                        Type = PictureType.FrontCover,
                        Description = "Cover",
                        MimeType = "image/jpeg"
                    }
                };
                file.Save();
                reportProgress("Metadata written successfully");
            }
            catch (Exception ex)
            {
                reportProgress($"Error writing metadata: {ex.Message}");
            }
        }
    }
}
