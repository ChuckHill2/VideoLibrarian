using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;

namespace MovieGuide
{
    public enum SortDirection { Ascending, Descending }

    public class SortProperties
    {
        public List<SortKey> Keys { get; set; }

        public SortProperties()
        {
            this.Keys = new List<SortKey>(7)
            {
                new SortKey("MovieName","Movie Name", "Sort by name of movie", true, SortDirection.Ascending, 
                    delegate(ITile x, ITile y)
                    {
                        int v = x.MovieProps.MovieName.CompareTo(y.MovieProps.MovieName);
                        if (v != 0) return v;
                        return x.MovieProps.GetHashCode() - y.MovieProps.GetHashCode();
                    }, 
                    delegate(ITile x, ITile y)
                    {
                        int v = y.MovieProps.MovieName.CompareTo(x.MovieProps.MovieName);
                        if (v != 0) return v;
                        return y.MovieProps.GetHashCode() - x.MovieProps.GetHashCode();
                    }),
                new SortKey("MovieClass","Video Type", "Sort by the type of movie", false, SortDirection.Ascending, 
                    delegate(ITile x, ITile y)
                    {
                        int v = x.MovieProps.MovieClass.CompareTo(y.MovieProps.MovieClass);
                        if (v != 0) return v;
                        return x.MovieProps.GetHashCode() - y.MovieProps.GetHashCode();
                    }, 
                    delegate(ITile x, ITile y)
                    {
                        int v = y.MovieProps.MovieClass.CompareTo(x.MovieProps.MovieClass);
                        if (v != 0) return v;
                        return y.MovieProps.GetHashCode() - x.MovieProps.GetHashCode();
                    }),
                new SortKey("ReleaseDate","Release Date", "Sort by release date", false, SortDirection.Ascending,
                    delegate(ITile x, ITile y)
                    {
                        int v = x.MovieProps.ReleaseDate.CompareTo(y.MovieProps.ReleaseDate);
                        if (v != 0) return v;
                        return x.MovieProps.GetHashCode() - y.MovieProps.GetHashCode();
                    },
                    delegate(ITile x, ITile y)
                    {
                        int v = y.MovieProps.ReleaseDate.CompareTo(x.MovieProps.ReleaseDate);
                        if (v != 0) return v;
                        return y.MovieProps.GetHashCode() - x.MovieProps.GetHashCode();
                    }),
                new SortKey("MovieRating","Movie Rating", "Sort by user rating", false, SortDirection.Ascending,
                    delegate(ITile x, ITile y)
                    {
                        int v = x.MovieProps.MovieRating.CompareTo(y.MovieProps.MovieRating);
                        if (v != 0) return v;
                        return x.MovieProps.GetHashCode() - y.MovieProps.GetHashCode();
                    },
                    delegate(ITile x, ITile y)
                    {
                        int v = y.MovieProps.MovieRating.CompareTo(x.MovieProps.MovieRating);
                        if (v != 0) return v;
                        return y.MovieProps.GetHashCode() - x.MovieProps.GetHashCode();
                    }),
                new SortKey("DownloadDate","Download Date", "Sort by date the video file was downloaded.", false, SortDirection.Ascending,
                    delegate(ITile x, ITile y)
                    { 
                        int v = x.MovieProps.DownloadDate.CompareTo(y.MovieProps.DownloadDate);
                        if (v != 0) return v;
                        return x.MovieProps.GetHashCode() - y.MovieProps.GetHashCode();
                    },
                    delegate(ITile x, ITile y)
                    {
                        int v = y.MovieProps.DownloadDate.CompareTo(x.MovieProps.DownloadDate);
                        if (v != 0) return v;
                        return y.MovieProps.GetHashCode() - x.MovieProps.GetHashCode();
                    }),
                new SortKey("FolderName","Folder Name", "Sort by name of folder containing video file.", false, SortDirection.Ascending, 
                    delegate(ITile x, ITile y)
                    { 
                        int v = x.MovieProps.FolderName.CompareTo(y.MovieProps.FolderName);
                        if (v != 0) return v;
                        return x.MovieProps.GetHashCode() - y.MovieProps.GetHashCode();
                    }, 
                    delegate(ITile x, ITile y)
                    { 
                        int v = y.MovieProps.FolderName.CompareTo(x.MovieProps.FolderName);
                        if (v != 0) return v;
                        return y.MovieProps.GetHashCode() - x.MovieProps.GetHashCode();
                    }),
                new SortKey("Watched","Watched", "Sort by date video was watched.", false, SortDirection.Ascending, 
                    delegate(ITile x, ITile y)
                    { 
                        int v = x.MovieProps.Watched.CompareTo(y.MovieProps.Watched);
                        if (v != 0) return v;
                        return x.MovieProps.GetHashCode() - y.MovieProps.GetHashCode();
                    }, 
                    delegate(ITile x, ITile y)
                    {
                        int v = y.MovieProps.Watched.CompareTo(x.MovieProps.Watched);
                        if (v != 0) return v;
                        return y.MovieProps.GetHashCode() - x.MovieProps.GetHashCode();
                    })
            };
        }

        public SortProperties(string props) : this()
        {
            if (props.IsNullOrEmpty()) return;
            var nuKeys = new List<SortKey>(7);
            foreach(var k in props.Split(new char[]{'|'},StringSplitOptions.RemoveEmptyEntries))
            {
                var pair = k.Trim().Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                var name = pair[0].Trim();
                var direction = pair[1].Trim();
                int i = this.Keys.IndexOf(sk => sk.Name.EqualsI(name));
                if (i<0) continue;
                var key = Keys[i];
                key.Enabled = true;
                SortDirection sd;
                if (Enum.TryParse<SortDirection>(direction, out sd)) key.Direction = sd;
                this.Keys.RemoveAt(i);
                nuKeys.Add(key);
            }
            foreach(var key in this.Keys)
            {
                key.Enabled = false;
                nuKeys.Add(key);
            }
            this.Keys = nuKeys;
        }

        public override string ToString()
        {
            var sb = new StringBuilder();
            foreach(var k in Keys)
            {
                if (!k.Enabled) continue;
                sb.Append(k.Name);
                sb.Append(',');
                sb.Append(k.Direction);
                sb.Append('|');
            }
            if (sb.Length == 0) sb.Append("MovieName,Ascending|"); //default
            sb.Length -= 1;
            return sb.ToString();
        }

        public SortProperties Clone()
        {
            SortProperties sp = (SortProperties)this.MemberwiseClone();
            for(int i=0; i< sp.Keys.Count; i++) sp.Keys[i] = sp.Keys[i].Clone();
            return sp;
        }

        /// <summary>
        /// Sort tiles
        /// </summary>
        /// <param name="tiles">array of tile's</param>
        /// <returns>True if changed</returns>
        public bool Sort(ITile[] tiles)
        {
            //Create short list of compare delegates
            var comparers = new List<Func<ITile, ITile, int>>(Keys.Count);
            for (int i = 0; i < Keys.Count; i++)
            {
                if (!Keys[i].Enabled) continue;
                if (Keys[i].Direction == SortDirection.Descending) comparers.Add(Keys[i].SortDescending);
                else comparers.Add(Keys[i].SortAscending);
            }
            int length = comparers.Count;
            if (length == 0) return false; //no sort keys were enabled.

            //Create comparer
            Comparison<ITile> comparer = (x, y) =>
            {
                for (int i = 0; i < length; i++)
                {
                    int result = comparers[i](x, y);
                    if (result != 0) return result;
                }
                return 0;
            };

            ITile[] dupeTiles = new ITile[tiles.Length];
            Array.Copy(tiles, dupeTiles, tiles.Length);

            //Now, sort in-place...
            Array.Sort<ITile>(tiles, Comparer<ITile>.Create(comparer));

            //Has the above Array.Sort change the order of tiles?
            bool changed = false;
            for (int i = 0; i < tiles.Length; i++)
            {
                if (tiles[i].MovieProps.GetHashCode() != dupeTiles[i].MovieProps.GetHashCode())
                {
                    changed = true;
                }
            }

           return changed;
        }

        public class SortKey
        {
            public string Name { get; set; }
            public string FriendlyName { get; set; }
            public string Description { get; set; }
            public bool Enabled { get; set; }
            public SortDirection Direction { get; set; }
            public Func<ITile, ITile, int> SortAscending { get; set; }
            public Func<ITile, ITile, int> SortDescending { get; set; }

            public SortKey()
            {
                Name = "";
                FriendlyName = "";
                Description = "";
                Enabled = false;
                Direction = SortDirection.Ascending;
            }

            public SortKey(string name, string friendlyname, string description, bool enabled, SortDirection order, Func<ITile, ITile, int> ascending, Func<ITile, ITile, int> descending)
            {
                Name = name;
                FriendlyName = friendlyname.IsNullOrEmpty() ? name : friendlyname;
                Description = description;
                Enabled = enabled;
                Direction = order;
                SortAscending = ascending;
                SortDescending = descending;
            }

            public SortKey Clone()
            {
                return (SortKey)this.MemberwiseClone();
            }

            public override string ToString()
            {
                return string.Concat(Name, ",", Direction.ToString());
            }
        }
    }
}
