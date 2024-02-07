using Nuke.Common;
using Nuke.Common.IO;

interface IHazPluginName : INukeBuild {
    AbsolutePath PluginDirectory => RootDirectory / "src" / PluginName;
    [Parameter("Project plugin name in solution"), Required] string PluginName => TryGetValue(() => PluginName);
}
