using Nuke.Common;
using Nuke.Common.IO;

interface IPluginParams : INukeBuild {
    AbsolutePath SourceDirectory => RootDirectory / PluginName;
    [Parameter("PluginName")] string PluginName => TryGetValue(() => PluginName);
}