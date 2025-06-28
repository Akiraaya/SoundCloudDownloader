using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SCDL.Models;

namespace SCDL.Interfaces
{
    public interface IMetadataWriter
    {
        Task WriteMetadataAsync(string filePath, TrackInfo track, Action<string> reportProgress);
    }
}
