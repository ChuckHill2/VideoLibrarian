//--------------------------------------------------------------------------
// <summary>
//   
// </summary>
// <copyright file="FilterProperties.cs" company="Chuck Hill">
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
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace VideoLibrarian
{
    public enum ContainsLocation //where to look for substring
    {
        Anywhere,
        Title,
        Plot,  //+summary
        Crew
        //CustomGroups //user-defined attributes (not part of IMDB) 
    }

    [XmlInclude(typeof(ContainsLocation))]
    public class FilterProperties
    {
        public const string CustomGroup_Any = "Any"; //Special reserved CustomGroup siginifying that filtering is disabled. Equivalant to empty string.

        //All possible values
        [XmlIgnore] public static FilterValue[] AvailableGenres { get; private set; }
        [XmlIgnore] public static FilterValue[] AvailableClasses { get; private set; }
        [XmlIgnore] public static FilterValue[] AvailableGroups { get; private set; }
        [XmlIgnore] public static int MinYear { get; private set; }
        [XmlIgnore] public static int MaxYear { get; private set; }
        [XmlIgnore] public static int MinRating { get; private set; }
        [XmlIgnore] public static int MaxRating { get; private set; }
        [XmlIgnore] public static bool HasWatched { get; private set; }

        //Current Values
        public string ContainsSubstring { get; set; }
        public ContainsLocation ContainsLocation { get; set; }
        private string __CustomGroup = "";
        public string CustomGroup
        {
            get => __CustomGroup;
            set
            {
                if (value == CustomGroup_Any) value = string.Empty;
                __CustomGroup = value;
            }
        }
        public FilterValue[] Genres { get; set; }
        public FilterValue[] Classes { get; set; }
        public int StartYear { get; set; }
        public int EndYear { get; set; }
        public int Rating { get; set; }
        public bool IncludeUnrated { get; set; }
        public bool? Watched { get; set; } //both==null
        public bool FilteringDisabled { get; set; }

        public FilterProperties()
        {
            Genres = new FilterValue[0];
            Classes = new FilterValue[0];
            StartYear = 1900;
            EndYear = DateTime.Now.Year;
            this.Rating = 1;
            IncludeUnrated = true;
            Watched = null;
            FilteringDisabled = true;
        }

        /// <summary>
        /// Initialize static all possible values. 
        /// </summary>
        /// <param name="mp"></param>
        public static void InitAvailableValues(IList<MovieProperties> mp)
        {
            Func<float, int> Floor = (v) => (int)v;
            Func<float, int> Ceil = (v) => { var x = (int)(v * 10); return x / 10 + (x % 10 > 0 ? 1 : 0); };

            var genres = new Dictionary<string, int>();
            var classes = new Dictionary<string, int>();
            var groups = new Dictionary<string, int>();

            MinYear = 9999;
            MaxYear = 0;
            MinRating = 9999;
            MaxRating = 0;
            HasWatched = false;

            foreach (var m in mp)
            {
                int k;
                foreach (var g in m.Genre)
                {
                    if (genres.TryGetValue(g, out k)) genres[g] = k + 1;
                    else genres.Add(g, 1);
                }

                foreach (var g in m.CustomGroups)
                {
                    if (groups.TryGetValue(g, out k)) groups[g] = k + 1;
                    else groups.Add(g, 1);
                }

                if (classes.TryGetValue(m.MovieClass, out k)) classes[m.MovieClass] = k + 1;
                else classes.Add(m.MovieClass, 1);

                if (m.Year > MaxYear) MaxYear = m.Year;
                if (m.Year < MinYear) MinYear = m.Year;
                int r = Ceil(m.MovieRating);
                if (r > MaxRating) MaxRating = r;
                r = Floor(m.MovieRating);
                if (r < MinRating) MinRating = r;
                if (m.Watched!=DateTime.MinValue) HasWatched = true;
            }

            AvailableGenres = genres.Select(m => new FilterValue(m.Key, m.Value)).OrderBy(m => m.Name, StringComparer.OrdinalIgnoreCase).ToArray();
            AvailableClasses = classes.Select(m => new FilterValue(m.Key, m.Value)).OrderBy(m => m.Name, StringComparer.OrdinalIgnoreCase).ToArray();
            AvailableGroups = groups.Where(m =>!m.Key.IsNullOrEmpty() && !m.Key.EqualsI(CustomGroup_Any)).Select(m => new FilterValue(m.Key, m.Value)).OrderBy(m => m.Name, StringComparer.OrdinalIgnoreCase).Prepend(new FilterValue(CustomGroup_Any, 0)).ToArray();
        }

        /// <summary>
        /// Sets tile visibility flag based upon these filter properties
        /// </summary>
        /// <param name="tiles">array of tile's</param>
        /// <returns>True if changed</returns>
        public bool Filter(ITile[] tiles)
        {
            bool changed = false;

            if (FilteringDisabled)
            {
                foreach(var tile in tiles)
                {
                    if (!tile.IsVisible) changed = true;
                    tile.IsVisible = true;
                }
                return changed;
            }

            Func<MovieProperties, bool> hasSubstring = (m) => true;
            if (!string.IsNullOrWhiteSpace(this.ContainsSubstring))
            {
                switch (this.ContainsLocation)
                {
                    case ContainsLocation.Anywhere:
                        hasSubstring = (m) => 
                            m.MovieName.ContainsI(this.ContainsSubstring) ||
                            m.Plot.ContainsI(this.ContainsSubstring) ||
                            m.Summary.ContainsI(this.ContainsSubstring) ||
                            m.Cast.ContainsI(this.ContainsSubstring) ||
                            m.Creators.ContainsI(this.ContainsSubstring) ||
                            m.Directors.ContainsI(this.ContainsSubstring);
                        break;
                    case ContainsLocation.Title:
                        hasSubstring = (m) => m.MovieName.ContainsI(this.ContainsSubstring);
                        break;
                    case ContainsLocation.Plot:
                        hasSubstring = (m) => m.Plot.ContainsI(this.ContainsSubstring) ||
                            m.Summary.ContainsI(this.ContainsSubstring);
                        break;
                    case ContainsLocation.Crew:
                        hasSubstring = (m) => m.Cast.ContainsI(this.ContainsSubstring) ||
                            m.Creators.ContainsI(this.ContainsSubstring) ||
                            m.Directors.ContainsI(this.ContainsSubstring);
                        break;
                    //case ContainsLocation.CustomGroups:
                    //    hasSubstring = (m) => m.CustomGroups.ContainsI(this.ContainsSubstring);
                    //    break;
                }
            }

            Func<MovieProperties, bool> hasCustomGroup = (m) => true;
            if (CustomGroup != CustomGroup_Any && CustomGroup != string.Empty)
            {
                hasCustomGroup = (m) => m.CustomGroups.Length == 0 ? false : m.CustomGroups.Contains(CustomGroup, StringComparer.OrdinalIgnoreCase);
            }

            Parallel.ForEach(tiles, tile =>
            {
                var hasGenre = tile.MovieProps.Genre.Any(s => this.Genres.Any(t => t.Name == s));
                var hasClass = this.Classes.Any(t => t.Name == tile.MovieProps.MovieClass);
                var hasYear = tile.MovieProps.Year >= StartYear && tile.MovieProps.Year <= EndYear;

                bool hasRating;
                if (Rating <= -MinRating) hasRating = tile.MovieProps.MovieRating <= -Rating;
                else hasRating = tile.MovieProps.MovieRating >= Rating || tile.MovieProps.MovieRating == 0 || IncludeUnrated;
                var hasWatched = Watched == null || Watched == (tile.MovieProps.Watched != DateTime.MinValue);

                bool ch = tile.IsVisible;
                tile.IsVisible = hasGenre && hasClass && hasYear && hasRating && hasWatched && hasCustomGroup(tile.MovieProps) && hasSubstring(tile.MovieProps);
                if (ch != tile.IsVisible) changed = true;
            });

            return changed;
        }

        [XmlRoot("Value")]
        public class FilterValue
        {
            private string _friendlyName;
            private string _name;
            private int _count;

            [XmlIgnore] 
            public string FriendlyName
            {
                get
                {
                    if (_friendlyName == null)
                    {
                        if (Count==0) _friendlyName = Name ?? "null";
                        else _friendlyName = string.Concat(Name ?? "null", " (", Count.ToString(), ")");
                    }
                    return _friendlyName;
                }
            }

            [XmlAttribute("Name")] 
            public string Name { get { return _name; } set { _name = value; } }

            [XmlIgnore] 
            public int Count { get { return _count; } set { _count = value; } }

            public FilterValue() { } //parameterless constructor for xml serialization

            public FilterValue(string name, int count) 
            { 
                _friendlyName = null; 
                _name = name; 
                _count = count; 
            }
        }
    }
}
