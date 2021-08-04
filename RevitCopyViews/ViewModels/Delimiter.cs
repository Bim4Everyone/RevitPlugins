using System;
using System.Linq;
using System.Text.RegularExpressions;

namespace RevitCopyViews.ViewModels {
    internal class Delimiter {
        public string Value { get; set; }
        public string DisplayValue { get; set; }

        public SplittedViewName SplitViewName(string originalName, SplitViewOptions splitViewOptions) {
            if(string.IsNullOrEmpty(originalName)) {
                throw new ArgumentException($"'{nameof(originalName)}' cannot be null or empty.", nameof(originalName));
            }

            if(splitViewOptions is null) {
                throw new ArgumentNullException(nameof(splitViewOptions));
            }

            string viewName = originalName;
            if(originalName.StartsWith("{") && originalName.EndsWith("}")) {
                return new SplittedViewName() { ViewName = originalName };
            }

            string elevations = GetElevations(originalName)?.Trim().Trim('_');
            if(!string.IsNullOrEmpty(elevations)) {
                originalName = originalName.Replace(elevations ?? string.Empty, string.Empty);
            }

            string prefix = splitViewOptions.ReplacePrefix ? null : GetPrefix(originalName)?.Trim().Trim('_');
            if(!string.IsNullOrEmpty(prefix)) {
                originalName = originalName.Replace(prefix ?? string.Empty, string.Empty);
            }

            string suffix = splitViewOptions.ReplaceSuffix ? null : GetSuffix(originalName)?.Trim().Trim('_');
            if(!string.IsNullOrEmpty(suffix)) {
                originalName = originalName.Replace(suffix ?? string.Empty, string.Empty);
            }

            if(string.IsNullOrEmpty(originalName.Trim().Trim('_'))) {
                return new SplittedViewName() { Prefix = null, ViewName = viewName, Suffix = null, Elevations = null };
            }

            viewName = originalName.Trim().Trim('_');
            return new SplittedViewName() { Prefix = prefix, ViewName = viewName, Suffix = suffix, Elevations = elevations };
        }

        private string GetPrefix(string originalName) {
            if(Value.Equals(" ")) {
                return GetPrefixSpace(originalName);
            } else if(Value.Equals("_")) {
                return GetPrefixUnderscore(originalName);
            }

            throw new NotSupportedException($"Данный вид разделителя \"{DisplayValue}\" не поддерживается");
        }

        private string GetPrefixSpace(string originalName) {
            int index = originalName.IndexOf(Value);
            if(index >= 0) {
                return originalName.Substring(0, index);
            }

            return null;
        }

        private string GetPrefixUnderscore(string originalName) {
            var splitted = originalName.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            int index = splitted.Length == 1 ? splitted[0].IndexOf(Value) : splitted[0].LastIndexOf(Value);
            if(index >= 0) {
                return originalName.Substring(0, index);
            }

            return null;
        }

        private string GetElevations(string originalName) {
            var match = Regex.Match(originalName, $@"{Regex.Escape(Value)}([+-]?\d+\.\d{{3}}){Regex.Escape(Value)}?");
            return match.Success ? match.Groups[1].Value : null;
        }

        private string GetSuffix(string originalName) {
            if((originalName.Split(new string[] { Value }, StringSplitOptions.RemoveEmptyEntries).Length - 1) > 1) {
                int index = originalName.LastIndexOf(Value);
                if(index >= 0) {
                    string suffix = originalName.Substring(index, originalName.Length - index);
                    string elevations = GetElevations(originalName);
                    return !string.IsNullOrEmpty(elevations) ? suffix.Replace(elevations, string.Empty) : suffix;
                }
            }
            return null;
        }

        public override string ToString() {
            return DisplayValue;
        }
    }

    internal class SplitViewOptions {
        public bool ReplacePrefix { get; set; }
        public bool ReplaceSuffix { get; set; }
    }

    internal class SplittedViewName {
        public string Prefix { get; set; }
        public string ViewName { get; set; }
        public string Elevations { get; set; }
        public string Suffix { get; set; }
    }
}