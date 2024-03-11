using Nuke.Common;
using Nuke.Common.IO;
using Nuke.Components;

using Serilog;

partial class Build {
    Target CreateScript => _ => _
        .Requires(() => PluginName)
        .Requires(() => PublishDirectory)
        .OnlyWhenDynamic(() => false, "Skipped not support.")
        .OnlyWhenDynamic(() => !Params.PluginWorkflowFile.FileExists(), $"Plugin workflow file does exists.")
        .Executes(() => {
            Log.Debug("TemplateFile: {TemplateFile}", Params.TemplateWorkflowFile);
            Log.Debug("PluginScriptFile: {PluginScriptFile}", Params.PluginWorkflowFile);

            string content = Params.TemplateWorkflowFile.ReadAllText()
                .Replace("${{ gen.plugin_name }}", PluginName);
            Params.PluginWorkflowFile.WriteAllText(content);
        });
}
