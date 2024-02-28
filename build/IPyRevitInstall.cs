using System;
using System.IO;

using Nuke.Common;
using Nuke.Common.Tooling;

using Serilog;

using static Nuke.Common.Tools.PowerShell.PowerShellTasks;

interface IPyRevitInstall : INukeBuild {
    [Parameter] Uri PyRevitInstallerUrl => TryGetValue(() => PyRevitInstallerUrl);

    Target InstallPyRevit => _ => _
        .Requires(() => PyRevitInstallerUrl)
        .OnlyWhenStatic(() => !IsServerBuild, "TODO after testing make only on ServerBuild")
        .Executes(() => {
            Log.Debug($"Download pyRevit installer from: {PyRevitInstallerUrl}");

            string pyRevitInstallerFile = Path.Combine(RootDirectory, "pyRevitInstaller.exe");
            PowerShell($"curl.exe -L {PyRevitInstallerUrl} -o {pyRevitInstallerFile} -s").EnsureOnlyStd();

            Assert.FileExists(pyRevitInstallerFile, "pyRevitInstaller didn't download");

            Log.Debug($"Run pyRevit installer from: {pyRevitInstallerFile}");
            PowerShell($"start {pyRevitInstallerFile} /silent -Wait");

            string appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            string pyRevitLabsJsonDllPath = Path.Combine(appData, @"pyRevit-Master\bin\pyRevitLabs.Json.dll");
            Assert.FileExists(pyRevitLabsJsonDllPath, "pyRevitLabs.Json.dll didn't install");
        })
    ;
}
