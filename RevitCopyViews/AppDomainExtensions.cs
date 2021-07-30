using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace RevitCopyViews {
    internal class AppDomainExtensions {
        public static Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args) {
            var assemblyName = new AssemblyName(args.Name);
            var assemblyPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), @"pyRevit\Extensions\BIM4Everyone.lib", assemblyName.Name + ".dll");
            return File.Exists(assemblyPath) ? Assembly.LoadFrom(assemblyPath) : null;
        }
    }
}
