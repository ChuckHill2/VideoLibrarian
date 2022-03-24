//--------------------------------------------------------------------------
// <summary>
//   
// </summary>
// <copyright file="SortProperties.cs" company="Chuck Hill">
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
using System.Text;

namespace VideoLibrarian
{
    public enum SortDirection { Ascending, Descending }

    public class SortProperties
    {
        public List<SortKey> Keys { get; set; }

        public SortProperties()
        {
            this.Keys = new List<SortKey>(7)
            {
                new SortKey("MovieName","Movie Name", "Sort by name of movie", true, SortDirection.Ascending, k => k.MovieProps.MovieName),
                new SortKey("MovieClass","Video Type", "Sort by the type of movie", false, SortDirection.Ascending, k => k.MovieProps.MovieClass),
                new SortKey("ReleaseDate","Release Date", "Sort by release date", false, SortDirection.Ascending, k => k.MovieProps.ReleaseDate),
                new SortKey("MovieRating","Movie Rating", "Sort by user rating", false, SortDirection.Ascending, k => k.MovieProps.MovieRating),
                new SortKey("DownloadDate","Download Date", "Sort by date the video file was downloaded.", false, SortDirection.Ascending, k => k.MovieProps.DownloadDate),
                new SortKey("FolderName","Folder Name", "Sort by name of folder containing video file.", false, SortDirection.Ascending, k => k.MovieProps.FolderName),
                new SortKey("Watched","Watched", "Sort by date video was watched.", false, SortDirection.Ascending, k => k.MovieProps.Watched)
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
            IOrderedEnumerable<ITile> etiles = null;
            foreach(var key in Keys)
            {
                if (!key.Enabled) continue;
                if (etiles == null)
                {
                    if (key.Direction == SortDirection.Ascending)
                        etiles = tiles.OrderBy(key.Key);
                    else
                        etiles = tiles.OrderByDescending(key.Key);
                }
                else
                {
                    if (key.Direction == SortDirection.Ascending)
                        etiles = etiles.ThenBy(key.Key);
                    else
                        etiles = etiles.ThenByDescending(key.Key);
                }
            }
            if (etiles == null) return false;

            var eTilesArray = etiles.ToArray();
            var changed = !tiles.SequenceEqual(eTilesArray);
            Array.Copy(eTilesArray, tiles, tiles.Length);

            //Has Sort change the order of tiles?
            return changed;
        }

        public class SortKey
        {
            public string Name { get; set; } //Key name used for export
            public string FriendlyName { get; set; } //friendly name for UI
            public string Description { get; set; } //tool tip
            public bool Enabled { get; set; } //Is this sort key used?
            public SortDirection Direction { get; set; } 
            public Func<ITile, object> Key { get; set; } //linq OrderBy delegate

            public SortKey()
            {
                Name = "";
                FriendlyName = "";
                Description = "";
                Enabled = false;
                Direction = SortDirection.Ascending;
            }

            public SortKey(string name, string friendlyname, string description, bool enabled, SortDirection order, Func<ITile, object> key)
            {
                Name = name;
                FriendlyName = friendlyname.IsNullOrEmpty() ? name : friendlyname;
                Description = description;
                Enabled = enabled;
                Direction = order;
                Key = key; 
            }

            public SortKey Clone() => (SortKey)this.MemberwiseClone();

            public override string ToString() => string.Concat(Name, ",", Direction.ToString());
        }
    }
}
