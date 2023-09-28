using Nuke.Common;
using Nuke.Common.IO;
using Nuke.Components;

using Serilog;

[ParameterPrefix(nameof(ICreateScript))]
interface ICreateScript : IHazOutput, IHazPluginName, IHazSolution {
    AbsolutePath TemplateFile => RootDirectory / ".github" / "templates" / "default.yml";
    AbsolutePath PluginScriptFile => RootDirectory / ".github" / "workflows" / $"{PluginName}.yml";

    Target CreateScript => _ => _
        .Requires(() => PluginName)
        .Requires(() => PublishDirectory)
        .OnlyWhenDynamic(() => false, "Skipped not support.")
        .OnlyWhenDynamic(() => !PluginScriptFile.FileExists(), $"Plugin script file does exists.")
        .Executes(() => {
            Log.Debug("TemplateFile: {TemplateFile}", TemplateFile);
            Log.Debug("PluginScriptFile: {PluginScriptFile}", PluginScriptFile);

            string content = TemplateFile.ReadAllText()
                .Replace("${{ gen.plugin_name }}", PluginName);
            PluginScriptFile.WriteAllText(content);
        });
}