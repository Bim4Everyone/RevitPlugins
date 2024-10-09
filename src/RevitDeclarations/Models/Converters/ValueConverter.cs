using System.Text.RegularExpressions;

namespace RevitDeclarations.Models {
    internal class ValueConverter {
        public static int ConvertStringToInt(string value) {
            if(string.IsNullOrEmpty(value)) {
                return 0;
            } else {
                string resultString = Regex.Match(value, @"\d+").Value;
                int.TryParse(resultString, out int result);
                return result;
            }
        }
    }
}
