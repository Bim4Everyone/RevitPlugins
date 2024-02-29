using System;
using System.IO;

using Nuke.Common;
using Nuke.Common.IO;

using Serilog;

using static Nuke.Common.Tools.PowerShell.PowerShellTasks;

interface IPyRevitInstall : INukeBuild {
    [Parameter($"Default is pyRevit_4.8.14.24016_signed.exe")]
    Uri PyRevitInstallerUrl => TryGetValue(() => PyRevitInstallerUrl) ?? new Uri("https://github.com/eirannejad/pyRevit/releases/download/v4.8.14.24016%2B1909/pyRevit_4.8.14.24016_signed.exe");

    AbsolutePath PyRevitPath
        => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "pyRevit-Master");

    Target InstallPyRevit => _ => _
        .Requires(() => PyRevitInstallerUrl)
        .OnlyWhenDynamic(() => !PyRevitPath.DirectoryExists(), $"{PyRevitPath} must not exist")
        .Executes(() => {
            Log.Debug("Test powershell.exe on github actions via ls command");
            PowerShell("ls");
            string pyRevitInstallerFile = Path.Combine(RootDirectory, "bin", "pyRevitInstaller.exe");
            Log.Debug($"Download pyRevit installer from: {PyRevitInstallerUrl} to: {pyRevitInstallerFile}");
            PowerShell($"curl.exe -L \"{PyRevitInstallerUrl}\" -o \"{pyRevitInstallerFile}\" -s");
            Assert.FileExists(pyRevitInstallerFile, "pyRevitInstaller didn't download");

            Log.Debug($"Run pyRevit installer from: {pyRevitInstallerFile}");
            PowerShell($"start \"{pyRevitInstallerFile}\" /silent -Wait");

            string appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            string pyRevitLabsJsonDllPath = Path.Combine(appData, @"pyRevit-Master\bin\pyRevitLabs.Json.dll");
            Assert.FileExists(pyRevitLabsJsonDllPath, "pyRevitLabs.Json.dll didn't install");
        })
    ;
}
