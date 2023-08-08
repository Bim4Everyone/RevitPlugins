using Nuke.Common;
using Nuke.Common.IO;

interface IHazTemplate : INukeBuild {
    string TemplateName => "RevitPluginTemplate";
    AbsolutePath TemplateDirectory => RootDirectory / ".github" / "templates" / TemplateName;
}