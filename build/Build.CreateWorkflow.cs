using System;
using System.Text.RegularExpressions;

using Nuke.Common;
using Nuke.Common.IO;
using Nuke.Components;

using Serilog;

partial class Build {
    Target CreateWorkflow => _ => _
        .Requires(() => PluginName)
        .OnlyWhenDynamic(() => !Params.PluginWorkflowFile.FileExists(), $"Plugin workflow file does exists.")
        .Executes(() => {
            Log.Debug("TemplateFile: {TemplateFile}", Params.TemplateWorkflowFile);
            Log.Debug("PluginScriptFile: {PluginScriptFile}", Params.PluginWorkflowFile);

            bool useDevExpress = string.Equals(Params.PluginType?.ToString(),
                PluginType.DevExpress.ToString(), StringComparison.OrdinalIgnoreCase);

            string content = Params.TemplateWorkflowFile.ReadAllText()
                .Replace("${{ gen.plugin_name }}", PluginName);

            // Параметр use-devexpress указывается только у плагинов с DevExpress,
            // у остальных строка удаляется, чтобы в workflow работало значение по умолчанию.
            content = useDevExpress
                ? content.Replace("${{ gen.use_devexpress }}", "true")
                : Regex.Replace(content, @"^.*\$\{\{ gen\.use_devexpress \}\}.*\r?\n", string.Empty,
                    RegexOptions.Multiline);

            Params.PluginWorkflowFile.WriteAllText(content);
        });
}
