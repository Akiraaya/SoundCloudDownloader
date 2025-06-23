using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SCDLwpf.Models;

namespace SCDLwpf.Interfaces
{
    public interface ITrackResolver
    {
        Task<TrackInfo> ResolveTrackAsync(string trackUrl);
    }
}
