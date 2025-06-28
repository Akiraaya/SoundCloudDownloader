using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SCDL.Models;

namespace SCDL.Interfaces
{
    public interface ITrackResolver
    {
        Task<TrackInfo> ResolveTrackAsync(string trackUrl);
    }
}
