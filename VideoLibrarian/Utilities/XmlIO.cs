//--------------------------------------------------------------------------
// <summary>
//   
// </summary>
// <copyright file="XmlIO.cs" company="Chuck Hill">
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
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Serialization;

namespace VideoLibrarian
{
    public static class XmlIO
    {
        /// <summary>
        /// Serialize object to xml file. Supports the custom attribute XmlComment 
        /// to associate one or more xml comments with a field or property.
        /// For managing xml member order, use "[XmlElement(Order=1)]" where order >= 1.
        /// </summary>
        /// <param name="obj">Object to serialize</param>
        /// <param name="path">Filename to write xml result to.</param>
        public static void Serialize(object obj, string path)
        {
            Type type = obj.GetType();
            try
            {
                using (var fs = File.Open(path,FileMode.Create,FileAccess.ReadWrite,FileShare.Read))
                {
                    new XmlSerializer(type).Serialize(fs, obj);
                    InsertComments(type, fs);
                }
            }
            catch (Exception ex)
            {
                throw new IOException(string.Format("Unable to save {0}\r\n{1}", path, ex.Message), ex);
            }
        }

        /// <summary>
        /// Deserialize xml file to typed object. Exceptions are reformatted 
        /// to be friendlier for debugging XML format errors.
        /// </summary>
        /// <typeparam name="T">Type to deserialize into.</typeparam>
        /// <param name="path">Source xml file</param>
        /// <returns>Deserialized object</returns>
        public static T Deserialize<T>(string path) where T : new()
        {
            if (!FileEx.Exists(path)) return new T();
            var xs = new XmlSerializer(typeof(T));
            try
            {
                var settings = new XmlReaderSettings();
                settings.CloseInput = true;
                settings.IgnoreComments = true;
                settings.IgnoreProcessingInstructions = true;
                settings.IgnoreWhitespace = true;
                using (var reader = XmlReader.Create(path, settings))
                {
                    var sd = (T)xs.Deserialize(reader);
                    return sd;
                }
            }
            catch (Exception ex)
            {
                string emsg = ex.GetBaseException().Message;

                string position = string.Empty;
                MatchCollection mc = Regex.Matches(ex.Message, @"\((?<R>[0-9]+),\s*(?<C>[0-9]+)\)");
                if (mc.Count > 0) position = string.Format(", Row={0}, Column={1}.", mc[0].Groups["R"].Value, mc[0].Groups["C"].Value);
                emsg = string.Format("Unable to parse the {0} XML.\r\n{1}\r\nFile: {2}{3}",typeof(T).Name, emsg, path, position);
                throw new FormatException(emsg, ex);
            }
        }

        /// <summary>
        ///   Insert xml comments into XmlSerialized output.
        /// </summary>
        /// <remarks>
        ///   XmlSerializer does not natively emit Xml comments. The official MS mechanism is to 
        ///   implement IXmlSerializable's WriterXml() and ReaderXml() on the class to be serialized. 
        ///   This is really ugly! So we implement our own post processing via XDoc.
        /// 
        ///   For managing xml member order use "[XmlElement(Order=1)]" where order >= 1.
        /// </remarks>
        /// <param name="t">Type of object that was serialized</param>
        /// <param name="stream">Open read/writable XML serialized stream.</param>
        private static void InsertComments(Type t, Stream stream)
        {
            XmlDocument xdoc = null;

            if (stream == null) throw new ArgumentNullException("InsertComments:Stream cannot be null");
            if (!stream.CanRead || !stream.CanSeek || !stream.CanWrite) throw new UnauthorizedAccessException("InsertComments:Stream must be read/write/seek accessable.");

            var members = t.GetMembers();
            foreach (var mi in members)
            {
                if (mi.MemberType != MemberTypes.Field && mi.MemberType != MemberTypes.Property) continue;
                if (mi.CustomAttributes.Any(m => m.AttributeType == typeof(XmlIgnoreAttribute))) continue;

                foreach (var comment in XmlCommentAttribute.GetComments(mi))
                {
                    if (xdoc == null) 
                    { 
                        stream.Position = 0;
                        xdoc = new XmlDocument(); 
                        xdoc.Load(stream);
                    }
                    var node = xdoc.GetElementsByTagName(mi.Name)[0];
                    node.ParentNode.InsertBefore(xdoc.CreateComment(comment), node);
                }
            }

            if (xdoc != null) 
            { 
                stream.Position = 0; 
                xdoc.Save(stream); 
            }
        }
    }

    #region public class XmlCommentAttribute : Attribute
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = true)]
    public class XmlCommentAttribute : Attribute
    {
        public XmlCommentAttribute() { }
        public XmlCommentAttribute(int lineNum, string value) { this.LineNum = lineNum; this.Value = value; }
        public XmlCommentAttribute(string value) { this.LineNum = 0; this.Value = value; }

        /// <summary>
        /// The comment string.
        /// </summary>
        public string Value { get; set; }

        /// <summary>
        /// Attributes are not stored in any particular order so we must assign a line number to each comment.
        /// </summary>
        public int LineNum { get; set; }

        /// <summary>
        /// Handy utility to retrieve all the ordered comment strings.
        /// </summary>
        /// <param name="mi">Field or property member info</param>
        /// <returns>Simple array or ordered comments for this property</returns>
        public static IEnumerable<string> GetComments(MemberInfo mi)
        {
            return (from p in mi.GetCustomAttributes(typeof(XmlCommentAttribute)) as IEnumerable<XmlCommentAttribute>
                    orderby p.LineNum ascending
                    select p.Value);
        }
    }
    #endregion
}
