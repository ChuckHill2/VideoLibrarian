using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VideoLibrarian
{
    /// <summary>
    /// Commonly used resource cache to improve speed (albiet small).
    /// This is used in the tile *.designer.cs because there are MANY open tiles.
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
