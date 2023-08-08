using System.IO;
using System.Linq;

using Nuke.Common;
using Nuke.Common.IO;

interface IHazTemplate : INukeBuild {
    string TemplateName => "RevitPluginTemplate";
    AbsolutePath TemplateDirectory => RootDirectory / ".github" / "templates" / TemplateName;

    // https://learn.microsoft.com/en-us/dotnet/standard/io/how-to-copy-directories
    void CopyDirectory(AbsolutePath sourceDir, AbsolutePath targetDir, bool recursive = true) {
        // Check if the source directory exists
        if(!sourceDir.Exists())
            throw new DirectoryNotFoundException($"Source directory not found: {sourceDir}");

        // Cache directories before we start copying
        AbsolutePath[] children = sourceDir.GetDirectories().ToArray();

        // Create the destination directory
        targetDir = UpdateName(targetDir).CreateDirectory();

        // Get the files in the source directory and copy to the destination directory
        foreach(AbsolutePath file in sourceDir.GetFiles()) {
            AbsolutePath targetFilePath = UpdateName(targetDir / file.Name);

            string content = file.ReadAllText()
                .Replace(TemplateName, this.From<IHazPluginName>().PluginName);

            targetFilePath.WriteAllText(content);
        }

        // If recursive and copying subdirectories, recursively call this method
        if(recursive) {
            foreach(AbsolutePath childDir in children) {
                CopyDirectory(childDir, targetDir / childDir.Name);
            }
        }
    }

    AbsolutePath UpdateName(AbsolutePath target) {
        string targetName = target.Name
            .Replace(TemplateName, this.From<IHazPluginName>().PluginName);
        return target.Parent / targetName;
    }
}