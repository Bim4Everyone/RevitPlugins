
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

using Autodesk.Revit.DB;

using dosymep.Revit.ServerClient;

namespace RevitServerFolders {
    internal class Program {
        public static void Main() {
            new DetachRevitFilesCommand() {
                ServerName = "10.2.0.137",
                RevitVersion = "2020",
                FolderName = "SKUG-27",
                TargetFolderName = @"C:\Temp\Revit"
            }.Execute();
        }
    }
}
