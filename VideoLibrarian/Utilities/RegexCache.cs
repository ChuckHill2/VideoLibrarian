//--------------------------------------------------------------------------
// <summary>
//   
// </summary>
// <copyright file="RegexCache.cs" company="Chuck Hill">
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
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text.RegularExpressions;

namespace VideoLibrarian
{
    // This cache was created to avoid creating isolated static regex expressions everywhere and because VideoLibrarian uses roughly
    // 30 regex patterns, repeated compilation can take a significant amount of time.

    /// <summary>
    /// Thread-safe cache of compiled-upon-demand regex patterns.
    /// Cached Regex's may be saved and subsequently restored upon restart of the application. (see RegexCache.CompileToAssembly)
    /// </summary>
    public static class RegexCache
    {
        private static readonly ConcurrentDictionary<string, Regex> Cache = new ConcurrentDictionary<string, Regex>();
        private static bool PatternAddedToCache = false; //flag to determine if CompileToAssembly() may be called. If false CompileToAssembly() does nothing.

        /// <summary>
        /// Load our pre-compiled regex library created by CompileToAssembly(), if it exists.
        /// </summary>
        static RegexCache()
        {
            string dllpath = Path.Combine(Path.GetDirectoryName(Process.GetCurrentProcess().MainModule.FileName), "RegexLib.dll");

            if (!File.Exists(dllpath)) return;
            try
            {
                Assembly asm = Assembly.Load(File.ReadAllBytes(dllpath)); //loaded this way to not lock the file, so file may be overwritten by CompileToAssembly(). 

                foreach (var t in asm.GetTypes().Where(tt => tt.IsPublic && tt.IsClass))
                {
                    UpdateRegexCacheSize();
                    var re = (Regex)Activator.CreateInstance(t);
                    var key = GetKey(re.ToString(), re.Options);
                    Cache.TryAdd(key, re);
                }
            }
            catch (Exception)
            {
                //if anything fails, then this is not a compiled regex library dll.
            }
        }

        /// <summary>
        /// Get existing or if not exists in cache, a new Regex object.
        /// </summary>
        /// <param name="pattern">
        ///    The regular expression pattern to match. This should only be const strings as it
        ///    would be unwise to add dynamically composed patterns that change a lot.
        ///    That would just plug up the cache with single use patterns.
        /// </param>
        /// <param name="options">A bitwise combination of additional enumeration values that modify the regular expression. Default=none.</param>
        /// <returns>An immutable regular expression object.</returns>
        /// <remarks>
        /// RegexOptions.Compiled is built-in and does not need to be specified.
        /// </remarks>
        public static Regex RegEx(string pattern, RegexOptions options = RegexOptions.None)
        {
            if (string.IsNullOrEmpty(pattern)) throw new ArgumentNullException(nameof(pattern), "Specified pattern is null or empty.");

            options |= RegexOptions.Compiled;

            var key = GetKey(pattern, options);
            Regex re = null;
            if (!Cache.TryGetValue(key, out re))
            {
                UpdateRegexCacheSize();
                re = new Regex(pattern, options); //This may throw exception for bad pattern.
                Cache.TryAdd(key, re);
                PatternAddedToCache = true;
            }

            return re;
        }

        /// <summary>
        /// Compiles the currently cached regex patterns into an assembly. The regexes now do not have to be recompiled at startup.
        /// </summary>
        /// <remarks>
        /// There is no advantage to using a static Regex object (with RegexOptions.Compiled) vs running from a regex assembly.
        /// The only gain is that the regex object is pre-compiled at startup. This feature will disappear on .NET5+ and .NET Core.
        /// </remarks>
        public static void CompileToAssembly()
        {
            //https://docs.microsoft.com/en-us/dotnet/api/system.text.regularexpressions.regex.compiletoassembly?view=netframework-4.8
            //https://docs.microsoft.com/en-us/dotnet/standard/base-types/best-practices
            //https://stackoverflow.com/questions/39470506/c-sharp-regex-performance-very-slow

            if (Cache.Count == 0 || !PatternAddedToCache) return;
            RegexCompilationInfo[] compilationArray = new RegexCompilationInfo[Cache.Count];
            int i = 0;
            foreach (var re in Cache)
            {
                compilationArray[i++] = new RegexCompilationInfo(re.Value.ToString(),
                 re.Value.Options,
                 i.ToString("'RE'0000"),
                 "Utilities.RegularExpressions",
                 true);
            }

            var version = Assembly.GetExecutingAssembly().GetName().Version.ToString();
            AssemblyName asmName = new AssemblyName($"RegexLib, Version={version}, Culture=neutral, PublicKeyToken=null");
            Regex.CompileToAssembly(compilationArray, asmName, GetCustomAttributes());
        }

        /// <summary>
        /// Retrieve the current executable assembly attributes to copy to new regex assembly.
        /// </summary>
        /// <returns></returns>
        private static CustomAttributeBuilder[] GetCustomAttributes()
        {
            Assembly thisAsm = Assembly.GetEntryAssembly();
            var attrTypes = new Type[]
            {
                typeof(AssemblyCompanyAttribute),
                //typeof(AssemblyConfigurationAttribute),
                typeof(AssemblyCopyrightAttribute),
                typeof(AssemblyCultureAttribute),
                typeof(AssemblyDescriptionAttribute),
                typeof(AssemblyFileVersionAttribute),
                typeof(AssemblyInformationalVersionAttribute),
                typeof(AssemblyProductAttribute),
                typeof(AssemblyTitleAttribute),
                typeof(AssemblyTrademarkAttribute),
                typeof(AssemblyVersionAttribute),
            };
            var attBuilder = new List<CustomAttributeBuilder>();

            const string description = "General-purpose library of compiled regular expressions";
            var constructorParamtypes = new Type[] { typeof(string) };
            foreach (var t in attrTypes)
            {
                var v = GetAttributeValue(thisAsm, t);
                if (string.IsNullOrEmpty(v) && t != typeof(AssemblyDescriptionAttribute)) continue;
                if (t == typeof(AssemblyDescriptionAttribute)) v = string.IsNullOrEmpty(v) ? description : description+"\r\n" + v; 
                ConstructorInfo ctor = t.GetConstructor(new Type[] { typeof(string) });
                attBuilder.Add(new CustomAttributeBuilder(ctor, new object[]{ v }));
            }

            return attBuilder.ToArray();
        }

        /// <summary>
        /// Retrieve attribute's constructor value.
        /// </summary>
        /// <param name="asm"></param>
        /// <param name="attrType"></param>
        /// <returns></returns>
        private static string GetAttributeValue(Assembly asm, Type attrType)
        {
            foreach (var data in asm.CustomAttributes)
            {
                if (attrType != data.AttributeType) continue;
                if (data.ConstructorArguments.Count > 0) return data.ConstructorArguments[0].Value.ToString();
                break;
            }
            return string.Empty;
        }

        private static void UpdateRegexCacheSize()
        {
            //Regex will throw out compiled regexes that exceed its internal cache size. We don't want to do that.
            //We expect a lot more Regex's than the default of 15.
            if ((Cache.Count + 1) > (Regex.CacheSize - 15))
            {
                Regex.CacheSize += 15;
            }
        }

        private static string GetKey(string pattern, RegexOptions options)
        {
            string key = string.Concat(((int)options).ToString((IFormatProvider)System.Globalization.NumberFormatInfo.InvariantInfo), ":", pattern);
            //uint key = unchecked((uint)pattern.GetHashCode());  //GetHashCode() is performed iternal to dictionry anyway, so this provides no optimization.
            return key;
        }
    }
}
