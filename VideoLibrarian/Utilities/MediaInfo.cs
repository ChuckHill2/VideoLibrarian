//--------------------------------------------------------------------------
// <summary>
//   
// </summary>
// <copyright file="MediaInfo.cs" company="Chuck Hill">
// Copyright (c) 2020 Chuck Hill.
//
// This library is free software; you can redistribute it and/or
// modify it under the terms of the GNU Lesser General Public License
// as published by the Free Software Foundation; either version 2.1
// of the License, or (at your option) any later version.
//
// This library is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Lesser General Public License for more details.
//
// The GNU Lesser General Public License can be viewed at
// http://www.opensource.org/licenses/lgpl-license.php. If
// you unfamiliar with this license or have questions about
// it, here is an http://www.gnu.org/licenses/gpl-faq.html.
//
// All code and executables are provided "as is" with no warranty
// either express or implied. The author accepts no liability for
// any damage or loss of business that this product may cause.
// </copyright>
// <repository>https://github.com/ChuckHill2/VideoLibrarian</repository>
// <author>Chuck Hill</author>
//--------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

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
