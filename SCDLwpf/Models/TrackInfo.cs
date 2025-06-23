using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SCDLwpf.Models
{
    public class TrackInfo
    {
        public string Title { get; set; } = "";
        public string Artist { get; set; } = "";
        public string? Album { get; set; }
        public string? Genre { get; set; }
        public int Year { get; set; } = 0;


        public string? ArtworkUrl { get; set; }
        public string StreamUrl { get; set; }
    }

}
