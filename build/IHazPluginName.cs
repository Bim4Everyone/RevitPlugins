using Nuke.Common;
using Nuke.Common.IO;

interface IHazPluginName : INukeBuild {
    AbsolutePath PluginDirectory => RootDirectory / PluginName;
    [Parameter("Project plugin name in solution")] string PluginName => TryGetValue(() => PluginName);
}