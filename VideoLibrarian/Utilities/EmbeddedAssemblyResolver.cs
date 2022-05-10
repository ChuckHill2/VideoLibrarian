//--------------------------------------------------------------------------
// <summary>
//   
// </summary>
// <copyright file="FormMain.cs" company="Chuck Hill">
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
using System.IO;
using System.Linq;
using System.Reflection;

namespace VideoLibrarian
{
    public static class EmbeddedAssemblyResolver
    {
        /// <summary>
        /// Locate needed assemblies required during runtime of this application. Assemblies may be embedded in other loaded assemblies (included this exe).
        /// This must be called only once on the first line of Program.Main().
        /// </summary>
        public static void SetResolver(AppDomain appdomain=null)
        {
            (appdomain ?? AppDomain.CurrentDomain).AssemblyResolve += CurrentDomain_AssemblyResolve; //Load embedded assemblies
        }

        private static Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args)
        {
            //This function can ONLY use THIS assembly or assemblies from the GAC because if this routine or attendant
            //subroutines needs to use an assembly that needs to be "found" then recursion will occur!
            AssemblyName asmName = new AssemblyName(args.Name);
            Exception ex = null;
            Assembly asm = null;

            if (asmName.Name.EndsWith(".resources", StringComparison.OrdinalIgnoreCase)) return null;
            if (asmName.Name.EndsWith(".XmlSerializers", StringComparison.OrdinalIgnoreCase)) return null;

            try
            {
                var loadedAssemblies = AppDomain.CurrentDomain.GetAssemblies();
                asm = loadedAssemblies.FirstOrDefault(a => (a.IsDynamic ? false : string.Compare(Path.GetFileNameWithoutExtension(a.Location), asmName.Name, true) == 0));
                if (asm != null) return asm;

                //Dig deeper. Look for embedded assembly resource only within OUR (non-microsoft) assemblies.
                var dllname = $"{asmName.Name}.dll";
                foreach (Assembly a in loadedAssemblies)
                {
                    if (a.IsDynamic) continue;
                    if (a.Location.ContainsI("Microsoft.NET")) continue; //Exclude microsoft .net assemblies

                    var resname = a.GetManifestResourceNames().FirstOrDefault(m => m.EndsWith("." + dllname, StringComparison.InvariantCultureIgnoreCase));
                    if (resname == null) continue;

                    var fullpath = Path.Combine(Path.GetDirectoryName(a.Location), dllname);
                    using (var fs = File.OpenWrite(fullpath)) a.GetManifestResourceStream(resname).CopyTo(fs);
                    asm = Assembly.LoadFrom(fullpath);
                    break;
                }
            }
            catch (Exception e) { ex = e; }
            finally
            {
                if (asm == null)
                {
                    Log.Write(Severity.Error, "Unresolved assembly: \"{0}\"", args.Name);
                    if (ex != null) Log.Write(Severity.Error, "Assembly Resolver Error: \"{0}\"", ex);
                }
                else
                {
                    Log.Write(Severity.Verbose, "Resolved assembly: \"{0}\"", asm.IsDynamic ? asm.ToString() : asm.Location);
                }
            }
            return asm;
        }
    }
}
