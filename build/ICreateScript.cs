using Nuke.Common;
using Nuke.Common.IO;

using Serilog;

[ParameterPrefix(nameof(ICreateScript))]
interface ICreateScript : IHazOutput, IHazPluginName {
    AbsolutePath TemplateFile => RootDirectory / ".github" / "templates" / "default.yml";
    AbsolutePath PluginScriptFile => RootDirectory / ".github" / "workflows" / $"{PluginName}.yml";

    Target CreateScript => _ => _
        .Requires(() => Output)
        .Requires(() => PluginName)
        .Executes(() => {
            Log.Debug("TemplateFile: {TemplateFile}", TemplateFile);
            Log.Debug("PluginScriptFile: {PluginScriptFile}", PluginScriptFile);
            
            string content = TemplateFile.ReadAllText()
                .Replace("${{ gen.output }}", Output)
                .Replace("${{ gen.plugin_name }}", PluginName);
            PluginScriptFile.WriteAllText(content);
        });
}