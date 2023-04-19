using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using PlatformSettings.Factories;
using PlatformSettings.Model;

using pyRevitLabs.Json.Linq;

namespace PlatformSettings.Services {
    internal class BuiltinExtensionsService : IExtensionsService<BuiltinExtension> {
        private readonly IExtensionFactory<BuiltinExtension> _builtinFactory;

        public static readonly string ExtensionsPath =
            Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "pyRevit-Master", "extensions");

        public static readonly string ExtensionsDefinitionPath =
            Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                @"pyRevit-Master\extensions\extensions.json");

        public static readonly string TagsDefinitionPath =
            Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                @"pyRevit-Master\extensions\pyRevitTags.extension\extension.json");

        public static readonly string ToolsDefinitionPath =
            Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                @"pyRevit-Master\extensions\pyRevitTools.extension\extension.json");


        public BuiltinExtensionsService(IExtensionFactory<BuiltinExtension> builtinFactory) {
            _builtinFactory = builtinFactory;
        }

        public IEnumerable<BuiltinExtension> GetExtensions() {
            yield return _builtinFactory.Create(JToken.Parse(File.ReadAllText(TagsDefinitionPath)));
            yield return _builtinFactory.Create(JToken.Parse(File.ReadAllText(ToolsDefinitionPath)));
        }
    }
}