using System.Diagnostics.Contracts;
using System.Runtime.InteropServices;
using System.Text;
using System.Globalization;
using System;
using System.Xml.Serialization;


namespace VideoLibrarian
{
    /// <summary>
    /// class System.Version cannot be serialized because its properties are readonly!
    /// This is serializable and implicitly castable to/from System.Version.
    /// </summary>
    [Serializable]
    [XmlInclude(typeof(Version))] //necessary when using implicit operators
    public sealed class VersionRW : ICloneable, IComparable, IComparable<VersionRW>, IComparable<Version>, IEquatable<VersionRW>, IEquatable<Version>
    {
        // AssemblyName depends on the order staying the same
        private int _Major;
        private int _Minor;
        private int _Build = -1;
        private int _Revision = -1;
    
        public VersionRW(int major, int minor, int build = -1, int revision = -1) 
        {
            if (major < 0) throw new ArgumentOutOfRangeException("major");
            if (minor < 0) throw new ArgumentOutOfRangeException("minor");
            if (build < -1) throw new ArgumentOutOfRangeException("build");
            if (revision < -1) throw new ArgumentOutOfRangeException("revision");
            
            _Major = major;
            _Minor = minor;
            _Build = build;
            _Revision = revision;
        }

        public VersionRW(String version) 
        {
            Version v = Version.Parse(version);
            _Major = v.Major;
            _Minor = v.Minor;
            _Build = v.Build;
            _Revision = v.Revision;
        }

        public VersionRW() 
        {
            _Major = 0;
            _Minor = 0;
        }

        public VersionRW(Version v)
        {
            _Major = v.Major;
            _Minor = v.Minor;
            _Build = v.Build;
            _Revision = v.Revision;
        }

        // Properties for setting and getting version numbers
        [XmlAttribute] public int Major { get { return _Major; } set { _Major = value; } }
        [XmlAttribute] public int Minor { get { return _Minor; } set { _Minor = value; } }
        [XmlAttribute] public int Build { get { return _Build; } set { _Build = value; } }
        [XmlAttribute] public int Revision { get { return _Revision; } set { _Revision = value; } }
     
        public Object Clone()
        {
            VersionRW v = new VersionRW();
            v._Major = _Major;
            v._Minor = _Minor;
            v._Build = _Build;
            v._Revision = _Revision;
            return(v);
        }

        public int CompareTo(Object version)
        {
            if (version is VersionRW)
            {
                VersionRW v = version as VersionRW;
                if (this._Major != v._Major) return this._Major > v._Major ? 1 : -1;
                if (this._Minor != v._Minor) return this._Minor > v._Minor ? 1 : -1;
                if (this._Build != v._Build) return this._Build > v._Build ? 1 : -1;
                if (this._Revision != v._Revision) return this._Revision > v._Revision ? 1 : -1;
                return 0;
            }
            if (version is Version)
            {
                Version v = version as Version;
                if (this._Major != v.Major) return this._Major > v.Major ? 1 : -1;
                if (this._Minor != v.Minor) return this._Minor > v.Minor ? 1 : -1;
                if (this._Build != v.Build) return this._Build > v.Build ? 1 : -1;
                if (this._Revision != v.Revision) return this._Revision > v.Revision ? 1 : -1;
                return 0;
            }
            return 1;
        }

        public int CompareTo(VersionRW v)
        {
            if (this._Major != v._Major) return this._Major > v._Major ? 1 : -1;
            if (this._Minor != v._Minor) return this._Minor > v._Minor ? 1 : -1;
            if (this._Build != v._Build) return this._Build > v._Build ? 1 : -1;
            if (this._Revision != v._Revision) return this._Revision > v._Revision ? 1 : -1;
            return 0;
        }

        public int CompareTo(Version v)
        {
            if (this._Major != v.Major) return this._Major > v.Major ? 1 : -1;
            if (this._Minor != v.Minor) return this._Minor > v.Minor ? 1 : -1;
            if (this._Build != v.Build) return this._Build > v.Build ? 1 : -1;
            if (this._Revision != v.Revision) return this._Revision > v.Revision ? 1 : -1;
            return 0;
        }

        public override bool Equals(Object obj)
        {
            if (obj is VersionRW)
            {
                VersionRW v = obj as VersionRW;
                return ((this._Major == v._Major) &&
                        (this._Minor == v._Minor) &&
                        (this._Build == v._Build) &&
                        (this._Revision == v._Revision));
            }

            if (obj is Version)
            {
                Version v = obj as Version;
                return ((this._Major == v.Major) &&
                        (this._Minor == v.Minor) &&
                        (this._Build == v.Build) &&
                        (this._Revision == v.Revision));
            }

            return false;
        }

        public bool Equals(VersionRW v)
        {
            if (v == (VersionRW)null) return false;
            return ((this._Major == v._Major) &&
                    (this._Minor == v._Minor) &&
                    (this._Build == v._Build) &&
                    (this._Revision == v._Revision));
        }

        public bool Equals(Version v)
        {
            if (v == (Version)null) return false;
            return ((this._Major == v.Major) &&
                    (this._Minor == v.Minor) &&
                    (this._Build == v.Build) &&
                    (this._Revision == v.Revision));
        }

        public override int GetHashCode()
        {
            // Let's assume that most version numbers will be pretty small and just
            // OR some lower order bits together.

            int accumulator = 0;

            accumulator |= (this._Major & 0x0000000F) << 28;
            accumulator |= (this._Minor & 0x000000FF) << 20;
            accumulator |= (this._Build & 0x000000FF) << 12;
            accumulator |= (this._Revision & 0x00000FFF);

            return accumulator;
        }

        public override String ToString() 
        {
            var sb = new StringBuilder(23);
            sb.Append(_Major); sb.Append('.');
            sb.Append(_Minor); sb.Append('.');
            if (_Build == -1) { sb.Length -= 1; return sb.ToString(); }
            sb.Append(_Build); sb.Append('.');
            if (_Revision == -1) { sb.Length -= 1; return sb.ToString(); }
            sb.Append(_Revision);
            return sb.ToString();
        }

        public static VersionRW Parse(string input) 
        {
            return new VersionRW(Version.Parse(input));
        }

        public static bool TryParse(string input, out VersionRW result) 
        {
            Version v;
            if (Version.TryParse(input, out v)) { result = new VersionRW(v); return true; }
            result = new VersionRW(0, 0, 0, 0);
            return false;
        }

        public static bool operator ==(VersionRW v1, VersionRW v2) { return v1.Equals(v2); }
        public static bool operator ==(VersionRW v1, Version v2) { return v1.Equals(v2); }
        public static bool operator ==(Version v1, VersionRW v2) { return v2.Equals(v1); }

        public static bool operator !=(VersionRW v1, VersionRW v2) { return !v1.Equals(v2); }
        public static bool operator !=(VersionRW v1, Version v2) { return !v1.Equals(v2); }
        public static bool operator !=(Version v1, VersionRW v2) { return !v2.Equals(v1); }

        public static bool operator <(VersionRW v1, VersionRW v2) { return (v1.CompareTo(v2) < 0); }
        public static bool operator <=(VersionRW v1, VersionRW v2) { return (v1.CompareTo(v2) <= 0); }
        public static bool operator >(VersionRW v1, VersionRW v2) { return (v2 < v1); }
        public static bool operator >=(VersionRW v1, VersionRW v2) { return (v2 <= v1); }

        public static implicit operator Version(VersionRW v)
        {
            if (v.Build == -1) return new Version(v.Major, v.Minor);
            if (v.Revision == -1) return new Version(v.Major, v.Minor, v.Build);
            return new Version(v.Major, v.Minor, v.Build, v.Revision);
        }
        public static implicit operator VersionRW(Version v) { return new VersionRW(v); }
    }
}
