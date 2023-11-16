using System;
using System.IO;

using Nuke.Common;

static class NukeBuildExtensions {
    public static T From<T>(this INukeBuild nukeBuild) where T : INukeBuild {
        return (T) (object) nukeBuild;
    }

    public static string GetExtensionsPath(string extensionDirectory) {
        extensionDirectory = Environment.ExpandEnvironmentVariables(extensionDirectory);
        if(Path.IsPathRooted(extensionDirectory)) {
            return extensionDirectory;
        }

        string appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        return Path.Combine(appData, @"pyRevit\Extensions", extensionDirectory);
    }
}