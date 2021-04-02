using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using System.Xml.Serialization;

namespace VideoLibrarian
{
    public enum ContainsLocation //where to look for substring
    {
        Anywhere,
        MovieName,
        Plot,
        Crew
    }

    [XmlInclude(typeof(ContainsLocation))]
    public class FilterProperties
    {
        //All possible values
        [XmlIgnore] public static FilterValue[] AvailableGenres { get; private set; }
        [XmlIgnore] public static FilterValue[] AvailableClasses { get; private set; }
        [XmlIgnore] public static int MinYear { get; private set; }
        [XmlIgnore] public static int MaxYear { get; private set; }
        [XmlIgnore] public static int MinRating { get; private set; }
        [XmlIgnore] public static int MaxRating { get; private set; }
        [XmlIgnore] public static bool HasWatched { get; private set; }

        //Current Values
        public string ContainsSubstring { get; set; }
        public ContainsLocation ContainsLocation { get; set; }
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

            Func<MovieProperties, bool> hasContainsValue = (m) => true;
            if (!string.IsNullOrWhiteSpace(this.ContainsSubstring))
            {
                switch (this.ContainsLocation)
                {
                    case ContainsLocation.Anywhere:
                        hasContainsValue = (m) => 
                            m.MovieName.ContainsI(this.ContainsSubstring) ||
                            m.Plot.ContainsI(this.ContainsSubstring) ||
                            m.Summary.ContainsI(this.ContainsSubstring) ||
                            m.Cast.ContainsI(this.ContainsSubstring) ||
                            m.Creators.ContainsI(this.ContainsSubstring) ||
                            m.Directors.ContainsI(this.ContainsSubstring);
                        break;
                    case ContainsLocation.MovieName:
                        hasContainsValue = (m) => m.MovieName.ContainsI(this.ContainsSubstring);
                        break;
                    case ContainsLocation.Plot:
                        hasContainsValue = (m) => m.Plot.ContainsI(this.ContainsSubstring) ||
                            m.Summary.ContainsI(this.ContainsSubstring);
                        break;
                    case ContainsLocation.Crew:
                        hasContainsValue = (m) => m.Cast.ContainsI(this.ContainsSubstring) ||
                            m.Creators.ContainsI(this.ContainsSubstring) ||
                            m.Directors.ContainsI(this.ContainsSubstring);
                        break;
                }
            }

            foreach (var tile in tiles)
            {
                var hasGenre = tile.MovieProps.Genre.Any(s => this.Genres.Any(t => t.Name == s));
                var hasClass = this.Classes.Any(t => t.Name == tile.MovieProps.MovieClass);
                var hasYear = tile.MovieProps.Year >= StartYear && tile.MovieProps.Year <= EndYear;
                var hasRating = tile.MovieProps.MovieRating >= Rating || (tile.MovieProps.MovieRating < 1 && IncludeUnrated);
                var hasWatched = Watched == null || Watched == (tile.MovieProps.Watched!=DateTime.MinValue);

                bool ch = tile.IsVisible;
                tile.IsVisible = hasGenre && hasClass && hasYear && hasRating && hasWatched && hasContainsValue(tile.MovieProps);
                if (ch != tile.IsVisible) changed = true;
            }

            return changed;
        }

        [XmlRoot("Value")]
        public struct FilterValue
        {
            private string _friendlyName;
            private string _name;
            private int _count;

            [XmlIgnore] 
            public string FriendlyName
            {
                get
                {
                    if (_friendlyName == null) _friendlyName = string.Concat(Name??"null"," (",Count.ToString(),")");
                    return _friendlyName;
                }
            }

            [XmlAttribute("Name")] 
            public string Name { get { return _name; } set { _name = value; } }

            [XmlIgnore] 
            public int Count { get { return _count; } set { _count = value; } }

            public FilterValue(string name, int count) 
            { 
                _friendlyName = null; 
                _name = name; 
                _count = count; 
            }
        }
    }
}
