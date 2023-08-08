using Nuke.Common;
using Nuke.Common.IO;
using Nuke.Components;

using Serilog;

[ParameterPrefix(nameof(ICreateScript))]
interface ICreateScript : IHazOutput, IHazPluginName, IHazSolution {
    AbsolutePath TemplateFile => RootDirectory / ".github" / "templates" / "default.yml";
    AbsolutePath PluginScriptFile => RootDirectory / ".github" / "workflows" / $"{PluginName}.yml";

    Target CreateScript => _ => _
        .Requires(() => Output)
        .Requires(() => PluginName)
        .OnlyWhenDynamic(() => Solution.GetProject(PluginName) == null, $"Plugin \"{PluginName}\" does exists.")
        .Executes(() => {
            Log.Debug("TemplateFile: {TemplateFile}", TemplateFile);
            Log.Debug("PluginScriptFile: {PluginScriptFile}", PluginScriptFile);
            
            string content = TemplateFile.ReadAllText()
                .Replace("${{ gen.output }}", Output)
                .Replace("${{ gen.plugin_name }}", PluginName);
            PluginScriptFile.WriteAllText(content);
        });
}