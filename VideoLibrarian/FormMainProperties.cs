//--------------------------------------------------------------------------
// <summary>
//   
// </summary>
// <copyright file="FormMainProperties.cs" company="Chuck Hill">
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
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Windows.Forms;
using System.Xml;
using System.Xml.Serialization;

namespace VideoLibrarian
{
    [XmlRoot("StateData")]
    [XmlInclude(typeof(ViewType))]
    public class FormMainProperties
    {
        private static readonly string FileName = Path.ChangeExtension(Process.GetCurrentProcess().MainModule.FileName, "SavedState.xml");

        [XmlElement(Order=1)] public VersionRW Version { get; set; }  //needed for future upgrades
        [XmlElement(Order=2)] public int DPIScaling { get; set; }     //if DPI scaling has changed.
        [XmlElement(Order=3)] public RECT DesktopBounds;

        [XmlElement(Order=4)] public SettingProperties Settings { get; set; }
        [XmlElement(Order=5)] public ViewType View { get; set; }
        [XmlElement(Order=6)] public string SortKey { get; set; }
        [XmlElement(Order=7)] public FilterProperties Filters { get; set; }
        [XmlElement(Order=8)] public int ScrollPosition { get; set; }
        [XmlComment("Explicit number of properties to load. For debugging use only.")]
        [XmlElement(Order=9)] public int MaxLoadedProperties { get; set; }

        public FormMainProperties()
        {
            Form owner = System.Windows.Forms.Form.ActiveForm;
            if (owner == null)
            {
                FormCollection fc = System.Windows.Forms.Application.OpenForms;
                if (fc != null && fc.Count > 0) owner = fc[0];
            }
            if (owner != null) this.DesktopBounds = owner.DesktopBounds;

            this.DPIScaling = GDI.DpiScalingFactor();
            this.Version = Assembly.GetExecutingAssembly().GetName().Version;
            this.Settings = new SettingProperties() { MediaFolders = new string[0] };
            this.View = ViewType.SmallTiles;
            this.SortKey = string.Empty;
            //this.Filters = new FilterProperties();
            this.ScrollPosition = 0;
            this.MaxLoadedProperties = 0;
        }

        public void Serialize()
        {
            XmlIO.Serialize(this, FileName);
        }

        public static FormMainProperties Deserialize()
        {
            return XmlIO.Deserialize<FormMainProperties>(FileName);
        }
    }
}
