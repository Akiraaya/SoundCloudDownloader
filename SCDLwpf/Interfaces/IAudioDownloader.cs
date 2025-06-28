using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SCDL.Models;

namespace SCDL.Interfaces
{
    public interface IAudioDownloader
    {
        Task<string> DownloadTrackAsync(TrackInfo track,string path, Action<string> reportProgress);
    }
}
