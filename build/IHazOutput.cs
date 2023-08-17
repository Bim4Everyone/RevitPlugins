using Nuke.Common;
using Nuke.Common.IO;

interface IHazOutput : INukeBuild {
    [Parameter($"Output folder, default value is \"RootDirectory/bin\"")]
    AbsolutePath Output => TryGetValue(() => Output) ?? DefaultOutput;
    [Parameter] AbsolutePath PublishDirectory => TryGetValue(() => PublishDirectory) ?? DefaultOutput;
    
    AbsolutePath DefaultOutput => RootDirectory / "bin";
    
}