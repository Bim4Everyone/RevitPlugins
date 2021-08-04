using System;
using System.Linq;
using System.Text.RegularExpressions;

namespace RevitCopyViews.ViewModels {
    internal class Delimiter {
        public static readonly string Value = "_";

        public static SplittedViewName SplitViewName(string originalName, SplitViewOptions splitViewOptions) {
            if(string.IsNullOrEmpty(originalName)) {
                throw new ArgumentException($"'{nameof(originalName)}' cannot be null or empty.", nameof(originalName));
            }

            if(splitViewOptions is null) {
                throw new ArgumentNullException(nameof(splitViewOptions));
            }

            string viewName = originalName;
            viewName = originalName.Trim().Trim('_');
            return new SplittedViewName() { Prefix = "", ViewName = viewName, Suffix = "", Elevations = "" };
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