using System;
using System.ComponentModel;
using System.Xml.Serialization;

namespace MovieGuide
{
    public class SettingProperties
    {
        [XmlArrayItem("Folder")]
        public string[] MediaFolders { get; set; }

        [DefaultValue(100)]
        public int Zoom = 100;
    }
}
