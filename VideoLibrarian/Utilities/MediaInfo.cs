using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VideoLibrarian
{
    public class MediaInfo
    {
        private static readonly NReco.VideoInfo.FFProbe ffProbe = new NReco.VideoInfo.FFProbe() { ToolPath = Path.GetDirectoryName(Process.GetCurrentProcess().MainModule.FileName) };
        public static MediaInfo GetInfo(string filePath)
        {
            var info = ffProbe.GetMediaInfo(filePath); //May throw FFProbeException
            if (info == null) return null;
            return new MediaInfo()
            {
                Duration = info.Duration,
                FormatLongName = info.FormatLongName,
                FormatName = info.FormatName,
                FormatTags = info.FormatTags,
                Streams = info.Streams.Select(s => new MediaInfo.StreamInfo()
                {
                    CodecLongName = s.CodecLongName,
                    CodecName = s.CodecName,
                    CodecType = s.CodecType,
                    FrameRate = s.FrameRate,
                    Height = s.Height,
                    Index = s.Index,
                    PixelFormat = s.PixelFormat,
                    Tags = s.Tags,
                    Width = s.Width,
                }).ToArray()
            };
        }

        private MediaInfo() { }

        public TimeSpan Duration { get; private set; }
        public string FormatLongName { get; private set; }
        public string FormatName { get; private set; }
        public KeyValuePair<string, string>[] FormatTags { get; private set; }
        public MediaInfo.StreamInfo[] Streams { get; private set; }

        public class StreamInfo
        {
            public string CodecLongName { get; set; }
            public string CodecName { get; set; }
            public string CodecType { get; set; }
            public float FrameRate { get; set; }
            public int Height { get; set; }
            public string Index { get; set; }
            public string PixelFormat { get; set; }
            public KeyValuePair<string, string>[] Tags { get; set; }
            public int Width { get; set; }
        }
    }
}
