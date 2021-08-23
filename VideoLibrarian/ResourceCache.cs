//--------------------------------------------------------------------------
// <summary>
//   
// </summary>
// <copyright file="ResourceCache.cs" company="Chuck Hill">
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
using System.Drawing;

namespace VideoLibrarian
{
    /// <summary>
    /// Commonly used resource cache to improve speed (albiet small). 
    /// This is used in tile *.designer.cs files because there are MANY 
    /// open tiles HOWEVER all instances of System.Drawing.Font() and 
    /// resources.GetObject() must be manually replaced because the 
    /// Winforms designer does not understand these optimizations.
    /// </summary>
    public static class ResourceCache
    {
        private static Bitmap _tileBackground = null;
        public static Bitmap TileBackground 
        {
            get
            {
                if (_tileBackground == null) _tileBackground = global::VideoLibrarian.Properties.Resources.DetailedTileBackground;
                return _tileBackground;
            }
        }

        private static Bitmap _imdbIcon = null;
        public static Bitmap ImdbIcon
        {
            get
            {
                if (_imdbIcon == null) _imdbIcon = global::VideoLibrarian.Properties.Resources.IMDB_Logo_2016;
                return _imdbIcon;
            }
        }

        private static Bitmap _imdbIconHover = null;
        public static Bitmap ImdbIconHover
        {
            get
            {
                if (_imdbIconHover == null) _imdbIconHover = global::VideoLibrarian.Properties.Resources.IMDB_Logo_2016_Hover;
                return _imdbIconHover;
            }
        }

        private static Bitmap _ratingsStar = null;
        public static Bitmap RatingsStar
        {
            get
            {
                if (_ratingsStar == null) _ratingsStar = global::VideoLibrarian.Properties.Resources.RatingsStar;
                return _ratingsStar;
            }
        }

        private static Bitmap _checkboxChecked = null;
        public static Bitmap CheckboxChecked
        {
            get
            {
                if (_checkboxChecked == null) _checkboxChecked = global::VideoLibrarian.Properties.Resources.CheckboxChecked;
                return _checkboxChecked;
            }
        }

        private static Bitmap _checkboxUnchecked = null;
        public static Bitmap CheckboxUnchecked
        {
            get
            {
                if (_checkboxUnchecked == null) _checkboxUnchecked = global::VideoLibrarian.Properties.Resources.CheckboxUnchecked;
                return _checkboxUnchecked;
            }
        }

        public static Font _fontRegular = null;
        public static Font FontRegular
        {
            get
            {
                if (_fontRegular == null) _fontRegular = new Font("Lucida Sans", 9F, FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
                return _fontRegular;
            }
        }

        public static Font _fontMedium = null;
        public static Font FontMedium
        {
            get
            {
                if (_fontMedium == null) _fontMedium = new Font("Lucida Sans", 12F, FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
                return _fontMedium;
            }
        }

        public static Font _fontBold = null;
        public static Font FontBold
        {
            get
            {
                if (_fontBold == null) _fontBold = new Font("Lucida Sans", 9F, FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 0);
                return _fontBold;
            }
        }

        public static Font _fontLargeBold = null;
        public static Font FontLargeBold
        {
            get
            {
                if (_fontLargeBold == null) _fontLargeBold = new Font("Lucida Sans", 14F, FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 0);
                return _fontLargeBold;
            }
        }
    }
}
