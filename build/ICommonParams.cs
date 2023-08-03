using System;
using System.Collections.Generic;
using System.IO;

using Nuke.Common;
using Nuke.Common.IO;
using Nuke.Common.ProjectModel;
using Nuke.Common.Tools.GitVersion;

interface ICommonParams : INukeBuild {
    AbsolutePath AppDataPath => Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
    IEnumerable<RevitConfiguration> DebugConfigurations => RevitConfiguration.GetDebugConfiguration();
    IEnumerable<RevitConfiguration> ReleaseConfigurations => RevitConfiguration.GetReleaseConfiguration();
    
    [Solution("RevitPlugins.sln")] Solution Solution => TryGetValue(() => Solution);
    [GitVersion] GitVersion GitVersion => TryGetValue(() => GitVersion);
    [Parameter("Output")] AbsolutePath Output => TryGetValue(() => Output) ?? RootDirectory / "bin";
    
    AbsolutePath GetOutput(string pluginTab) {
        return Path.IsPathRooted(pluginTab) ? pluginTab : (AppDataPath / pluginTab);
    }
}