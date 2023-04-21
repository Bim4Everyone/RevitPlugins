using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using pyRevitLabs.Json.Linq;

using RevitPlatformSettings.Factories;
using RevitPlatformSettings.Model;

namespace RevitPlatformSettings.Services {
    internal class ThirdPartyExtensionsService : IExtensionsService<ThirdPartyExtension> {
        private readonly IExtensionFactory<ThirdPartyExtension> _thirdPartyFactory;

        public static readonly string ExtensionsPath =
            Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "pyRevit", "Extensions");

        public static readonly string ExtensionsDefinitionPath =
            Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                @"pyRevit\Extensions\01.BIM.extension\extensions.json");


        public ThirdPartyExtensionsService(IExtensionFactory<ThirdPartyExtension> thirdPartyFactory) {
            _thirdPartyFactory = thirdPartyFactory;
        }

        public IEnumerable<ThirdPartyExtension> GetExtensions() {
            return JObject.Parse(File.ReadAllText(ExtensionsDefinitionPath))
                .GetValue("extensions")
                .ToObject<JToken[]>()
                .Select(item => _thirdPartyFactory.Create(item));
        }
    }
}