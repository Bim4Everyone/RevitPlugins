using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace RevitLintelsManager.Services;

internal static class SettingsFileTransaction {
    public static void Execute(IEnumerable<string> paths, Action save) {
        var backups = paths.Select(Path.GetFullPath).Distinct(StringComparer.OrdinalIgnoreCase)
            .ToDictionary(path => path, path => File.Exists(path) ? File.ReadAllBytes(path) : null);
        try {
            save();
        } catch(Exception saveException) {
            var errors = new List<Exception> { saveException };
            foreach(var backup in backups) {
                try {
                    if(backup.Value is null) {
                        if(File.Exists(backup.Key)) {
                            File.Delete(backup.Key);
                        }
                    } else {
                        File.WriteAllBytes(backup.Key, backup.Value);
                    }
                } catch(Exception rollbackException) {
                    errors.Add(rollbackException);
                }
            }
            if(errors.Count > 1) {
                throw new AggregateException(errors);
            }
            throw;
        }
    }
}
