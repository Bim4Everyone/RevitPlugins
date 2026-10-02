using System.IO;

namespace RevitExportSpecToJson.Utils;

internal static class FileUtils {
    public static string GetSafeFolderName(string input, string defaultFolderName = "untitled", char replacement = '_') {
        if(string.IsNullOrWhiteSpace(input)) {
            return defaultFolderName;
        }

        // 1. Get the list of OS-specific invalid characters
        char[] invalidChars = Path.GetInvalidFileNameChars();

        // 2. Replace invalid characters with your preferred replacement character
        string safeName = new string(
            [.. input.Select(c => invalidChars.Contains(c) ? replacement : c)]);

        // 3. Trim spaces
        safeName = safeName.Trim();

        // 4. Handle edge case where trimming results in an empty string
        return string.IsNullOrWhiteSpace(safeName) ? defaultFolderName : safeName;
    }
    
    public static string GetSafeFileName(string input, string defaultFileName = "untitled", char replacement = '_') {
        if(string.IsNullOrWhiteSpace(input)) {
            return defaultFileName;
        }

        // 1. Get the list of OS-specific invalid characters
        char[] invalidChars = Path.GetInvalidFileNameChars();

        // 2. Replace invalid characters with your preferred replacement character
        string safeName = new string(
            [.. input.Select(c => invalidChars.Contains(c) ? replacement : c)]);

        // 3. Trim spaces and trailing periods (Windows doesn't allow trailing dots)
        safeName = safeName.Trim().TrimEnd('.');

        // 4. Handle edge case where trimming results in an empty string
        return string.IsNullOrWhiteSpace(safeName) ? defaultFileName : safeName;
    }
}
